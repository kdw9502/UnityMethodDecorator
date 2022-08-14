using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using MethodCallCount;
using UnityEngine;
using Debug = UnityEngine.Debug;

[AttributeUsage(AttributeTargets.Method)]
public abstract class DecoratorAttribute : Attribute
{
    public enum ParameterType
    {
        ClassName,
        MethodName,
        ParameterValues,
    }
}

