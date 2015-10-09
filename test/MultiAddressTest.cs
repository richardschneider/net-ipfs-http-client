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
    }
}
