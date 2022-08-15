using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace UnityDecoratorAttribute
{
    public class ClampPercentageAttribute: DecoratorAttribute
    {
        [Preserve]
        public static void PreAction(ref int param)
        {
            param = Math.Clamp(param, 0, 100);
        }

        public static PreActionParameterType[] ParameterTypes => new[] {PreActionParameterType.ParameterValues};
    }
}