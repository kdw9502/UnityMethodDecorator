# WIP

currently not fully implemented and tested

# UnityDecoratorAttribute

Python style decorator (add action to method with attribute) implement using dll injection.

## Installing

[Download package](https://github.com/kdw9502/UnityDecoratorAttribute/releases/download/2.0.0/UnityDecoratorAttribute.unitypackage) and import to your Unity project.

## Usage

### Parameter Logging

```c#
using UnityDecoratorAttribute;
public class AttributeExample : MonoBehaviour
{
    [ZeroParameterLog]
    private void Start()
    {
        ParameterLogExample("test a", "test b");
    }

    [TwoParameterLog]
    private void ParameterLogExample(string a, string b)
    {
    }
}
```
Add `(Zero~Four)ParameterLogAttribute` to target method. 

To log static method, use `Static(Zero~Four)ParameterLogAttribute` instead.

### Result

![image](https://user-images.githubusercontent.com/21076531/184547089-a75fba5b-e9e7-4131-af9f-54dbcbd0fe51.png)

If you want to use custom format, edit ParameterLogAttribute::PreAction() or create attribute with same static method.

### Method Call Count

```c#
[CallCount]
private void Update()
{
}
```
Add `CallCountAttribute` to target method.

```c#
static void GetUpdateCallCount()
{
    int callCount = CallCounter.GetMethodCallCount(nameof(AttributeExample), nameof(Update));
    Debug.Log($"Update Call Count : {callCount}");
}
```
To get number of method calls, call `CallCounter.GetMethodCallCount(className, methodName)`

### Result

![image](https://user-images.githubusercontent.com/21076531/184547638-25deef6e-2d46-461b-98a7-139ec116c122.png)


## Limitation
Currently only support for method in [Assembly-CSharp.dll](https://docs.unity3d.com/2019.4/Documentation/Manual/ScriptCompilationAssemblyDefinitionFiles.html)

Not support for ref parameter.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details
