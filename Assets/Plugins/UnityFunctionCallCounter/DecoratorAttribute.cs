using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityDecoratorAttribute;
using UnityEngine;
using Debug = UnityEngine.Debug;

[AttributeUsage(AttributeTargets.Method)]
public abstract class DecoratorAttribute : Attribute
{
    public enum PreActionParameterType
    {
        ClassName,
        MethodName,
        This,
        ParameterValues,
    }
}

