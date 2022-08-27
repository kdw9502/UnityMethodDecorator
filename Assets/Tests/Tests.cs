using System.Collections;
using System.Diagnostics;
using System.Threading;
using UnityDecoratorAttribute;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;
using Task = System.Threading.Tasks.Task;

namespace UnityDecoratorAttribute.Tests
{
    public class Tests
    {
        public class TestClass
        {
            [CallCount]
            public void CallCountMethod()
            {
            }

            [ParameterLog]
            public void CallLogMethod()
            {
            }

            [ClampParameter(0, 100)]
            public int ClampParam0to100(int arg)
            {
                return arg;
            }
            
            [ClampParameter(0f, 100f)]
            public float ClampParam0to100(float arg)
            {
                return arg;
            }
            
            [ClampReturn(0, 100)]
            public int ClampReturn0to100(int arg)
            {
                return arg;
            }
            
            [ClampReturn(0, 100)]

            public int Return1001()
            {
                return 1002;
            }

            public void TwoParamLog()
            {
            }

            [ParameterLog]
            public void TwoParamLog(string str1, string str2)
            {
            }

            [ParameterLog]
            public void TwoParamLog(string str, object obj)
            {
            }

            [PerformanceCheck]
            public void PerformanceParamTest()
            {
                Thread.Sleep(100);
            }

            [PerformanceCheck]
            public async Task AsyncTest()
            {
                Debug.Log("test");
                await Task.Delay(1000);
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
            Assert.AreEqual(randomInt, callCount);
            yield return null;
        }

        [UnityTest]
        public IEnumerator CallLogTest()
        {
            var testClass = new TestClass();
            testClass.CallLogMethod();
            LogAssert.Expect(LogType.Log, $"{nameof(TestClass)}::{nameof(TestClass.CallLogMethod)}");
            testClass.TwoParamLog("testString", "asdf");
            LogAssert.Expect(LogType.Log,
                $"{nameof(TestClass)}::{nameof(TestClass.TwoParamLog)} param: testString, asdf");

            testClass.TwoParamLog("testString", 0.01f);
            LogAssert.Expect(LogType.Log,
                $"{nameof(TestClass)}::{nameof(TestClass.TwoParamLog)} param: testString, {0.01f}");
            yield return null;
        }

        [UnityTest]
        public IEnumerator ClampParamTest()
        {
            var testClass = new TestClass();
            int val = testClass.ClampParam0to100(-88);
            Assert.AreEqual(0, val);
            val = testClass.ClampParam0to100(999);
            Assert.AreEqual(100, val);
            val = testClass.ClampParam0to100(10);
            Assert.AreEqual(10, val);
            
            float val2 = testClass.ClampParam0to100(-88f);
            Assert.AreEqual(0f, val2);
            val2 = testClass.ClampParam0to100(999f);
            Assert.AreEqual(100f, val2);
            val2 = testClass.ClampParam0to100(10f);
            Assert.AreEqual(10f, val2);
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator ClampReturnTest()
        {
            var testClass = new TestClass();
            var val = testClass.ClampReturn0to100(-88);
            Assert.AreEqual(0, val);
            val = testClass.ClampReturn0to100(999);
            Assert.AreEqual(100, val);
            val = testClass.ClampReturn0to100(10);
            Assert.AreEqual(10, val);
            val = testClass.Return1001();
            Assert.AreEqual(100, val);
            yield return null;
        }
        [UnityTest]
        public IEnumerator PerformanceParamTest()
        {
            var stopWatch = Stopwatch.StartNew();
            var testClass = new TestClass();
            var executeCount = UnityEngine.Random.Range(1, 10);

            for (int i = 0; i < executeCount; i++)
            {
                testClass.PerformanceParamTest();
            }
            
            var totalExecutionTime = PerformanceCheck.GetTotalExecutionTimeMs(nameof(TestClass), nameof(TestClass.PerformanceParamTest));
            Debug.Log($"exe {totalExecutionTime} stop {stopWatch.ElapsedMilliseconds}");
            Assert.AreApproximatelyEqual(stopWatch.ElapsedMilliseconds, totalExecutionTime, 50);
            Assert.AreEqual(executeCount , PerformanceCheck.GetExecutionCount(nameof(TestClass), nameof(testClass.PerformanceParamTest)));
            var meanExecutionTime =
                PerformanceCheck.GetMeanExecutionTimeMs(nameof(TestClass), nameof(TestClass.PerformanceParamTest));
            Assert.AreApproximatelyEqual(stopWatch.ElapsedMilliseconds / executeCount, meanExecutionTime, 15);
            yield return null;
        }
        //
        // [UnityTest]
        // public IEnumerator AsyncTest()
        // {
        //     var stopWatch = Stopwatch.StartNew();
        //     var testClass = new TestClass();
        //
        //     var task = testClass.AsyncTest();
        //     while (!task.IsCompleted)
        //     {
        //         yield return null;
        //     }
        //
        //     var executionTime = Performance.GetExecutionTime(nameof(TestClass), nameof(TestClass.AsyncTest));
        //     Debug.Log($"exe {executionTime} stop {stopWatch.ElapsedMilliseconds}");
        //     Assert.AreApproximatelyEqual(executionTime, stopWatch.ElapsedMilliseconds, 30);
        // }
    }
}