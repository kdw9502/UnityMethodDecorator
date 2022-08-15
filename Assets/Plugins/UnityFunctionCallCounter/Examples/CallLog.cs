using System;
using System.Collections;
using System.Collections.Generic;
using UnityDecoratorAttribute;
using UnityEngine;
using UnityEngine.Scripting;

namespace UnityDecoratorAttribute
{
    public class ZeroParameterLogAttribute : DecoratorAttribute
    {
        [Preserve]
        public static void PreAction(string className, string methodName)
        {
            Debug.Log($"{className}::{methodName}");
        }

        public static PreActionParameterType[] ParameterTypes => new[] {PreActionParameterType.ClassName, PreActionParameterType.MethodName};
    }


    public class OneParameterLogAttribute : DecoratorAttribute
    {
        [Preserve]
        public static void PreAction(string className, string methodName, object param1)
        {
            Debug.Log($"{className}::{methodName} param: {param1}");
        }

        public static PreActionParameterType[] ParameterTypes => new[]
            {PreActionParameterType.ClassName, PreActionParameterType.MethodName, PreActionParameterType.ParameterValues};
    }


    public class TwoParameterLogAttribute : DecoratorAttribute
    {
        [Preserve]
        public static void PreAction(string className, string methodName, object param1, object param2)
        {
            Debug.Log($"{className}::{methodName} param: {param1}, {param2}");
        }

        public static PreActionParameterType[] ParameterTypes => new[]
            {PreActionParameterType.ClassName, PreActionParameterType.MethodName, PreActionParameterType.ParameterValues};
    }

    public class ThreeParameterLogAttribute : DecoratorAttribute
    {
        [Preserve]
        public static void PreAction(string className, string methodName, object param1, object param2, object param3)
        {
            Debug.Log($"{className}::{methodName} param: {param1}, {param2}, {param3}");
        }

        public static PreActionParameterType[] ParameterTypes => new[]
            {PreActionParameterType.ClassName, PreActionParameterType.MethodName, PreActionParameterType.ParameterValues};
    }


    public class FourParameterLogAttribute : DecoratorAttribute
    {
        [Preserve]
        public static void PreAction(string className, string methodName, object param1, object param2, object param3,
            object param4)
        {
            Debug.Log($"{className}::{methodName} param: {param1}, {param2}, {param3}, {param4}");
        }

        public static PreActionParameterType[] ParameterTypes => new[]
            {PreActionParameterType.ClassName, PreActionParameterType.MethodName, PreActionParameterType.ParameterValues};
    }

    public class LogThis : DecoratorAttribute
    {
        [Preserve]
        public static void PreAction(string className, string methodName, object @this)
        {
            Debug.Log($"{@this} {className}::{methodName}");
        }

        public static PreActionParameterType[] ParameterTypes => new[]
            {PreActionParameterType.ClassName, PreActionParameterType.MethodName, PreActionParameterType.This};
    }
}