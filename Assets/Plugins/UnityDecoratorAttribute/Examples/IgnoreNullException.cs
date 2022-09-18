using System;
using UnityEngine;

namespace UnityDecoratorAttribute
{
    [AttributeUsage(AttributeTargets.Method)]

    public partial class IgnoreNullException : IgnoreExceptionAttribute
    {
        public static void CatchException(Exception exception)
        {
            if (exception is NullReferenceException)
                Debug.LogError(exception);
            else
                throw exception;
        }
    }
}