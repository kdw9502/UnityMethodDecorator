using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

#if UNITY_EDITOR
namespace UnityDecoratorAttribute
{
    public static partial class ILInjector
    {
        private class TryCatchInjector : AbstractInjector
        {

            public TryCatchInjector(TypeDefinition type, MethodDefinition targetMethod, CustomAttribute attribute,
                AssemblyDefinition assemblyDefinition):base(type,targetMethod,attribute,assemblyDefinition)
            {
            }

            public override void Inject()
            {
                if (IsInjected())
                    return;
                
                var returnInstruction = FixReturns();

                var beforeReturn = Instruction.Create(OpCodes.Nop);
                ilProcessor.InsertBefore(returnInstruction, beforeReturn);
                
                var catchMethod = ilProcessor.Create(
                    OpCodes.Call,
                    assemblyDefinition.MainModule.ImportReference(attributeType.GetMethod("CatchException", new[] {typeof(Exception)})));
                
                ilProcessor.InsertAfter(beforeReturn, catchMethod);
                
                var handler = new ExceptionHandler(ExceptionHandlerType.Catch)
                {
                    TryStart = firstInst,
                    TryEnd = beforeReturn,
                    HandlerStart = beforeReturn,
                    HandlerEnd = returnInstruction,
                    CatchType = assemblyDefinition.MainModule.ImportReference(typeof(Exception))
                };

                targetMethod.Body.ExceptionHandlers.Add(handler);
                OnAfterMethodInject();
            }

            // https://stackoverflow.com/questions/12769699/mono-cecil-injecting-try-finally
            Instruction FixReturns()
            {
                var body = targetMethod.Body;
                if (targetMethod.ReturnType.Name == "Void")
                {
                    var instructions = body.Instructions;
                    var lastRet = Instruction.Create(OpCodes.Ret);
                    instructions.Add(lastRet);

                    for (var index = 0; index < instructions.Count - 1; index++)
                    {
                        var instruction = instructions[index];
                        if (instruction.OpCode == OpCodes.Ret)
                        {
                            instructions[index] = Instruction.Create(OpCodes.Leave, lastRet);
                        }
                    }
                    return lastRet;
                }
                else
                {
                    var instructions = targetMethod.Body.Instructions;
                    var returnVariable = new VariableDefinition(targetMethod.ReturnType);
                    body.Variables.Add(returnVariable);
                    var lastLd = Instruction.Create(OpCodes.Ldloc, returnVariable);
                    instructions.Add(lastLd);
                    instructions.Add(Instruction.Create(OpCodes.Ret));

                    for (var index = 0; index < instructions.Count - 2; index++)
                    {
                        var instruction = instructions[index];
                        if (instruction.OpCode == OpCodes.Ret)
                        {
                            instructions[index] = Instruction.Create(OpCodes.Leave, lastLd);
                            instructions.Insert(index, Instruction.Create(OpCodes.Stloc, returnVariable));
                            index++;
                        }
                    }
                    return lastLd;
                }
            }
            Instruction FirstInstructionSkipCtor()
            {
                if (targetMethod.IsConstructor && !targetMethod.IsStatic)
                {
                    return targetMethod.Body.Instructions.Skip(2).First();
                }
                return targetMethod.Body.Instructions.First();
            }
        }
    }
}
#endif