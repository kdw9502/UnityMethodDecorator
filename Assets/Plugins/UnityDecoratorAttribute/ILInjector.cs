#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using UnityEditor.Callbacks;
using Debug = UnityEngine.Debug;
using MethodBody = Mono.Cecil.Cil.MethodBody;

namespace UnityDecoratorAttribute
{
//https://programmer.group/use-mono.cecil-to-inject-code-into-dll-in-unity.html
//https://www.reddit.com/r/csharp/comments/5qtpso/using_monocecil_in_c_with_unity/
    public static class ILInjector
    {
        private static bool isInjected = false;
        private const int INJECTION_NOP_COUNT = 3;
        public static string[] injectAssemblies = {"Assembly-CSharp.dll", "Tests.dll"};

        static readonly char sep = Path.DirectorySeparatorChar;

        static string assemblyDirectoryPath =
            Application.dataPath + sep + ".." + sep + "Library" + sep + "ScriptAssemblies" + sep;

        [PostProcessBuild(1000)]
        private static void OnPostprocessBuildPlayer(BuildTarget buildTarget, string buildPath)
        {
            var dllPath = buildPath.Replace(".exe", $"_Data{sep}Managed{sep}");
            if (Directory.Exists(dllPath))
                assemblyDirectoryPath = dllPath;
            ForceInjectAll();
        }

        [UnityEditor.Callbacks.PostProcessScene]
        [InitializeOnLoadMethod]
        // [PostProcessScene]
        public static void AutoInject()
        {
            if (EditorApplication.isCompiling || Application.isPlaying)
                return;

            Stopwatch stopwatch = Stopwatch.StartNew();
            InjectAll();
            Debug.Log($"Inject Elapsed : {stopwatch.ElapsedMilliseconds:0}ms");
        }

        public static void ForceInjectAll()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            isInjected = false;
            InjectAll();
            Debug.Log($"Inject Elapsed : {stopwatch.ElapsedMilliseconds:0}ms");
        }

        private static void InjectAll()
        {
            if (isInjected)
                return;
            isInjected = true;

            foreach (var assembly in injectAssemblies)
            {
                var fullPath = assemblyDirectoryPath + assembly;
                if (File.Exists(fullPath))
                    InjectAssembly(fullPath);
            }
        }

        private static void InjectAssembly(string assemblyPath)
        {
            using var assemblyDefinition =
                AssemblyDefinition.ReadAssembly(assemblyPath, new ReaderParameters {ReadWrite = true});
            var types = assemblyDefinition.MainModule.GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods();
                foreach (var method in methods)
                {
                    var decoratorAttribute = method.CustomAttributes.FirstOrDefault(attr =>
                        typeof(DecoratorAttribute).IsAssignableFrom(attr.AttributeType.GetMonoType()));

                    if (decoratorAttribute == null)
                        continue;

                    var methodInjector = new MethodInjector(type, method, decoratorAttribute, assemblyDefinition);
                    methodInjector.InsertPreAction();
                    methodInjector.InsertPostAction();
                }
            }

            foreach (var searchPath in AppDomain.CurrentDomain.GetAssemblies()
                         .Select(asm => Path.GetDirectoryName(asm.ManifestModule.FullyQualifiedName))
                         .Where(path => !string.IsNullOrEmpty(path)).Distinct())
            {
                ((BaseAssemblyResolver) assemblyDefinition.MainModule.AssemblyResolver).AddSearchDirectory(searchPath);
            }

