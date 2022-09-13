# WIP

currently not fully implemented and tested.

# UnityDecoratorAttribute

Like Python's @decorator, add custom action to method with attribute.(implemented using dll injection.)

By adding DecoratorAttribute to method, you can execute custom actions on beginning of method and end of method.

## Install

[Download package](https://github.com/kdw9502/UnityDecoratorAttribute/releases) and import to your Unity project.

# Usage

## PreDefined Example Attributes

[PrameterLog](Assets/Plugins/UnityDecoratorAttribute/Examples/CallLog.cs) : Log parameter values when the method is called.
```c#
[ParameterLog]
public void TestParameterLog(int a, float b, Text text)
{
}
```
[InstanceLog](Assets/Plugins/UnityDecoratorAttribute/Examples/CallLog.cs) : Log parameter values and instance.ToString() when the method called.
```c#
[InstanceLog]
public void TestInstanceLog(int a)
{
}

```
### Result

![image](https://user-images.githubusercontent.com/21076531/187033810-063e7924-224d-4277-a2ae-12b05bd04dfb.png)

[CallCount](Assets/Plugins/UnityDecoratorAttribute/Examples/CallCounter.cs) : Store the number of method call and get count by `CallCounter.GetMethodCallCount(className, methodName)`

![image](https://user-images.githubusercontent.com/21076531/184547638-25deef6e-2d46-461b-98a7-139ec116c122.png)

[PerformanceCheckAttribute](Assets/Plugins/UnityDecoratorAttribute/Examples/PerformanceCheck.cs) : Store total execution time and count.

`PerformanceCheck.GetExecutionCount(className, methodName)` : Return method execution (call) Count

`PerformanceCheck.GetTotalExecutionTimeMs(className, methodName)` : return total execution time of method in milliseconds

`PerformanceCheck.GetMeanExecutionTimeMs(className, methodName)` : return mean execution time of method in milliseconds
```c#
[PerformanceCheck]
public void Update()
{
    elapsedTime += Time.deltaTime;
    if (elapsedTime <= 5) 
        return;

    Debug.Log($"GetExecutionCount : {PerformanceCheck.GetExecutionCount(nameof(AttributeExample), nameof(Update))}");
    Debug.Log($"GetTotalExecutionTimeMs : {PerformanceCheck.GetTotalExecutionTimeMs(nameof(AttributeExample), nameof(Update))} ms");
    Debug.Log($"GetMeanExecutionTimeMs : {PerformanceCheck.GetMeanExecutionTimeMs(nameof(AttributeExample), nameof(Update))} ms");
    elapsedTime = 0;
}
```
![image](https://user-images.githubusercontent.com/21076531/187035466-d63a8c20-6ef9-4962-8468-616d13903928.png)

[ClampParameter](Assets/Plugins/UnityDecoratorAttribute/Examples/ChangeParameter.cs) : Clamp first parameter value.

[ClampReturn](Assets/Plugins/UnityDecoratorAttribute/Examples/ChangeParameter.cs) : Clamp return value.

etc.


## Make Custom attribute

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

If your PreAction needs parameter, Define field `public static ParameterType[] PreActionParameterTypes`

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

PostAction is called before target method return.

#### Define

Define method `public static void PostAction` Attribute. 

If your PostAction needs parameter, Define field `public static ParameterType[] PostActionParameterTypes`

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



## Limitation

Not support for async methods.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details
