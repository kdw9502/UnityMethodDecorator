using System.Collections;
using System.Collections.Generic;
using UnityDecoratorAttribute;
using UnityEditor;
using UnityEngine;

public class AttributeExample : MonoBehaviour
{
    [ZeroParameterLog]
    private void Start()
    {
        Debug.Log("start " + this);
        
        ParameterLogExample("test a", "test b");
    }

    [TwoParameterLog]
    private void ParameterLogExample(string a, string b)
    {
        Debug.Log("ParameterLogExample");
    }

    // [MenuItem("UnityMethodCallCounter/Test")]
    // static void Example()
    // {
    //     Param("a");
    // }
    //
    // [StaticOneParameterLog]
    // static void Param(string a)
    // {
    //     Param(a,"b");
    // }
    //
    // [StaticTwoParameterLog]
    // static void Param(string a, string b)
    // {
    //     Param(a,b,"c");
    // }
    //
    // [StaticThreeParameterLog]
    // static void Param(string a, string b, string c)
    // {
    //     Param(a,b,c,"d");
    // }
    //
    // [StaticFourParameterLog]
    // static void Param(string a, string b, string c, string d)
    // {
    // }
     
}