using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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

            [IgnoreNullException]
            public int NullException()
            {
                TryCatchTest a = null;

                return a.GetHashCode();
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
        
        [UnityTest]
        public IEnumerator IgnoreNullExceptionTest()
        {
            var testClass = new TestClass();
            try
            {
                var result = testClass.NullException();
                Assert.AreEqual(0, result);
                throw exception;
            }
            catch (Exception ex)
            {
                if (ex != exception)
                    Assert.IsTrue(false);
            }
            LogAssert.Expect(LogType.Error, new Regex(".*System.NullReferenceException.*"));
            

            yield return null;
        }
    }
}