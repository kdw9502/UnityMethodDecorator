using System;
using System.Collections;
using System.Collections.Generic;
using UnityDecoratorAttribute;
using UnityEngine;
using UnityEngine.Scripting;

namespace UnityDecoratorAttribute
{
    public class ParameterLogAttribute : DecoratorAttribute
    {
        [Preserve]
        public static void PreAction(string className, string methodName)
        {
            Debug.Log($"{className}::{methodName}");
        }

        [Preserve]
        public static void PreAction(string className, string methodName, object param1)
        {
            Debug.Log($"{className}::{methodName} Parameters : {param1}");
        }
        
        [Preserve]
        public static void PreAction(string className, string methodName, object param1, object param2)
        {
            Debug.Log($"{className}::{methodName} Parameters : {param1}, {param2}");
        }
        
        [Preserve]
        public static void PreAction(string className, string methodName, object param1, object param2, object param3)
        {
            Debug.Log($"{className}::{methodName} Parameters : {param1}, {param2}, {param3}");
        }
        
        [Preserve]
        public static void PreAction(string className, string methodName, object param1, object param2, object param3,
            object param4)
        {
            Debug.Log($"{className}::{methodName} Parameters : {param1}, {param2}, {param3}, {param4}");
        }
        
        [Preserve]
        public static void PreAction(string className, string methodName, object param1, object param2, object param3,
            object param4, object param5)
        {
            Debug.Log($"{className}::{methodName} Parameters : {param1}, {param2}, {param3}, {param4}, {param5}");
        }
        
        public static ParameterType[] PreActionParameterTypes => 
            new[] {ParameterType.ClassName, ParameterType.MethodName, ParameterType.ParameterValues};
    }

    public class InstanceLogAttribute : DecoratorAttribute
    {
        [Preserve]
        public static void PreAction(string className, string methodName, object @this)
        {
            Debug.Log($"{className}::{methodName} Instance : {@this} ");
        }
        
        [Preserve]
        public static void PreAction(string className, string methodName, object @this, object p1)
        {
            Debug.Log($"{className}::{methodName} Instance : {@this}  Parameters : {p1}");
        }
        
        [Preserve]
        public static void PreAction(string className, string methodName, object @this, object p1, object p2)
        {
            Debug.Log($"{className}::{methodName} Instance : {@this}  Parameters : {p1}, {p2}");
        }
        
        [Preserve]
        public static void PreAction(string className, string methodName, object @this, object p1, object p2, object p3)
        {
            Debug.Log($"{className}::{methodName} Instance : {@this}  Parameters : {p1}, {p2}, {p3}");
        }

        [Preserve]
        public static void PreAction(string className, string methodName, object @this, object p1, object p2, object p3, object p4)
        {
            Debug.Log($"{className}::{methodName} Instance : {@this}  Parameters : {p1}, {p2}, {p3}, {p4}");
        }
        
        [Preserve]
        public static void PreAction(string className, string methodName, object @this, object p1, object p2, object p3, object p4, object p5)
        {
            Debug.Log($"{className}::{methodName} Instance : {@this}  Parameters : {p1}, {p2}, {p3}, {p4}, {p5}");
        }



        public static ParameterType[] PreActionParameterTypes => new[]
            {ParameterType.ClassName, ParameterType.MethodName, ParameterType.This, ParameterType.ParameterValues};
    }
}