#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;
using MethodBody = Mono.Cecil.Cil.MethodBody;

namespace UnityDecoratorAttribute
{
    public static partial class ILInjector
    {
        private class MethodInjector
        {
            private enum ActionType
            {
                PreAction,
                PostAction
            }
            
            private TypeDefinition type;
            private MethodDefinition targetMethod;
            private CustomAttribute attribute;
            private Type attributeType;
            private AssemblyDefinition assemblyDefinition;

            private MethodInfo preAction;
            private DecoratorAttribute.ParameterType[] enumParams;
            private Instruction newInst;
            private ILProcessor ilProcessor;
            private Instruction firstInst;

            private MethodInfo postAction;
            private Instruction lastInst;

            private BindingFlags bindingFlag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static |
                                               BindingFlags.FlattenHierarchy;

            private byte returnValueIndex;

            public MethodInjector(TypeDefinition type, MethodDefinition targetMethod, CustomAttribute attribute,
                AssemblyDefinition assemblyDefinition)
            {
                this.type = type;
                this.targetMethod = targetMethod;
                this.attribute = attribute;
                this.assemblyDefinition = assemblyDefinition;

                attributeType = attribute.AttributeType.GetMonoType();
                ilProcessor = targetMethod.Body.GetILProcessor();
                firstInst = targetMethod.Body.Instructions.First();
                lastInst = targetMethod.Body.Instructions.Last();
            }

            private bool ValidateParams(ActionType actionType)
            {
                enumParams =
                    attributeType.GetProperty($"{actionType}ParameterTypes", bindingFlag)?.GetValue(null) as
                        DecoratorAttribute.ParameterType[];

                if (enumParams == null)
                {
                    // Debug.Log($"Can't find {attribute.GetType()}::{actionType}ParameterTypes");
                    return false;
                }

                if (actionType == ActionType.PostAction)
                {
                    if (enumParams.Contains(DecoratorAttribute.ParameterType.ParameterValues))
                    {
                        Debug.LogError($"{DecoratorAttribute.ParameterType.ParameterValues} is not valid for {actionType}ParameterTypes ");
                        return false;                        
                    }
                }

                else if (actionType == ActionType.PreAction)
                {
                    if (enumParams.Contains(DecoratorAttribute.ParameterType.ReturnValue))
                    {
                        Debug.LogError($"{DecoratorAttribute.ParameterType.ReturnValue} is not valid for {actionType}ParameterTypes ");
                        return false;   
                    }
                }

                return true;
            }
            
            private MethodInfo FindValidAction(ActionType actionType)
            {
                if (enumParams == null)
                    return null;

                var expectedParamTypes = new List<Type>();
                foreach (var parameterType in enumParams)
                {
                    switch (parameterType)
                    {
                        case DecoratorAttribute.ParameterType.ClassName:
                            expectedParamTypes.Add(typeof(string));
                            break;
                        case DecoratorAttribute.ParameterType.MethodName:
                            expectedParamTypes.Add(typeof(string));
                            break;
                        case DecoratorAttribute.ParameterType.This:
                            expectedParamTypes.Add(type.GetMonoType());
                            break;
                        case DecoratorAttribute.ParameterType.ParameterValues:
                            expectedParamTypes.AddRange(targetMethod.Parameters.Select(param =>
                                param.ParameterType.GetMonoType()));
                            break;
                        case DecoratorAttribute.ParameterType.ReturnValue:
                            expectedParamTypes.Add(targetMethod.ReturnType.GetMonoType());
                            break;
                        case DecoratorAttribute.ParameterType.AttributeValues:
                            expectedParamTypes.AddRange(
                                attribute.Constructor.Parameters.Select(param => param.ParameterType.GetMonoType()));
                            break;
                    }
                }

                var methods = attributeType.GetMethods(bindingFlag);

                foreach (var method in methods)
                {
                    if (method.Name != actionType.ToString())
                        continue;
                    if (method.ReturnType != typeof(void))
                        continue;
                    var methodParamTypes = method.GetParameters().Select(param => param.ParameterType).ToArray();
                    if (methodParamTypes.Length != expectedParamTypes.Count)
                        continue;
                    bool allSame = methodParamTypes.Select((type, i) => type == expectedParamTypes[i] || 
                                                                        type.IsAssignableFrom(expectedParamTypes[i]) || 
                                                                        type.GetElementType() == expectedParamTypes[i])
                        .All(x => x);

                    if (allSame)
                    {
                        return method;
                    }
                }

                Debug.LogError($"Can't find appropriate {attributeType.Name} {actionType} for {targetMethod.Name}. " +
                          $"Expected : {actionType}({string.Join(", ", expectedParamTypes.Select(type => type.Name))})");
                return null;
            }


