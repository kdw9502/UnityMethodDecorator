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
        private abstract class AbstractInjector
        {
            protected readonly TypeDefinition type;
            protected readonly AssemblyDefinition assemblyDefinition;
            protected readonly MethodDefinition targetMethod;
            protected readonly CustomAttribute attribute;
            protected readonly Type attributeType;
            protected readonly ILProcessor ilProcessor;
            protected readonly Instruction firstInst;
            protected readonly Instruction lastInst;
            public AbstractInjector(TypeDefinition type, MethodDefinition targetMethod, CustomAttribute attribute,
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

            private void ComputeOffsets(MethodBody body)
            {
                var offset = 0;
                foreach (var instruction in body.Instructions)
                {
                    instruction.Offset = offset;
                    offset += instruction.GetSize();
                }
            }

            public abstract void Inject();

            protected void OnAfterMethodInject()
            {
                AddHistory();
                // targetMethod.Body.InitLocals = true;
                targetMethod.Body.OptimizeMacros();
                ComputeOffsets(targetMethod.Body);
            }
            
            private void AddHistory()
            {
                var attributeVariableForCheckInjected = new VariableDefinition(attribute.AttributeType);
                targetMethod.Body.Variables.Add(attributeVariableForCheckInjected);
            }

            protected bool IsInjected()
            {
                return targetMethod.Body.Variables.Any(x => x.VariableType == attribute.AttributeType);
            }
        }
    }
}
#endif