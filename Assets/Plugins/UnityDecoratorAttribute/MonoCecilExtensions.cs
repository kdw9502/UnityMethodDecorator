using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

// https://stackoverflow.com/questions/4184384/mono-cecil-typereference-to-type
public static class MonoCecilExtensions
{
    private static Dictionary<TypeReference, Type> cache = new();
    public static Type GetMonoType(this TypeReference type)
    {
        var returnValue = cache.GetValueOrDefault(type) ?? Type.GetType(type.GetReflectionName(), false);
        cache[type] = returnValue;
        return returnValue;
    }

    private static string GetReflectionName(this TypeReference type)
    {
        if (!type.IsGenericInstance) 
            return type.FullName;
            
        var genericInstance = (GenericInstanceType)type;
        return
            $"{genericInstance.Namespace}.{type.Name}[{string.Join(",", genericInstance.GenericArguments.Select(p => p.GetReflectionName()).ToArray())}]";
    }
}
