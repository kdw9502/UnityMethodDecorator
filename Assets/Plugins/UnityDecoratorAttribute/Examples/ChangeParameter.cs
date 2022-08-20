using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace UnityDecoratorAttribute
{
    public class ClampParameterInt: DecoratorAttribute
    {
        public ClampParameterInt(int min, int max)
        {
        }
        
        [Preserve]
        public static void PreAction(ref int param, int min, int max)
        {
            param = Math.Clamp(param, min, max);
        }

        public static PreActionParameterType[] PreActionParameterTypes => 
            new[] {PreActionParameterType.ParameterValues, PreActionParameterType.AttributeValues};
    }
    
    
    public class ClampParameterFloat: DecoratorAttribute
    {
        public ClampParameterFloat(float min, float max)
        {
        }
        
        [Preserve]
        public static void PreAction(ref float param, float min, float max)
        {
            param = Math.Clamp(param, min, max);
        }

        public static PreActionParameterType[] PreActionParameterTypes => 
            new[] {PreActionParameterType.ParameterValues, PreActionParameterType.AttributeValues};
    }
    
    public class ClampReturnInt: DecoratorAttribute
    {
        public ClampReturnInt(int min, int max)
        {
        }
        
        [Preserve]
        public static int PostAction(int param, int min, int max)
        {
            return Math.Clamp(param, min, max);
        }

        public static PostActionParameterType[] PostActionParameterTypes => 
            new[] {PostActionParameterType.ReturnValue, PostActionParameterType.AttributeValues};
    }
    
}