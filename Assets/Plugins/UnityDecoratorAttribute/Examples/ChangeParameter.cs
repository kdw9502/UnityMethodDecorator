using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace UnityDecoratorAttribute
{
    public class ClampParameter : DecoratorAttribute
    {
        public ClampParameter(int min, int max)
        {
        }

        public ClampParameter(long min, long max)
        {
        }

        public ClampParameter(float min, float max)
        {
        }

        public ClampParameter(double min, double max)
        {
        }
        [Preserve]
        public static void PreAction(ref int param, int min, int max)
        {
            param = Math.Clamp(param, min, max);
        }
        [Preserve]
        public static void PreAction(ref long param, long min, long max)
        {
            param = Math.Clamp(param, min, max);
        }
        [Preserve]
        public static void PreAction(ref float param, float min, float max)
        {
            param = Math.Clamp(param, min, max);
        }
        [Preserve]
        public static void PreAction(ref double param, double min, double max)
        {
            param = Math.Clamp(param, min, max);
        }
        public static ParameterType[] PreActionParameterTypes =>
            new[] {ParameterType.ParameterValues, ParameterType.AttributeValues};
    }


    public class ClampReturn : DecoratorAttribute
    {
        public ClampReturn(int min, int max)
        {
        }

        public ClampReturn(long min, long max)
        {
        }

        public ClampReturn(float min, float max)
        {
        }

        public ClampReturn(double min, double max)
        {
        }

        [Preserve]
        public static void PostAction(ref int @return, int min, int max)
        {
            @return = Math.Clamp(@return, min, max);
        }

        [Preserve]
        public static void PostAction(ref long @return, long min, long max)
        {
            @return = Math.Clamp(@return, min, max);
        }

        [Preserve]
        public static void PostAction(ref float @return, float min, float max)
        {
            @return = Math.Clamp(@return, min, max);
        }

        [Preserve]
        public static void PostAction(ref double @return, double min, double max)
        {
            @return = Math.Clamp(@return, min, max);
        }

        public static ParameterType[] PostActionParameterTypes =>
            new[] {ParameterType.ReturnValue, ParameterType.AttributeValues};
    }
}