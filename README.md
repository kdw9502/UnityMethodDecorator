# UnityDecoratorAttribute

UnityDecoratorAttribute is a library for adding custom actions to methods using attributes, similar to Python's @decorator. This library is implemented using Mono.Cecil.

By adding the DecoratorAttribute to a method, you can execute custom actions at the beginning and end of the method.
## Installation

To use UnityDecoratorAttribute, download the package from [this GitHub repository](https://github.com/kdw9502/UnityDecoratorAttribute/releases)  and import it into your Unity project.
# Usage
## Predefined Example Attributes

UnityDecoratorAttribute comes with several predefined example attributes that you can use to add custom actions to your methods: 
- [IgnoreExceptionAttribute](Assets/Plugins/UnityDecoratorAttribute/IgnoreExceptionAttribute.cs) : Ignores exceptions and returns a default value when an exception is raised. 
- [IgnoreNullExceptionAttribute](Assets/Plugins/UnityDecoratorAttribute/Examples/IgnoreNullException.cs) : Only ignores NullReferenceExceptions. This is an example of an attribute that inherits from IgnoreExceptionAttribute. 
- [ParameterLogAttribute](Assets/Plugins/UnityDecoratorAttribute/Examples/CallLog.cs) : Logs parameter values when the method is called.

```c#
[ParameterLog]
public void TestParameterLog(int a, float b, Text text)
{
}

```

 
- [InstanceLogAttribute](Assets/Plugins/UnityDecoratorAttribute/Examples/CallLog.cs) : Logs parameter values and instance.ToString() when the method is called.

```c#
[InstanceLog]
public void TestInstanceLog(int a)
{
}

```


### Result

![image](https://user-images.githubusercontent.com/21076531/187033810-063e7924-224d-4277-a2ae-12b05bd04dfb.png) 
 
- [CallCounterAttribute](Assets/Plugins/UnityDecoratorAttribute/Examples/CallCounter.cs) : Stores the number of times a method is called and returns the count using `CallCounter.GetMethodCallCount(className, methodName)`.

![image](https://user-images.githubusercontent.com/21076531/184547638-25deef6e-2d46-461b-98a7-139ec116c122.png) 
 
- [PerformanceCheckAttribute](Assets/Plugins/UnityDecoratorAttribute/Examples/PerformanceCheck.cs) : Stores the total execution time and count.

`PerformanceCheck.GetExecutionCount(className, methodName)`: Returns the number of times the method has been executed (called).

`PerformanceCheck.GetTotalExecutionTimeMs(className, methodName)`: Returns the total execution time of the method in milliseconds.

`PerformanceCheck.GetMeanExecutionTimeMs(className, methodName)`: Returns the mean execution time of the method in milliseconds.

```c#
[PerformanceCheck]
public void Update()
{
    elapsedTime += Time.deltaTime;
    if (elapsedTime <= 5) 
        return;

    Debug.Log($"GetExecutionCount: {PerformanceCheck.GetExecutionCount(nameof(AttributeExample), nameof(Update))}");
    Debug.Log($"GetTotalExecutionTimeMs: {PerformanceCheck.GetTotalExecutionTimeMs(nameof(AttributeExample), nameof(Update))} ms");
    Debug.Log($"GetMeanExecutionTimeMs: {PerformanceCheck.GetMeanExecutionTimeMs(nameof(AttributeExample), nameof(Update))} ms");
    elapsedTime = 0;
}

```



![image](https://user-images.githubusercontent.com/21076531/187035466-d63a8c20-6ef9-4962-8468-616d13903928.png) 
 
- [ClampParameterAttribute](Assets/Plugins/UnityDecoratorAttribute/Examples/ChangeParameter.cs) : Clamps the value of the first parameter. 
- [ClampReturnAttribute](Assets/Plugins/UnityDecoratorAttribute/Examples/ChangeParameter.cs) : Clamps the return value.
## Creating a Custom Attribute

To create a custom attribute, you need to inherit from `UnityDecoratorAttribute.DecoratorAttribute`:

```
c#public class ExampleAttribute : DecoratorAttribute
{
}

```


### PreAction

The `PreAction` method is always called when the target method is called.
#### Defining PreAction

To define the `PreAction` method in your attribute, create a static method called `PreAction`. If your `PreAction` method needs parameters, define a field called `PreActionParameterTypes` with the required parameter types.
##### Example

```c#
public class DebugLogAttribute : DecoratorAttribute
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


### PostAction

The `PostAction` method is called before the target method returns.
#### Defining PostAction

To define the `PostAction` method in your attribute, create a static method called `PostAction`. If your `PostAction` method needs parameters, define a field called `PostActionParameterTypes` with the required parameter types.
##### Example

```c#
public class ClampReturnAttribute : DecoratorAttribute
{
    public ClampReturn(int min, int max)
    {
    }
    
    [Preserve]
    public static void PostAction(ref int @return, int min, int max)
    {
        @return = Math.Clamp(@return, min, max);
    }

    public static ParameterType[] PostActionParameterTypes =>
        new[] {ParameterType.ReturnValue, ParameterType.AttributeValues};
}

```



The `PostAction` method can take parameters of different types. You can use the `ParameterType` enum to specify the type of each parameter.

```c#
public enum ParameterType
{
    ClassName,
    MethodName,
    This,  // Target Method's Instance
    ParameterValues, // Target Method's Parameter Values (only for PreAction)
    ReturnValue, //  Target Method's ReturnValue (only for PostAction)
    AttributeValues, // Attribute values
}

```


## Limitations

This library does not support coroutines or async methods.
## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE)  file for details.
