using System;
using System.Collections;
using System.Collections.Generic;
using UnityDecoratorAttribute;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class AttributeExample : MonoBehaviour
{
    [ParameterLog]
    public void TestParameterLog(int a, float b, Text text)
    {
    }

    [InstanceLog]
    public void TestInstanceLog(int a)
    {
    }

    private float elapsedTime = 0;
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


    public void Start()
    {
        TestParameterLog(1,2.1f, gameObject.AddComponent<Text>());
        TestInstanceLog(3);
    }
}