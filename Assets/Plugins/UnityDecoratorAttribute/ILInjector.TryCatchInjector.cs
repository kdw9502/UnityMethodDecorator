using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

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
                firstInst = targetMethod.Body.Instructions.First();
                lastInst = targetMethod.Body.Instructions.Last();
            }

            public void InjectTryCatch()
            {
                var catchMethod = ilProcessor.Create(
                    OpCodes.Call,
                    assemblyDefinition.MainModule.ImportReference(attributeType.GetMethod("CatchException", new[] {typeof(Exception)})));
                var ret = ilProcessor.Create(OpCodes.Ret);
                var leave = ilProcessor.Create(OpCodes.Leave, ret);

                ilProcessor.InsertAfter(lastInst, catchMethod);

                ilProcessor.InsertAfter(catchMethod, leave);
                ilProcessor.InsertAfter(leave, ret);

                var handler = new ExceptionHandler(ExceptionHandlerType.Catch)
                {
                    TryStart = firstInst,
                    TryEnd = catchMethod,
                    HandlerStart = catchMethod,
                    HandlerEnd = ret,
                    CatchType = assemblyDefinition.MainModule.ImportReference(typeof(Exception)),
                };

                targetMethod.Body.ExceptionHandlers.Add(handler);

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
        }
    }
}
#endif