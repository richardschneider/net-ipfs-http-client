using Ipfs.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Ipfs.Api
{

    [TestClass]
    public class PinnedObjectTest
    {

        [TestMethod]
        public void Stringify()
        {
            var pin = new PinnedObject
            {
                Id = "QmStfpa7ppKPSsdnazBy3Q5QH4zNzGLcpWV88otjVSV7SY",
                Mode = PinMode.Recursive
            };
            Assert.AreEqual("QmStfpa7ppKPSsdnazBy3Q5QH4zNzGLcpWV88otjVSV7SY Recursive", pin.ToString());
        }

    }
}
