# WIP

currently not fully implemented and tested.

# UnityDecoratorAttribute

Python style decorator, which is add action to method with attribute, implement using dll injection.

By adding DecoratorAttribute to method, you can execute custom actions on beginning of method and end of method.

## Install

[Download package](https://github.com/kdw9502/UnityDecoratorAttribute/releases) and import to your Unity project.

## Usage

### PreDefined Example Attributes
[ClampParameterInt, ClampParameterFloat](Assets/Plugins/UnityDecoratorAttribute/Examples/ChangeParameter.cs) : Clamp first parameter value.

[ClampReturnInt, ClampReturnFloat](Assets/Plugins/UnityDecoratorAttribute/Examples/ChangeParameter.cs) : Clamp return value.

[LogThis](Assets/Plugins/UnityDecoratorAttribute/Examples/CallLog.cs) : Call `Debug.Log($"{this} {className}::{methodName}")` on target method called.

[OneParameterLogAttribute ~ FourParameterLogAttribute](Assets/Plugins/UnityDecoratorAttribute/Examples/CallLog.cs) : Log parameter values.

[CallCount](Assets/Plugins/UnityDecoratorAttribute/Examples/CallCounter.cs) : Store the number of target method calls and get count by `CallCounter.GetMethodCallCount(className, methodName)`

etc.

### Make Custom attribute

Create Attribute inherit UnityDecoratorAttribute.DecoratorAttribute
```c#
public class ExampleAttribute : DecoratorAttribute
{
}
```

### PreAction 

PreAction is always called when target method is called.

#### Define

Define method `public static void PreAction` Attribute. 

If your PreAction needs parameter, Define field `public static PreActionParameterType[] PreActionParameterTypes`

```c#
public class ExampleAttribute : DecoratorAttribute
{
    [Preserve]
    public static void PreAction(string className, string methodName)
    {
        Debug.Log($"{className}::{methodName}");
    }

    public static PreActionParameterType[] PreActionParameterTypes => 
        new[] {PreActionParameterType.ClassName, PreActionParameterType.MethodName};
}
```


PreAction can use following parameters.
```c#
public enum PreActionParameterType
{
    ClassName,
    MethodName,
    This,  // Target Method's Instance
    ParameterValues, // Target Method's Parmeter Values
    AttributeValues, // Attribute values
}
```


### PostAction 

PostAction is called before target method return.

#### Define

Define method `public static void PostAction` Attribute. 

If your PostAction needs parameter, Define field `public static PostActionParameterType[] PostActionParameterTypes`

PostAction can use following parameters.
```
public enum PostActionParameterType
{
    ClassName,
    MethodName,
    This,
    ReturnValue,
    AttributeValues,
}
```

## Limitation

Not support for async methods.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details
