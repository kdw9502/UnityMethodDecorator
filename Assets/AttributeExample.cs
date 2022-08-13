using System.Collections;
using System.Collections.Generic;
using MethodCallCount;
using UnityEditor;
using UnityEngine;

public class AttributeExample : MonoBehaviour
{    
    [CallCount]
    void Start()
    {
        Application.targetFrameRate = 60;
    }

    [CallCount]
    void Update()
    {
    }

    [MenuItem("UnityMethodCallCounter/Test")]
    static void GetCountExample()
    {
        var count = CallCounter.GetMethodCallCount(nameof(AttributeExample), nameof(Update));
        Debug.Log($"Method Call Count of {nameof(AttributeExample)}, {nameof(Update)} : {count}");
    }
}
