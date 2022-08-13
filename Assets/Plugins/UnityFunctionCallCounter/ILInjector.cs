#if UNITY_EDITOR

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using UnityEditor.Callbacks;
using MethodBody = Mono.Cecil.Cil.MethodBody;

//https://programmer.group/use-mono.cecil-to-inject-code-into-dll-in-unity.html
//https://www.reddit.com/r/csharp/comments/5qtpso/using_monocecil_in_c_with_unity/
public class ILInjector
{
    private static bool isInjected = false;
    [PostProcessBuild(1000)]
    private static void OnPostprocessBuildPlayer(BuildTarget buildTarget, string buildPath)
    {
        isInjected = false;
    }
    
    
    [PostProcessScene]
    public static void AutoInject()
    {
        InjectAll();
    }

    [InitializeOnLoadMethod]
    public static void InjectAll()
    {
        if (isInjected)
            return;

        var sep = Path.DirectorySeparatorChar;
        var assemblyPath = Application.dataPath + sep + ".." + sep + "Library" + sep + "ScriptAssemblies" + sep +
                           "Assembly-CSharp.dll";
        using var assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath, new ReaderParameters {ReadWrite = true});
        var types = assemblyDefinition.MainModule.GetTypes();
        foreach (var type in types)
        {
            var methods = type.GetMethods()
                .Where(method => method.CustomAttributes
                    .Any(attr => attr.Constructor.DeclaringType.Name == nameof(CallCountAttribute)));
            foreach (var method in methods)
            {
                Inject(type, method, assemblyDefinition);
                Debug.Log("Inject " + type.Name + "::" + method.Name);
            }
        }

        assemblyDefinition.Write();
        isInjected = true;
    }

    private static void Inject(TypeDefinition type, MethodDefinition method, AssemblyDefinition assemblyDefinition)
    {
        var firstInst = method.Body.Instructions.First();
        var inst = method.Body.Instructions;
        var ilProcessor = method.Body.GetILProcessor();
        var increaseCallCountRef = assemblyDefinition.MainModule.ImportReference(typeof(CallCounter).GetMethod(
            "IncreaseFunctionCallCount",
            new[] {typeof(string), typeof(string)}));
        var newInst = ilProcessor.Create(OpCodes.Ldstr, type.Name);
        var curernt = InsertBefore(ilProcessor, firstInst, newInst);
        newInst = ilProcessor.Create(OpCodes.Ldstr, method.Name);
        curernt = InsertBefore(ilProcessor, firstInst, newInst);
        newInst = ilProcessor.Create(OpCodes.Call, increaseCallCountRef);
        curernt = InsertBefore(ilProcessor, firstInst, newInst);
        newInst = ilProcessor.Create(OpCodes.Nop);
        curernt = InsertBefore(ilProcessor, firstInst, newInst);

        ComputeOffsets(method.Body);
    }

    public static void Inject()
    {
        if (EditorApplication.isCompiling || Application.isPlaying)
            return;
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
#endif