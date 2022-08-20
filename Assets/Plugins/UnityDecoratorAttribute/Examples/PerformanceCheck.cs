using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.Scripting;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace UnityDecoratorAttribute
{
    public static class PerformanceCheck
    {
        private static Stopwatch stopwatch = new Stopwatch();
        private static Dictionary<string, long> executionTime = new();
        private static string currentMethod;
        public static void Start(string methodName)
        {
            currentMethod = methodName;
            stopwatch.Restart();
        }

        public static void End()
        {
            executionTime[currentMethod] = executionTime.GetValueOrDefault(currentMethod, 0) + stopwatch.ElapsedMilliseconds;
        }

        public static long GetExecutionTime(string className, string methodName)
        {
            return executionTime.GetValueOrDefault($"{className}::{methodName}");
        }
    }

    public class PerformanceCheckAttribute : DecoratorAttribute
    {
        [Preserve]
        public static void PreAction(string className, string methodName)
        {
            PerformanceCheck.Start($"{className}::{methodName}");
        }

        public static PreActionParameterType[] PreActionParameterTypes => new[]
            {PreActionParameterType.ClassName, PreActionParameterType.MethodName};

        [Preserve]
        public static void PostAction()
        {
            PerformanceCheck.End();
        }

        public static PostActionParameterType[] PostActionParameterTypes => new PostActionParameterType[]{};
    }
}