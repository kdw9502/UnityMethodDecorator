using System;
using UnityEngine;

namespace UnityDecoratorAttribute
{
    [AttributeUsage(AttributeTargets.Method)]

    public class IgnoreExceptionAttribute: Attribute
    {
        public static void CatchException(Exception exception)
        {
            Debug.LogError(exception);
        }
    }
}