using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;
using UnityDecoratorAttribute;
namespace UnityDecoratorAttribute.Tests
{
    public class TryCatchTest
    {
        public class TestClass
        {
            [IgnoreException]
            public void Throw()
            {
                throw new Exception("Test Exception");
            }
        }
        
        [UnityTest]
        public IEnumerator ThrowTest()
        {
            var testClass = new TestClass();
            try
            {
                testClass.Throw();
            }
            catch (Exception ex)
            {
                Assert.IsTrue(false);
            }
            
            LogAssert.Expect(LogType.Log, $"Test Exception");


            yield return null;
        }
    }
}