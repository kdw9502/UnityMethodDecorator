using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Mono.Cecil;
public class ILInjector
{
    public static bool isInjected = false;
    [UnityEditor.Callbacks.PostProcessScene]
    public static void AutoInject()
    {
        InjectAll();
    }

    public static void InjectAll()
    {
        if (EditorApplication.isCompiling || Application.isPlaying || isInjected)
            return;

        // var types = AppDomain.CurrentDomain.GetAssemblies()
        //     .Where(assembly => !(assembly.ManifestModule is System.Reflection.Emit.ModuleBuilder))
        //     .SelectMany(assembly => assembly.GetTypes());

        // var methods = types
            // .SelectMany(type => type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic |
                                                // BindingFlags.DeclaredOnly));
        var assemblyDefinitions = AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => AssemblyDefinition.ReadAssembly(assembly.Location));
        //
        // var types = assemblyDefinitions.Select();
    }
    
    public static void Inject()
    {
        if (EditorApplication.isCompiling || Application.isPlaying)
            return;
        
        
    }
}