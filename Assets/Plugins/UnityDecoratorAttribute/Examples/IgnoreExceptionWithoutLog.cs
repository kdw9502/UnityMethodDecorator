using System;
using UnityEngine;

namespace UnityDecoratorAttribute
{
    [AttributeUsage(AttributeTargets.Method)]

    public class IgnoreExceptionWithoutLog: IgnoreExceptionAttribute
    {
        public static void CatchException(Exception exception)
        {
            // Do Nothing
        }
    }
}