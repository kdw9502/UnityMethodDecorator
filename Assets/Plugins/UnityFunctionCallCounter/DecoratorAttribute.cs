using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

[AttributeUsage(AttributeTargets.Method)]
public abstract class DecoratorAttribute : Attribute
{
    public object[] param;

    public abstract void PreAction(object[] param);
    public abstract void PostAction();
}

public class PrintElapsedTimeAttribute : DecoratorAttribute
{
    private string methodName;
    private DateTime startTime;
    public PrintElapsedTimeAttribute(string name)
    {
        methodName = name;
    }
    public override void PreAction(object[] param)
    {
        this.param = param;
        startTime = DateTime.Now;
    }

    public override void PostAction()
    {
        Debug.Log($"{methodName} ElapsedTime {(DateTime.Now - startTime).TotalSeconds} seconds");
    }
}