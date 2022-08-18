using System;


[AttributeUsage(AttributeTargets.Method)]
public abstract class DecoratorAttribute : Attribute
{
    public enum PreActionParameterType
    {
        ClassName,
        MethodName,
        This,
        ParameterValues,
        AttributeValues,
    }
    
    public enum PostActionParameterType
    {
        ClassName,
        MethodName,
        This,
        ReturnValues,
        AttributeValues,
    }
}

