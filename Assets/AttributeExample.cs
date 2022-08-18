using System;
using System.Collections;
using System.Collections.Generic;
using UnityDecoratorAttribute;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

public class AttributeExample : MonoBehaviour
{
    [ZeroParameterLog]
    private void Start()
    {
        
        Clamp(-99);        
    }
    
    // [CallCount]
    private void Update()
    {
    }

    // [TwoParameterLog]
    private void ParameterLogExample(string a, string b)
    {
    }
    
#if UNITY_EDITOR
    [MenuItem("UnityMethodCallCounter/Test")]
#endif
    static void GetUpdateCallCount()
    {

    }
    
    static void Param(string a)
    {
    }

    private int A(int a, int b)
    {
        int returnValue = 2;
        Debug.Log(returnValue);
        
        return Clamp(returnValue);
    }
    [ClampParameterInt(0, 100)]
    private int Clamp(int a)
    {
        
        Debug.Log(a);
        return a;
    }
    private int Clamp2(int a)
    {
        return a;
    }
}