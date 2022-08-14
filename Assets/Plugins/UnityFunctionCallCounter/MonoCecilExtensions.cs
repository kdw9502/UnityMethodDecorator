using System;
using System.Linq;
using Mono.Cecil;

// https://stackoverflow.com/questions/4184384/mono-cecil-typereference-to-type
public static class MonoCecilExtensions
{
    public static Type GetMonoType(this TypeReference type)
    {
        return Type.GetType(type.GetReflectionName(), true);
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
