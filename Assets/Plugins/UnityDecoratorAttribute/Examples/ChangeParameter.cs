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

        public static PreActionParameterType[] PreActionParameterTypes => new[] {PreActionParameterType.ParameterValues, PreActionParameterType.AttributeValues};
    }
}