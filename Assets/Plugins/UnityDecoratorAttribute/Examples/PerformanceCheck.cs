using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.Scripting;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace UnityDecoratorAttribute
{
    // only for main thread
    public static class PerformanceCheck
    {
        private static Stopwatch stopwatch = new Stopwatch();
        private static Dictionary<(string className, string methodName), long> totalExecutionTime = new();
        private static Dictionary<(string className, string methodName), long> executeCount = new();
        private static (string className, string methodName) currentMethod;
        public static void Start(string className, string methodName)
        {
            currentMethod = (className, methodName);
            stopwatch.Restart();
        }

        public static void End()
        {
            executeCount[currentMethod] = executeCount.GetValueOrDefault(currentMethod) + 1;
            totalExecutionTime[currentMethod] = totalExecutionTime.GetValueOrDefault(currentMethod) + stopwatch.ElapsedMilliseconds;
        }

        public static long GetTotalExecutionTimeMs(string className, string methodName)
        {
            return totalExecutionTime.GetValueOrDefault((className, methodName));
        }
        
        public static long GetMeanExecutionTimeMs(string className, string methodName)
        {
            return GetTotalExecutionTimeMs(className, methodName) / GetExecutionCount(className, methodName);
        }

        public static long GetExecutionCount(string className, string methodName)
        {
            return executeCount.GetValueOrDefault((className, methodName));
        }
    }

    public class PerformanceCheckAttribute : DecoratorAttribute
    {
        [Preserve]
        public static void PreAction(string className, string methodName)
        {
            PerformanceCheck.Start(className, methodName);
        }

        public static ParameterType[] PreActionParameterTypes => new[] {ParameterType.ClassName, ParameterType.MethodName};

        [Preserve]
        public static void PostAction()
        {
            PerformanceCheck.End();
        }

        public static ParameterType[] PostActionParameterTypes => new ParameterType[]{};
    }
}