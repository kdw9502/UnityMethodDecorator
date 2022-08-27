using System;

namespace UnityDecoratorAttribute
{
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class DecoratorAttribute : Attribute
    {
        public enum ParameterType
        {
            ClassName,
            MethodName,
            This,  // Target Method's Instance
            ParameterValues, // Target Method's Parameter Values (only for PreAction)
            ReturnValue, //  Target Method's ReturnValue (only for PostAction)
            AttributeValues, // Attribute values
        }
    }
}