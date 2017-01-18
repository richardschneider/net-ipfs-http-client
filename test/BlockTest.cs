using Ipfs.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Ipfs.Api
{

    [TestClass]
    public class BlockTest
    {
        byte[] someBytes = new byte[] { 1, 2, 3 };

        [TestMethod]
        public void DataBytes()
        {
            var block = new Block
            {
                DataBytes = someBytes
            };
            CollectionAssert.AreEqual(someBytes, block.DataBytes);
        }

        [TestMethod]
        public void DataStream()
        {
            var block = new Block
            {
                DataBytes = someBytes
            };
            var stream = block.DataStream;
            Assert.AreEqual(1, stream.ReadByte());
            Assert.AreEqual(2, stream.ReadByte());
            Assert.AreEqual(3, stream.ReadByte());
            Assert.AreEqual(-1, stream.ReadByte(), "at eof");
        }

    }
}
