using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ipfs.Api
{
    /// <summary>
    ///   Asserting an <see cref="Exception"/>.
    /// </summary>
    public static class ExceptionAssert
    {

        public static T Throws<T>(Action action, string expectedMessage = null) where T : Exception
        {
            try
            {
                action();
            }
            catch (T e)
            {
                if (expectedMessage != null)
                    Assert.AreEqual(expectedMessage, e.Message, "Wrong exception message.");
                return e;
            }
            Assert.Fail("Exception of type {0} should be thrown.", typeof(T));

            //  The compiler doesn't know that Assert.Fail will always throw an exception
            return null;
        }

        public static Exception Throws(Action action, string expectedMessage = null)
        {
            return Throws<Exception>(action, expectedMessage);
        }

    }
}
