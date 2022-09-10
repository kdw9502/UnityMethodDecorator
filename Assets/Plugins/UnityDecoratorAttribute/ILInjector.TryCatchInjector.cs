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
        private class TryCatchInjector
        {
            private readonly TypeDefinition type;
            private readonly AssemblyDefinition assemblyDefinition;
            private readonly MethodDefinition targetMethod;
            private readonly CustomAttribute attribute;
            private readonly Type attributeType;
            private readonly ILProcessor ilProcessor;
            private readonly Instruction firstInst;
            private readonly Instruction lastInst;

            public TryCatchInjector(TypeDefinition type, MethodDefinition targetMethod, CustomAttribute attribute,
                AssemblyDefinition assemblyDefinition)
            {
                this.type = type;
                this.targetMethod = targetMethod;
                this.attribute = attribute;
                this.assemblyDefinition = assemblyDefinition;

                attributeType = attribute.AttributeType.GetMonoType();
                ilProcessor = targetMethod.Body.GetILProcessor();
                firstInst = FirstInstructionSkipCtor();
                lastInst = targetMethod.Body.Instructions.Last();
            }

            public void InjectTryCatch()
            {
                var returnInstruction = FixReturns();

                var beforeReturn = Instruction.Create(OpCodes.Nop);
                ilProcessor.InsertBefore(returnInstruction, beforeReturn);
                
                var catchMethod = ilProcessor.Create(
                    OpCodes.Call,
                    assemblyDefinition.MainModule.ImportReference(attributeType.GetMethod("CatchException", new[] {typeof(Exception)})));

                // ilProcessor.InsertAfter(beforeReturn, catchMethod);
                
                var handler = new ExceptionHandler(ExceptionHandlerType.Catch)
                {
                    TryStart = firstInst,
                    TryEnd = beforeReturn,
                    HandlerStart = beforeReturn,
                    HandlerEnd = returnInstruction
                };

                targetMethod.Body.ExceptionHandlers.Add(handler);
                targetMethod.Body.InitLocals = true;
                targetMethod.Body.OptimizeMacros();

                ComputeOffsets(targetMethod.Body);
            }
            
            private void ComputeOffsets(MethodBody body)
            {
                var offset = 0;
                foreach (var instruction in body.Instructions)
                {
                    instruction.Offset = offset;
                    offset += instruction.GetSize();
                }
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