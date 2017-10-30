﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ipfs.Api
{
    [TestClass]
    public class PeerNodeTest
    {
        const string mars = "QmSoLMeWqB7YGVLJN3pNLQpmmEk35v6wYtsMGLzSr5QBU3";

        [TestMethod]
        public async Task Valid_Node()
        {
            var ipfs = TestFixture.Ipfs;
            var localNode = await ipfs.IdAsync();
            Assert.IsTrue(localNode.IsValid());
        }

        [TestMethod]
        public async Task Invalid_Node_Address()
        {
            var ipfs = TestFixture.Ipfs;
            var localNode = await ipfs.IdAsync();
            var addresses = localNode.Addresses.ToList();
            addresses.Add(new MultiAddress($"/ip6/::1/tcp/4001/ipfs/{mars}"));
            localNode.Addresses = addresses;
            Assert.IsFalse(localNode.IsValid());
        }

        [TestMethod]
        public async Task Invalid_Node_Id()
        {
            var ipfs = TestFixture.Ipfs;
            var localNode = await ipfs.IdAsync();
            localNode.Id = mars;
            Assert.IsFalse(localNode.IsValid());
        }

    }
}