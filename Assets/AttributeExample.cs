using System.Collections;
using System.Collections.Generic;
using UnityDecoratorAttribute;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

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
    
#if UNITY_EDITOR
    [MenuItem("UnityMethodCallCounter/Test")]
#endif
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