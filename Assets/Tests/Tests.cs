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

            private int testValue = 0;
            
            [PerformanceCheck]
            public void PostActionBranch()
            {
                testValue++;
                if (testValue > 5)
                {
                    Debug.Log($"Do Something");
                }
            }

            [PerformanceCheck]
            public int ManyReturnPostActionBranch(int a)
            {
                if (a == 0)
                {
                    return 1;
                }

                if (a == 1)
                {
                    return 2;
                }

                if (a == 2)
                {
                    return 3;
                }

                if (a == 3)
                {
                    return 1;
                }

                return 0;
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
                $"{nameof(TestClass)}::{nameof(TestClass.TwoParamLog)} Parameters : testString, asdf");

            testClass.TwoParamLog("testString", 0.01f);
            LogAssert.Expect(LogType.Log,
                $"{nameof(TestClass)}::{nameof(TestClass.TwoParamLog)} Parameters : testString, {0.01f}");
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
            Debug.Log($"totalExecutionTime {totalExecutionTime} stopwatch {stopWatch.ElapsedMilliseconds}");
            Assert.AreApproximatelyEqual(stopWatch.ElapsedMilliseconds, totalExecutionTime, 50);
            Assert.AreEqual(executeCount , PerformanceCheck.GetExecutionCount(nameof(TestClass), nameof(testClass.PerformanceParamTest)));
            var meanExecutionTime =
                PerformanceCheck.GetMeanExecutionTimeMs(nameof(TestClass), nameof(TestClass.PerformanceParamTest));
            Debug.Log($"meanExecutionTime {meanExecutionTime} stopwatch {stopWatch.ElapsedMilliseconds / executeCount}");

            Assert.AreApproximatelyEqual(stopWatch.ElapsedMilliseconds / executeCount, meanExecutionTime, 15);
            yield return null;
        }

        // fixed in 740b024a976fa0b5101beb414b383d4a5c66e519
        // if some il branch to ret, PostAction was not called  
        [UnityTest]
        public IEnumerator BranchTest()
        {
            var testClass = new TestClass();
            var executeCount = UnityEngine.Random.Range(6, 20);
            for (int i = 0; i < executeCount; i++)
            {
                testClass.PostActionBranch();
            }
            Assert.AreEqual(executeCount, PerformanceCheck.GetExecutionCount(nameof(TestClass), nameof(TestClass.PostActionBranch)));
            yield return null;

        }
        
        [UnityTest]
        public IEnumerator BranchTest2()
        {
            var testClass = new TestClass();
            var executeCount = UnityEngine.Random.Range(6, 20);
            for (int i = 0; i < executeCount; i++)
            {
                testClass.ManyReturnPostActionBranch(i);
            }
            Assert.AreEqual(executeCount, PerformanceCheck.GetExecutionCount(nameof(TestClass), nameof(TestClass.ManyReturnPostActionBranch)));
            yield return null;

        }
        
    }
}