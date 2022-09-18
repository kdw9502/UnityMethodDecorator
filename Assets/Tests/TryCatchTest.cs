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
            
            [IgnoreExceptionWithoutLog]
            public int ReturnValueOrException(int a)
            {
                return new[] {0, 1, 2, 3}[a];
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
        
        [UnityTest]
        public IEnumerator IgnoreExceptionReturnValueTest()
        {
            var testClass = new TestClass();

            var result = testClass.ReturnValueOrException(0);
            Assert.AreEqual(0, result);
            result = testClass.ReturnValueOrException(1);
            Assert.AreEqual(1, result);
            result = testClass.ReturnValueOrException(2);
            Assert.AreEqual(2, result);
            result = testClass.ReturnValueOrException(3);
            Assert.AreEqual(3, result);
            result = testClass.ReturnValueOrException(4);
            Assert.AreEqual(0, result);
            result = testClass.ReturnValueOrException(5);
            Assert.AreEqual(0, result);
            
            yield return null;
        }
    }
}