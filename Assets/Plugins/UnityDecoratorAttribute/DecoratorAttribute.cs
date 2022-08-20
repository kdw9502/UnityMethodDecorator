using System;

namespace UnityDecoratorAttribute
{
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class DecoratorAttribute : Attribute
    {
        public enum PreActionParameterType
        {
            ClassName,
            MethodName,
            This,  // Target Method's Instance
            ParameterValues, // Target Method's Parmeter Values
            AttributeValues, // Attribute values
        }

        public enum PostActionParameterType
        {
            ClassName,
            MethodName,
            This,
            ReturnValue,
            AttributeValues,
        }
    }
}