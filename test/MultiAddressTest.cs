using Ipfs.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Ipfs.Api
{
    
    [TestClass]
    public partial class MultiAddressTest
    {
        const string somewhere = "/ip4/10.1.10.10/tcp/29087/ipfs/QmVcSqVEsvm5RR9mBLjwpb2XjFVn5bPdPL69mL8PH45pPC";
        const string nowhere = "/ip4/10.1.10.11/tcp/29087/ipfs/QmVcSqVEsvm5RR9mBLjwpb2XjFVn5bPdPL69mL8PH45pPC";

        [TestMethod]
        public void Parsing()
        {
            var a = new MultiAddress(somewhere);
            Assert.AreEqual(3, a.Parts.Count);
            Assert.AreEqual("ip4", a.Parts[0].Protocol);
            Assert.AreEqual("10.1.10.10", a.Parts[0].Value);
            Assert.AreEqual("tcp", a.Parts[1].Protocol);
            Assert.AreEqual("29087", a.Parts[1].Value);
            Assert.AreEqual("ipfs", a.Parts[2].Protocol);
            Assert.AreEqual("QmVcSqVEsvm5RR9mBLjwpb2XjFVn5bPdPL69mL8PH45pPC", a.Parts[2].Value);
        }

        [TestMethod]
        public void Protocols_In_Use()
        {
            var p = new MultiAddress(somewhere).Protocols.ToArray();
            Assert.AreEqual(3, p.Length);
            Assert.AreEqual("ip4", p[0]);
            Assert.AreEqual("tcp", p[1]);
            Assert.AreEqual("ipfs", p[2]);
        }

        [TestMethod]
        public new void ToString()
        {
            Assert.AreEqual(somewhere, new MultiAddress(somewhere).ToString());
        }

        [TestMethod]
        public void Value_Equality()
        {
            var a0 = new MultiAddress(somewhere);
            var a1 = new MultiAddress(somewhere);
            var b = new MultiAddress(nowhere);

            #pragma warning disable 1718
            Assert.IsTrue(a0 == a0);
            Assert.IsTrue(a0 == a1);
            Assert.IsFalse(a0 == b);

            #pragma warning disable 1718
            Assert.IsFalse(a0 != a0);
            Assert.IsFalse(a0 != a1);
            Assert.IsTrue(a0 != b);

            Assert.IsTrue(a0.Equals(a0));
            Assert.IsTrue(a0.Equals(a1));
            Assert.IsFalse(a0.Equals(b));

            Assert.AreEqual(a0, a0);
            Assert.AreEqual(a0, a1);
            Assert.AreNotEqual(a0, b);

            Assert.AreEqual<MultiAddress>(a0, a0);
            Assert.AreEqual<MultiAddress>(a0, a1);
            Assert.AreNotEqual<MultiAddress>(a0, b);

            Assert.AreEqual(a0.GetHashCode(), a0.GetHashCode());
            Assert.AreEqual(a0.GetHashCode(), a1.GetHashCode());
            Assert.AreNotEqual(a0.GetHashCode(), b.GetHashCode());
        }
    }
}