            private void InjectParameter(Instruction target)
            {
                int paramIndex = 0;
                foreach (var parameter in enumParams)
                {
                    switch (parameter)
                    {
                        case DecoratorAttribute.ParameterType.ClassName:
                            newInst = ilProcessor.Create(OpCodes.Ldstr, type.Name);
                            InsertBefore(target, newInst);
                            paramIndex++;
                            break;
                        case DecoratorAttribute.ParameterType.MethodName:
                            newInst = ilProcessor.Create(OpCodes.Ldstr, targetMethod.Name);
                            InsertBefore(target, newInst);
                            paramIndex++;
                            break;
                        case DecoratorAttribute.ParameterType.This:
                            newInst = ilProcessor.Create(OpCodes.Ldarg_S, (byte) 0);
                            InsertBefore(target, newInst);
                            paramIndex++;
                            break;
                        case DecoratorAttribute.ParameterType.ParameterValues:
                            paramIndex += InsertParameterValuesArguments(paramIndex);
                            break;
                        case DecoratorAttribute.ParameterType.AttributeValues:
                            paramIndex += InsertAttributeValuesArguments(target);
                            break;
                        case DecoratorAttribute.ParameterType.ReturnValue:
                            InsertReturnValueArguments(returnValueIndex, paramIndex);
                            paramIndex++;
                            break;
                    }
                }
            }

            private void InjectPreActionCall()
            {
                var preActionRef = assemblyDefinition.MainModule.ImportReference(preAction);
                newInst = ilProcessor.Create(OpCodes.Call, preActionRef);
                InsertBefore(firstInst, newInst);
            }

            private void InjectPostActionCall()
            {
                var postActionRef = assemblyDefinition.MainModule.ImportReference(postAction);
                newInst = ilProcessor.Create(OpCodes.Call, postActionRef);
                InsertBefore(lastInst, newInst);
                
            }

            private void InjectNopForCheckInjectedPreAction()
            {
                // To Check Injection, Insert 3 nop  
                for (int i = 0; i < INJECTION_NOP_COUNT; i++)
                {
                    newInst = ilProcessor.Create(OpCodes.Nop);
                    InsertBefore(firstInst, newInst);
                }
            }

            private void InjectNopForCheckInjectedPostAction()
            {
                // To Check Injection, Insert 3 nop  
                for (int i = 0; i < INJECTION_NOP_COUNT; i++)
                {
                    newInst = ilProcessor.Create(OpCodes.Nop);
                    InsertAfter(lastInst, newInst);
                }
            }

            private bool IsAlreadyInjectPreAction()
            {
                if (targetMethod.Body.Instructions.Count < INJECTION_NOP_COUNT)
                    return false;

                return targetMethod.Body.Instructions.Take(3).All(inst => inst.OpCode.Code == Code.Nop);
            }

            private bool IsAlreadyInjectPostAction()
            {
                if (targetMethod.Body.Instructions.Count < INJECTION_NOP_COUNT)
                    return false;

                return targetMethod.Body.Instructions.TakeLast(3).All(inst => inst.OpCode.Code == Code.Nop);
            }

            public void InsertPreAction()
            {
                if (!ValidateParams(ActionType.PreAction))
                    return;
                preAction = FindValidAction(ActionType.PreAction);
                if (preAction == null)
                {
                    return;
                }

                if (IsAlreadyInjectPreAction())
                    return;

                InjectNopForCheckInjectedPreAction();
                InjectParameter(firstInst);
                InjectPreActionCall();

                ComputeOffsets(targetMethod.Body);
            }

            public void InsertPostAction()
            {
                if (!ValidateParams(ActionType.PostAction))
                    return;
                postAction = FindValidAction(ActionType.PostAction);
                if (postAction == null)
                {
                    return;
                }
                
                if (IsAlreadyInjectPostAction())
                    return;

                InjectNopForCheckInjectedPostAction();

                var localVarCount = targetMethod.Body.Variables.Count;
                returnValueIndex = (byte) (localVarCount - 1);

                var firstOfInjection = ilProcessor.Create(OpCodes.Nop);
                InsertBefore(lastInst, firstOfInjection);
                
                if (targetMethod.ReturnType.Name != "Void")
                {
                    newInst = ilProcessor.Create(OpCodes.Stloc_S, returnValueIndex);
                    InsertBefore(lastInst, newInst);
                }

                InjectParameter(lastInst);
                
                InjectPostActionCall();
                
                if (targetMethod.ReturnType.Name != "Void")
                {
                    newInst = ilProcessor.Create(OpCodes.Ldloc_S, returnValueIndex);
                    InsertBefore(lastInst, newInst);
                }
                ComputeOffsets(targetMethod.Body);
                
                ChangeBranchTarget(lastInst, firstOfInjection);
                ComputeOffsets(targetMethod.Body);

            }

            private void ChangeBranchTarget(Instruction lastInst, Instruction firstOfInjection)
            {
                foreach (var instruction in targetMethod.Body.Instructions.ToArray())
                {
                    if (instruction.OpCode.OperandType == OperandType.InlineBrTarget || 
                        instruction.OpCode.OperandType == OperandType.ShortInlineBrTarget)
                    {
                        if (instruction.Operand == lastInst)
                        {
                            newInst = ilProcessor.Create(instruction.OpCode, firstOfInjection);
                            Replace(instruction, newInst);
                        }
                    }
                }
            }

