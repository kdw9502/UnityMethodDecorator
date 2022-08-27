using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace UnityDecoratorAttribute
{
    public class ClampParameter: DecoratorAttribute
    {
        public ClampParameter(int min, int max)
        {
        }
        
        public ClampParameter(float min, float max)
        {
        }
        
        [Preserve]
        public static void PreAction(ref int param, int min, int max)
        {
            param = Math.Clamp(param, min, max);
        }
        
        [Preserve]
        public static void PreAction(ref float param, float min, float max)
        {
            param = Math.Clamp(param, min, max);
        }

        public static ParameterType[] PreActionParameterTypes => 
            new[] {ParameterType.ParameterValues, ParameterType.AttributeValues};
    }
    
    
    public class ClampReturnInt: DecoratorAttribute
    {
        public ClampReturnInt(int min, int max)
        {
        }
        
        [Preserve]
        public static void PostAction(ref int param, int min, int max)
        {
            param = Math.Clamp(param, min, max);
        }

        public static ParameterType[] PostActionParameterTypes => 
            new[] {ParameterType.ReturnValue, ParameterType.AttributeValues};
    }
    
}