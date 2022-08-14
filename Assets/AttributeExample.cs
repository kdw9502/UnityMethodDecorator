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

    [OneParameterLog]
    static void Param(string a)
    {
        Param(a,"b");
    }

    [TwoParameterLog]
    static void Param(string a, string b)
    {
        Param(a,b,"c");
    }

    [ThreeParameterLog]
    static void Param(string a, string b, string c)
    {
        Param(a,b,c,"d");
    }

    [FourParameterLog]
    static void Param(string a, string b, string c, string d)
    {
    }
}