            private int InsertParameterValuesArguments(int paramIndex)
            {
                var preActionParamInfos = preAction.GetParameters();
                var targetMethodParamLength = targetMethod.Parameters.Count;
                var isStaticMethod = targetMethod.IsStatic;
                
                for (var i = 0; i < targetMethodParamLength; i++)
                {
                    // if method.IsStatic first of stack is 'this'
                    var argIndex = i + (isStaticMethod ? 0 : 1);
                    if (preActionParamInfos[paramIndex + i].ParameterType.IsByRef)
                    {
                        newInst = ilProcessor.Create(OpCodes.Ldarga_S, (byte) argIndex);
                    }
                    else if (!preActionParamInfos[paramIndex + i].ParameterType.IsPrimitive &&
                             targetMethod.Parameters[i].ParameterType.IsPrimitive)
                    {
                        newInst = ilProcessor.Create(OpCodes.Ldarg_S, (byte) argIndex);
                        InsertBefore(firstInst, newInst);
                        newInst = ilProcessor.Create(OpCodes.Box, targetMethod.Parameters[i].ParameterType);
                    }
                    else
                    {
                        newInst = ilProcessor.Create(OpCodes.Ldarg_S, (byte) argIndex);
                    }

                    InsertBefore(firstInst, newInst);
                }

                return targetMethodParamLength;
            }

            private void InsertReturnValueArguments(byte returnValueIndex, int paramIndex)
            {
                var postActionParamInfo = postAction.GetParameters();

                if (postActionParamInfo[paramIndex].ParameterType.IsByRef)
                {
                    newInst = ilProcessor.Create(OpCodes.Ldloca_S, returnValueIndex);
                }
                else if (!postActionParamInfo[paramIndex].ParameterType.IsPrimitive && 
                         targetMethod.ReturnType.IsPrimitive)
                {
                    newInst = ilProcessor.Create(OpCodes.Ldloc_S, returnValueIndex);
                    InsertBefore(firstInst, newInst);
                    newInst = ilProcessor.Create(OpCodes.Box, targetMethod.ReturnType);
                }
                else
                {
                    newInst = ilProcessor.Create(OpCodes.Ldloc_S, returnValueIndex);
                }

                InsertBefore(lastInst, newInst);
            }

            private int InsertAttributeValuesArguments(Instruction target)
            {
                var constructorArguments = attribute.ConstructorArguments;
                foreach (var constructorArgument in constructorArguments)
                {
                    var val = constructorArgument.Value;
                    var argumentType = constructorArgument.Type.GetMonoType();
                    InsertValueToArgument(val, argumentType, target);
                }

                return constructorArguments.Count;
            }

            private void InsertValueToArgument(object val, Type argumentType, Instruction target)
            {
                if (argumentType == typeof(string))
                {
                    newInst = ilProcessor.Create(OpCodes.Ldstr, (string) val);
                    InsertBefore(target, newInst);
                }
                else if (argumentType == typeof(int))
                {
                    newInst = ilProcessor.Create(OpCodes.Ldc_I4, (int) val);
                    InsertBefore(target, newInst);
                }
                else if (argumentType == typeof(long))
                {
                    newInst = ilProcessor.Create(OpCodes.Ldc_I8, (long) val);
                    InsertBefore(target, newInst);
                }
                else if (argumentType == typeof(float))
                {
                    newInst = ilProcessor.Create(OpCodes.Ldc_R4, (float) val);
                    InsertBefore(target, newInst);
                }
                else if (argumentType == typeof(double))
                {
                    newInst = ilProcessor.Create(OpCodes.Ldc_R8, (float) val);
                    InsertBefore(target, newInst);
                }
                else if (argumentType == typeof(bool))
                {
                    newInst = ilProcessor.Create(OpCodes.Ldc_I4, (bool) val ? 1 : 0);
                    InsertBefore(target, newInst);
                }
                else
                {
                    Debug.LogError($"{type} parameter is not supported for UnityDecoratorAttribute");
                }
            }

            private void InsertBefore(Instruction target, Instruction instruction)
            {
                ilProcessor.InsertBefore(target, instruction);
            }

            private void InsertAfter(Instruction target, Instruction instruction)
            {
                ilProcessor.InsertAfter(target, instruction);
            }

            private void Replace(Instruction target, Instruction instruction)
            {
                ilProcessor.Replace(target, instruction);
            }

            //Calculating the offset of the injected function
            private void ComputeOffsets(MethodBody body)
            {
                var offset = 0;
                foreach (var instruction in body.Instructions)
                {
                    instruction.Offset = offset;
                    offset += instruction.GetSize();
                }
            }
        }
    }
}
#endif