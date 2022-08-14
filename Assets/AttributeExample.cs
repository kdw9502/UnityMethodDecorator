using System.Collections;
using System.Collections.Generic;
using MethodCallCount;
using UnityEditor;
using UnityEngine;

public class AttributeExample : MonoBehaviour
{
    [ZeroParameterLog]
    void Start()
    {
        Application.targetFrameRate = 60;
        Debug.Log(name);
    }

    [CallCount]
    void Update()
    {
    }

    [MenuItem("UnityMethodCallCounter/Test")]
    static void Example()
    {
        Param("a");
    }

    [StaticOneParameterLog]
    static void Param(string a)
    {
        Param(a,"b");
    }

    [StaticTwoParameterLog]
    static void Param(string a, string b)
    {
        Param(a,b,"c");
    }

    [StaticThreeParameterLog]
    static void Param(string a, string b, string c)
    {
        Param(a,b,c,"d");
    }

    [StaticFourParameterLog]
    static void Param(string a, string b, string c, string d)
    {
    }
     
}