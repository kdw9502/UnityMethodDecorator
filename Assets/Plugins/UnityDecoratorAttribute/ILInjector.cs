#if UNITY_EDITOR

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using UnityEditor.Callbacks;
using Debug = UnityEngine.Debug;

namespace UnityDecoratorAttribute
{
//https://programmer.group/use-mono.cecil-to-inject-code-into-dll-in-unity.html
//https://www.reddit.com/r/csharp/comments/5qtpso/using_monocecil_in_c_with_unity/
    public static partial class ILInjector
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

                    if (decoratorAttribute != null)
                    {
                        var methodInjector = new MethodInjector(type, method, decoratorAttribute, assemblyDefinition);
                        methodInjector.InsertPreAction();
                        methodInjector.InsertPostAction();
                    }
                    
                    var ignoreExceptionAttribute = method.CustomAttributes.FirstOrDefault(attr =>
                        typeof(IgnoreExceptionAttribute).IsAssignableFrom(attr.AttributeType.GetMonoType()));

                    if (ignoreExceptionAttribute != null)
                    {
                        var tryCatchInjector =
                            new TryCatchInjector(type, method, ignoreExceptionAttribute, assemblyDefinition);
                        tryCatchInjector.InjectTryCatch();
                    }
                        
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
    }
}
#endif
