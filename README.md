# UnityMethodCallCounter
Get count of function call in runtime using dll injection

## Installing

[Download package](https://github.com/kdw9502/UnityMethodCallCounter/releases/download/1.0.0/UnityFunctionCallCounter.unitypackage) and import to your Unity project.

## Example

1. Add MethodCallCount.CallCountAttribute to target method

```c#
using MethodCallCount;
public class AttributeExample : MonoBehaviour
{    
    [CallCount]
    void Start()
    {
    }

    [CallCount]
    void Update()
    {
    }
}

```


2. Dll Injection automatically executed by [PostProcessSceneAttribute](https://docs.unity3d.com/ScriptReference/Callbacks.PostProcessSceneAttribute.html) or [InitializeOnLoadMethod](https://docs.unity3d.com/ScriptReference/InitializeOnLoadMethodAttribute.html) or [PostProcessBuildAttribute](https://docs.unity3d.com/ScriptReference/Callbacks.PostProcessBuildAttribute.html). 

![image](https://user-images.githubusercontent.com/21076531/184492767-88fd2dbd-c231-44e2-b494-f7469893815e.png)

If it doesn't run automatically, Click `UnityMethodCallCounter/Inject Dll` MenuItem On the top window menu.

![image](https://user-images.githubusercontent.com/21076531/184492821-3fedc151-4b76-4e71-bcea-51a3c5ae0e75.png)

3. Call `CallCounter.GetMethodCallCount(typeName, methodName)` to get method call count

```
void GetCountExample()
{
    var count = CallCounter.GetMethodCallCount(nameof(AttributeExample), nameof(Update));
    Debug.Log($"Method Call Count of {nameof(AttributeExample)}, {nameof(Update)} : {count}");
}
```

![image](https://user-images.githubusercontent.com/21076531/184493039-b5fbdf6f-a771-4fde-839b-3eb2632e1b7e.png)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details