            assemblyDefinition.Write();
        }

        private class MethodInjector
        {
            private TypeDefinition type;
            private MethodDefinition method;
            private CustomAttribute attribute;
            private Type attributeType;
            private AssemblyDefinition assemblyDefinition;

            private MethodInfo preAction;
            private DecoratorAttribute.PreActionParameterType[] preActionEnumParams;
            private Instruction newInst;
            private ILProcessor ilProcessor;
            private Instruction firstInst;

            private MethodInfo postAction;
            private Instruction lastInst;

            private BindingFlags bindingFlag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static |
                                               BindingFlags.FlattenHierarchy;

            public MethodInjector(TypeDefinition type, MethodDefinition method, CustomAttribute attribute,
                AssemblyDefinition assemblyDefinition)
            {
                this.type = type;
                this.method = method;
                this.attribute = attribute;
                this.assemblyDefinition = assemblyDefinition;

                attributeType = attribute.AttributeType.GetMonoType();
                ilProcessor = method.Body.GetILProcessor();
                firstInst = method.Body.Instructions.First();
            }

            private void FindValidPreAction()
            {
                preActionEnumParams =
                    attributeType.GetProperty("PreActionParameterTypes", bindingFlag)?.GetValue(null) as
                        DecoratorAttribute.PreActionParameterType[];

                if (preActionEnumParams == null)
                    return;

                var expectedParamTypes = new List<Type>();
                foreach (var parameterType in preActionEnumParams)
                {
                    switch (parameterType)
                    {
                        case DecoratorAttribute.PreActionParameterType.ClassName:
                            expectedParamTypes.Add(typeof(string));
                            break;
                        case DecoratorAttribute.PreActionParameterType.MethodName:
                            expectedParamTypes.Add(typeof(string));
                            break;
                        case DecoratorAttribute.PreActionParameterType.This:
                            expectedParamTypes.Add(type.GetMonoType());
                            break;
                        case DecoratorAttribute.PreActionParameterType.ParameterValues:
                            expectedParamTypes.AddRange(method.Parameters.Select(param =>
                                param.ParameterType.GetMonoType()));
                            break;
                        case DecoratorAttribute.PreActionParameterType.AttributeValues:
                            expectedParamTypes.AddRange(
                                attribute.Constructor.Parameters.Select(param => param.ParameterType.GetMonoType()));
                            break;
                    }
                }

                var methods = attributeType.GetMethods(bindingFlag);

                foreach (var method in methods)
                {
                    if (method.Name != "PreAction")
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
                        preAction = method;
                        break;;
                    }
                }

                if (preAction == null)
                {
                    Debug.Log($"Can't find appropriate {attributeType.Name} PreAction for {method.Name}. " +
                              $"Expected : PreAction({string.Join(", ", expectedParamTypes.Select(type => type.Name))})");
                }
            }


            private void InjectPreActionParameterLoadIL()
            {
                foreach (var parameter in preActionEnumParams)
                {
                    switch (parameter)
                    {
                        case DecoratorAttribute.PreActionParameterType.ClassName:
                            newInst = ilProcessor.Create(OpCodes.Ldstr, type.Name);
                            InsertBefore(ilProcessor, firstInst, newInst);
                            break;
                        case DecoratorAttribute.PreActionParameterType.MethodName:
                            newInst = ilProcessor.Create(OpCodes.Ldstr, method.Name);
                            InsertBefore(ilProcessor, firstInst, newInst);
                            break;
                        case DecoratorAttribute.PreActionParameterType.This:
                            newInst = ilProcessor.Create(OpCodes.Ldarg_S, (byte) 0);
                            InsertBefore(ilProcessor, firstInst, newInst);
                            break;
                        case DecoratorAttribute.PreActionParameterType.ParameterValues:
                            InsertParameterValuesArguments();
                            break;
                        case DecoratorAttribute.PreActionParameterType.AttributeValues:
                            InsertAttributeValuesArguments(firstInst);
                            break;
                    }
                }
            }

            private void InjectPreActionCall()
            {
                var preActionRef = assemblyDefinition.MainModule.ImportReference(preAction);
                newInst = ilProcessor.Create(OpCodes.Call, preActionRef);
                InsertBefore(ilProcessor, firstInst, newInst);
            }

            private void InjectNopForCheckInjected()
            {
                // To Check Injection, Insert 3 nop  

                for (int i = 0; i < INJECTION_NOP_COUNT; i++)
                {
                    newInst = ilProcessor.Create(OpCodes.Nop);
                    InsertBefore(ilProcessor, firstInst, newInst);
                }
            }

            private bool IsAlreadyInjectPreAction()
            {
                if (method.Body.Instructions.Count < INJECTION_NOP_COUNT)
                    return false;

                return method.Body.Instructions.Take(3).All(inst => inst.OpCode.Code == Code.Nop);
            }

            public void InsertPreAction()
            {
                FindValidPreAction();
                if (preAction == null)
                {
                    return;
                }

                if (IsAlreadyInjectPreAction())
                    return;

                InjectNopForCheckInjected();
                InjectPreActionParameterLoadIL();
                InjectPreActionCall();

                ComputeOffsets(method.Body);
            }

            public void InsertPostAction()
            {
                postAction = attributeType.GetMethod("PostAction", bindingFlag);
                if (postAction == null)
                {
                    // Debug.Log($"{attribute.AttributeType.Name} doesn't have PostAction method");
                    return;
                }

                var postActionEnumParams =
                    attributeType.GetProperty("PostActionParameterTypes", bindingFlag)?.GetValue(null) as
                        DecoratorAttribute.PostActionParameterType[] ??
                    new DecoratorAttribute.PostActionParameterType[] { };

                var postActionRef = assemblyDefinition.MainModule.ImportReference(postAction);

                if (method.Body.Instructions.Count >= INJECTION_NOP_COUNT)
                {
                    if (method.Body.Instructions.TakeLast(3).All(inst => inst.OpCode.Code == Code.Nop))
                    {
                        return;
                    }
                }

                ilProcessor = method.Body.GetILProcessor();
                lastInst = method.Body.Instructions.Last();


                var localVarCount = method.Body.Variables.Count;
                byte returnValueIndex = (byte) (localVarCount - 1);

                // To Check Injection, Insert 3 nop  
                for (int i = 0; i < INJECTION_NOP_COUNT; i++)
                {
                    newInst = ilProcessor.Create(OpCodes.Nop);
                    InsertAfter(ilProcessor, lastInst, newInst);
                }

                if (method.ReturnType.Name != "Void")
                {
                    newInst = ilProcessor.Create(OpCodes.Stloc_S, returnValueIndex);
                    InsertBefore(ilProcessor, lastInst, newInst);
                }

                foreach (var parameter in postActionEnumParams)
                {
                    switch (parameter)
                    {
                        case DecoratorAttribute.PostActionParameterType.ClassName:
                            newInst = ilProcessor.Create(OpCodes.Ldstr, type.Name);
                            InsertBefore(ilProcessor, lastInst, newInst);
                            break;
                        case DecoratorAttribute.PostActionParameterType.MethodName:
                            newInst = ilProcessor.Create(OpCodes.Ldstr, method.Name);
                            InsertBefore(ilProcessor, lastInst, newInst);
                            break;
                        case DecoratorAttribute.PostActionParameterType.This:
                            newInst = ilProcessor.Create(OpCodes.Ldarg_S, (byte) 0);
                            InsertBefore(ilProcessor, lastInst, newInst);
                            break;
                        case DecoratorAttribute.PostActionParameterType.ReturnValue:
                            InsertReturnValueArguments(returnValueIndex);
                            break;
                        case DecoratorAttribute.PostActionParameterType.AttributeValues:
                            InsertAttributeValuesArguments(lastInst);
                            break;
                    }
                }

                newInst = ilProcessor.Create(OpCodes.Call, postActionRef);
                InsertBefore(ilProcessor, lastInst, newInst);

                ComputeOffsets(method.Body);
            }

            private void InsertParameterValuesArguments()
            {
                var preActionParamInfos = preAction.GetParameters();
                var targetMethodParamLength = method.Parameters.Count;
                var isStaticMethod = method.IsStatic;

                for (var i = 0; i < targetMethodParamLength; i++)
                {
                    // if method.IsStatic first of stack is 'this'
                    var argIndex = i + (isStaticMethod ? 0 : 1);
                    if (preActionParamInfos[i].ParameterType.IsByRef)
                    {
                        newInst = ilProcessor.Create(OpCodes.Ldarga_S, (byte) argIndex);
                    }
                    else if (!preActionParamInfos[i].ParameterType.IsPrimitive &&
                             method.Parameters[i].ParameterType.IsPrimitive)
                    {
                        newInst = ilProcessor.Create(OpCodes.Ldarg_S, (byte) argIndex);
                        InsertBefore(ilProcessor, firstInst, newInst);
                        newInst = ilProcessor.Create(OpCodes.Box, method.Parameters[i].ParameterType);
                    }
                    else
                    {
                        newInst = ilProcessor.Create(OpCodes.Ldarg_S, (byte) argIndex);
                    }

                    InsertBefore(ilProcessor, firstInst, newInst);
                }
            }

            private void InsertReturnValueArguments(byte returnValueIndex)
            {
                if (method.ReturnType.IsByReference)
                {
                    newInst = ilProcessor.Create(OpCodes.Ldloca_S, returnValueIndex);
                }
                // else if (!postAction.GetParameters() && method.ReturnType.IsPrimitive)
                // {
                //     newInst = ilProcessor.Create(OpCodes.Ldloc_S, returnValueIndex);
                //     InsertBefore(ilProcessor, firstInst, newInst);
                //     newInst = ilProcessor.Create(OpCodes.Box, method.ReturnType);
                // }
                else
                {
                    newInst = ilProcessor.Create(OpCodes.Ldloc_S, returnValueIndex);
                }

                InsertBefore(ilProcessor, lastInst, newInst);
            }

            private void InsertAttributeValuesArguments(Instruction target)
            {
                var constructorArguments = attribute.ConstructorArguments;
                foreach (var constructorArgument in constructorArguments)
                {
                    var val = constructorArgument.Value;
                    var argumentType = constructorArgument.Type.GetMonoType();
                    InsertValueToArgument(val, argumentType, target);
                }
            }

            private void InsertValueToArgument(object val, Type argumentType, Instruction target)
            {
                if (argumentType == typeof(string))
                {
                    newInst = ilProcessor.Create(OpCodes.Ldstr, (string) val);
                    InsertBefore(ilProcessor, target, newInst);
                }
                else if (argumentType == typeof(int))
                {
                    newInst = ilProcessor.Create(OpCodes.Ldc_I4, (int) val);
                    InsertBefore(ilProcessor, target, newInst);
                }
                else if (argumentType == typeof(long))
                {
                    newInst = ilProcessor.Create(OpCodes.Ldc_I8, (long) val);
                    InsertBefore(ilProcessor, target, newInst);
                }
                else if (argumentType == typeof(float))
                {
                    newInst = ilProcessor.Create(OpCodes.Ldc_R4, (float) val);
                    InsertBefore(ilProcessor, target, newInst);
                }
                else if (argumentType == typeof(double))
                {
                    newInst = ilProcessor.Create(OpCodes.Ldc_R8, (float) val);
                    InsertBefore(ilProcessor, target, newInst);
                }
                else if (argumentType == typeof(bool))
                {
                    newInst = ilProcessor.Create(OpCodes.Ldc_I4, (bool) val ? 1 : 0);
                    InsertBefore(ilProcessor, target, newInst);
                }
                else
                {
                    Debug.LogError($"{type} parameter is not supported for UnityDecoratorAttribute");
                }
            }

            private Instruction InsertBefore(ILProcessor ilProcessor, Instruction target, Instruction instruction)
            {
                ilProcessor.InsertBefore(target, instruction);
                return instruction;
            }

            private Instruction InsertAfter(ILProcessor ilProcessor, Instruction target, Instruction instruction)
            {
                ilProcessor.InsertAfter(target, instruction);
                return instruction;
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