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
    public class ILInjector
    {
        private static bool isInjected = false;
        private const int INJECTION_NOP_COUNT = 3;

        [PostProcessBuild(1000)]
        private static void OnPostprocessBuildPlayer(BuildTarget buildTarget, string buildPath)
        {
            isInjected = false;
        }


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


        public static void InjectAll()
        {
            if (isInjected)
                return;
            isInjected = true;

            var sep = Path.DirectorySeparatorChar;
            var assemblyPath = Application.dataPath + sep + ".." + sep + "Library" + sep + "ScriptAssemblies" + sep +
                               "Assembly-CSharp.dll";
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

                    Inject(type, method, decoratorAttribute, assemblyDefinition);
                }
            }

            assemblyDefinition.Write();
        }

        private static void Inject(TypeDefinition type, MethodDefinition method, CustomAttribute attribute,
            AssemblyDefinition assemblyDefinition)
        {
            var bindingFlag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static |
                              BindingFlags.FlattenHierarchy;
            var attributeType = attribute.AttributeType.GetMonoType();
            var preAction = attributeType.GetMethod("PreAction", bindingFlag);
            if (preAction == null)
            {
                Debug.LogError($"{attribute.AttributeType.Name} doesn't have PreAction method");
                return;
            }

            var preActionEnumParams =
                attributeType.GetProperty("ParameterTypes", bindingFlag)?.GetValue(null) as DecoratorAttribute.PreActionParameterType[] ??
                new DecoratorAttribute.PreActionParameterType[] { };

            var preActionRef = assemblyDefinition.MainModule.ImportReference(preAction);

            var ilProcessor = method.Body.GetILProcessor();
            var firstInst = method.Body.Instructions.First();

            Instruction newInst;

            if (method.Body.Instructions.Count >= INJECTION_NOP_COUNT)
            {
                if (method.Body.Instructions.Take(3).All(inst => inst.OpCode == OpCodes.Nop))
                {
                    // Debug.Log("Already Injected " + type.Name + "::" + method.Name);
                    return;
                }
            }

            // To Check Injection, Insert 3 nop  
            for (int i = 0; i < INJECTION_NOP_COUNT; i++)
            {
                newInst = ilProcessor.Create(OpCodes.Nop);
                InsertBefore(ilProcessor, firstInst, newInst);
            }

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
                        newInst = ilProcessor.Create(OpCodes.Ldarg_S, (byte)0);
                        InsertBefore(ilProcessor, firstInst, newInst);
                        break;
                    case DecoratorAttribute.PreActionParameterType.ParameterValues:
                        var preActionParamInfos = preAction.GetParameters();
                        var preActionParamLength = preActionParamInfos.Length - preActionEnumParams.Length + 1;
                        var isStaticMethod = method.IsStatic;

                        for (var i = 0; i < preActionParamLength; i++)
                        {
                            // if method.IsStatic first of stack is 'this'
                            var argIndex = i + (isStaticMethod ? 0 : 1);
                            if (preActionParamInfos[i].ParameterType.IsByRef)
                            {
                                newInst = ilProcessor.Create(OpCodes.Ldarga_S, (byte)argIndex);
                            }
                            else
                            {
                                newInst = ilProcessor.Create(OpCodes.Ldarg_S, (byte)argIndex);
                            }

                            InsertBefore(ilProcessor, firstInst, newInst);
                        }

                        break;
                }
            }

            newInst = ilProcessor.Create(OpCodes.Call, preActionRef);
            InsertBefore(ilProcessor, firstInst, newInst);

            ComputeOffsets(method.Body);

            // Debug.Log("Inject " + type.Name + "::" + method.Name);
        }

        private static Instruction InsertBefore(ILProcessor ilProcessor, Instruction target, Instruction instruction)
        {
            ilProcessor.InsertBefore(target, instruction);
            return instruction;
        }

        private static Instruction InsertAfter(ILProcessor ilProcessor, Instruction target, Instruction instruction)
        {
            ilProcessor.InsertAfter(target, instruction);
            return instruction;
        }

        //Calculating the offset of the injected function
        private static void ComputeOffsets(MethodBody body)
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
#endif