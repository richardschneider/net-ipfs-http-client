using Ipfs.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.IO;

namespace Ipfs.Api
{
    
    [TestClass]
    public partial class MerkleNodeTest
    {
        const string IpfsInfo = "QmVtU7ths96fMgZ8YSZAbKghyieq7AjxNdcqyVzxTt3qVe";

        [TestMethod]
        public void HashWithNamespace()
        {
            var node = new MerkleNode("/ipfs/" + IpfsInfo);
            Assert.AreEqual(IpfsInfo, node.Hash);
        }

        [TestMethod]
        public void Stringify()
        {
            var node = new MerkleNode(IpfsInfo);
            Assert.AreEqual("/ipfs/" + IpfsInfo, node.ToString());
        }

        [TestMethod]
        public void FromString()
        {
            var a = new MerkleNode(IpfsInfo);
            var b = (MerkleNode)IpfsInfo;
            Assert.AreEqual(a, b);
        }

        [TestMethod]
        public void NullHash()
        {
            ExceptionAssert.Throws<ArgumentNullException>(() => new MerkleNode((string)null));
            ExceptionAssert.Throws<ArgumentNullException>(() => new MerkleNode(""));
        }

        [TestMethod]
        public void Stats()
        {
            var node = new MerkleNode(IpfsInfo);
            Assert.AreEqual(309, node.BlockSize);
            Assert.AreEqual(307, node.LinksSize);
            Assert.AreEqual(6, node.LinksCount);
            Assert.AreEqual(2, node.DataSize);
            Assert.AreEqual(6345, node.CumulativeSize);
        }

        [TestMethod]
        public void Links_and_LinksCount()
        {
            var node = new MerkleNode(IpfsInfo);
            Assert.AreEqual(6, node.LinksCount);
            Assert.AreEqual(6, node.Links.Count());
        }

        [TestMethod]
        public void FromALink()
        {
            var node = new MerkleNode(IpfsInfo);
            var link = new MerkleNode(node.Links.First());
            Assert.AreEqual(link.Hash, node.Links.First().Hash);
            Assert.AreEqual(link.Name, node.Links.First().Name);
            Assert.AreEqual(link.BlockSize, node.Links.First().Size);
        }

        [TestMethod]
        public void ToALink()
        {
            var node = new MerkleNode(IpfsInfo);
            var link = node.ToLink();
            Assert.AreEqual(link.Hash, node.Hash);
            Assert.AreEqual(link.Name, node.Name);
            Assert.AreEqual(link.Size, node.BlockSize);

        }

        [TestMethod]
        public void Value_Equality()
        {
            var a0 = new MerkleNode("QmStfpa7ppKPSsdnazBy3Q5QH4zNzGLcpWV88otjVSV7SY");
            var a1 = new MerkleNode("QmStfpa7ppKPSsdnazBy3Q5QH4zNzGLcpWV88otjVSV7SY");
            var b = new MerkleNode("QmagNHT6twJRBZcGeviiGzHVTMbNnJZameLyL6T14GUHCS");
            MerkleNode nullNode = null;

            #pragma warning disable 1718
            Assert.IsTrue(a0 == a0);
            Assert.IsTrue(a0 == a1);
            Assert.IsFalse(a0 == b);
            Assert.IsFalse(a0 == null);

            #pragma warning disable 1718
            Assert.IsFalse(a0 != a0);
            Assert.IsFalse(a0 != a1);
            Assert.IsTrue(a0 != b);
            Assert.IsTrue(a0 != null);

            Assert.IsTrue(a0.Equals(a0));
            Assert.IsTrue(a0.Equals(a1));
            Assert.IsFalse(a0.Equals(b));
            Assert.IsFalse(a0.Equals(null));

            Assert.AreEqual(a0, a0);
            Assert.AreEqual(a0, a1);
            Assert.AreNotEqual(a0, b);
            Assert.AreNotEqual(a0, null);

            Assert.AreEqual<MerkleNode>(a0, a0);
            Assert.AreEqual<MerkleNode>(a0, a1);
            Assert.AreNotEqual<MerkleNode>(a0, b);
            Assert.AreNotEqual<MerkleNode>(a0, null);

            Assert.AreEqual(a0.GetHashCode(), a0.GetHashCode());
            Assert.AreEqual(a0.GetHashCode(), a1.GetHashCode());
            Assert.AreNotEqual(a0.GetHashCode(), b.GetHashCode());

            Assert.IsTrue(nullNode == null);
            Assert.IsFalse(null == a0);
            Assert.IsFalse(nullNode != null);
            Assert.IsTrue(null != a0);
        }

        [TestMethod]
        public void DataBytes()
        {
            var node = new MerkleNode(IpfsInfo);
            byte[] data = node.DataBytes;
            Assert.AreEqual(node.BlockSize, data.Length);
        }

        [TestMethod]
        public void DataStream()
        {
            var node = new MerkleNode(IpfsInfo);
            byte[] data = node.DataBytes;
            var streamData = new MemoryStream();
            node.DataStream.CopyTo(streamData);
            CollectionAssert.AreEqual(data, streamData.ToArray());
        }

    }
}
