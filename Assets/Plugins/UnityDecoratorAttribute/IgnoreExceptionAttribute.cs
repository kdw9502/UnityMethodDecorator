using System;
using UnityEngine;

namespace UnityDecoratorAttribute
{
    public class IgnoreExceptionAttribute
    {
        public void CatchException(Exception exception)
        {
            Debug.Log($"{exception.Message}");
        }
    }
}