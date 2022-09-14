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
        public static Exception exception = new Exception("Test Exception");
        
        public class TestClass
        {
            [IgnoreException]
            public void Throw()
            {
                
                throw exception;
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
                if (ex == exception)
                    Assert.IsTrue(false);
                else
                    throw;
            }
            
            LogAssert.Expect(LogType.Error, exception.ToString());


            yield return null;
        }
    }
}