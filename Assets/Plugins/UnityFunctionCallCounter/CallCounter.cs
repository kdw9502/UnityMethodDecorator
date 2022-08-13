using System;using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace FunctionCallCount
{
    public static class CallCounter
    {
        private static readonly Dictionary<(string className, string functionName), int> callCountDict = new();

        public static int GetFunctionCallCount(string className, string functionName)
        {
            var key = (className, functionName);
            return callCountDict.ContainsKey(key) ? callCountDict[key] : 0;
        }

        [Preserve]
        public static void IncreaseFunctionCallCount(string className, string functionName)
        {
            var key = (className, functionName);

            if (!callCountDict.ContainsKey(key))
            {
                callCountDict[key] = 0;
            }

            callCountDict[key]++;

            // Debug.Log($"CallCount {key} : {callCountDict[key]}");
        }

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class CallCountAttribute : Attribute
    {

    }
}