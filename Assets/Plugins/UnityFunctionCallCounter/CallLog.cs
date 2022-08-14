using System;
using System.Collections;
using System.Collections.Generic;
using MethodCallCount;
using UnityEngine;
using UnityEngine.Scripting;

public class ZeroParameterLogAttribute : DecoratorAttribute
{
    [Preserve]
    public static void PreAction(string className, string methodName)
    {
        Debug.Log($"Call {className}::{methodName}");
    }

    public static ParameterType[] ParameterTypes => new[] {ParameterType.ClassName, ParameterType.MethodName};
}

public class OneParameterLogAttribute : DecoratorAttribute
{
    [Preserve]
    public static void PreAction(string className, string methodName, object param1)
    {
        Debug.Log($"Call {className}::{methodName} with param {param1}");
    }

    public static ParameterType[] ParameterTypes => new[]
        {ParameterType.ClassName, ParameterType.MethodName, ParameterType.ParameterValues};
}

public class TwoParameterLogAttribute : DecoratorAttribute
{
    [Preserve]
    public static void PreAction(string className, string methodName, object param1, object param2)
    {
        Debug.Log($"Call {className}::{methodName} with param {param1} {param2}");
    }

    public static ParameterType[] ParameterTypes => new[]
        {ParameterType.ClassName, ParameterType.MethodName, ParameterType.ParameterValues};
}

public class ThreeParameterLogAttribute : DecoratorAttribute
{
    [Preserve]
    public static void PreAction(string className, string methodName, object param1, object param2, object param3)
    {
        Debug.Log($"Call {className}::{methodName} with param {param1} {param2} {param3}");
    }

    public static ParameterType[] ParameterTypes => new[]
        {ParameterType.ClassName, ParameterType.MethodName, ParameterType.ParameterValues};
}

public class FourParameterLogAttribute : DecoratorAttribute
{
    [Preserve]
    public static void PreAction(string className, string methodName, object param1, object param2, object param3,
        object param4)
    {
        Debug.Log($"Call {className}::{methodName} with param {param1} {param2} {param3} {param4}");
    }

    public static ParameterType[] ParameterTypes => new[]
        {ParameterType.ClassName, ParameterType.MethodName, ParameterType.ParameterValues};
}