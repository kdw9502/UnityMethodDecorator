using System.Collections;
using System.Collections.Generic;
using UnityDecoratorAttribute;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

public class Tests
{
    public class TestClass
    {
        [CallCount]
        public void CallCountMethod()
        {
        }
        [CallLog]
        public void CallLogMethod()
        {
        }

        [ClampParameterInt(0, 100)]
        public int Clamp0to100(int arg)
        {
            return arg;
        }

        
        public void TwoParamLog()
        {
            
        }
        [TwoParameterLog]
        public void TwoParamLog(string str1, string str2)
        {
        }
        [TwoParameterLog]
        public void TwoParamLog(string str, float _float)
        {
        }
    }   
    
    [UnityTest]
    public IEnumerator CallCountTest()
    {
        var testClass = new TestClass();
        var randomInt = UnityEngine.Random.Range(0, 100);
        for (int i = 0; i < randomInt; i++)
        {
            testClass.CallCountMethod();
        }

        var callCount = CallCounter.GetMethodCallCount(nameof(TestClass), nameof(TestClass.CallCountMethod));
        Assert.AreEqual(randomInt,callCount);
        yield return null;
    }
    [UnityTest]
    public IEnumerator CallLogTest()
    {
        var testClass = new TestClass();
        testClass.CallLogMethod();

        LogAssert.Expect(LogType.Log, $"{nameof(TestClass)}::{nameof(TestClass.CallLogMethod)}");
        yield return null;
    }
    
    [UnityTest]
    public IEnumerator TwoParamLogTest()
    {
        var testClass = new TestClass();
        testClass.TwoParamLog("testString", "asdf");
        LogAssert.Expect(LogType.Log, $"{nameof(TestClass)}::{nameof(TestClass.TwoParamLog)} param: testString, asdf");
        
        testClass.TwoParamLog("testString", 0.01f);
        LogAssert.Expect(LogType.Log, $"{nameof(TestClass)}::{nameof(TestClass.TwoParamLog)} param: testString, {0.01f}");
        yield return null;
    }


    [UnityTest]
    public IEnumerator ClampParamTest()
    {
        var testClass = new TestClass();
        var val = testClass.Clamp0to100(-88);
        Assert.AreEqual(0, val);
        val = testClass.Clamp0to100(999);
        Assert.AreEqual(100, val);
        yield return null;
    }

}