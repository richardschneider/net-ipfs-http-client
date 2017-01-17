﻿using Ipfs.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Ipfs.Api
{

    public partial class IpfsClientTest
    {

        [TestMethod]
        public void Pin_List()
        {
            var ipfs = TestFixture.Ipfs;
            Assert.IsNotNull(ipfs.PinnedObjects);
            Assert.IsTrue(ipfs.PinnedObjects.Count > 0);
        }

        [TestMethod]
        public void Pin_Add_Remove()
        {
            var ipfs = TestFixture.Ipfs;
            var result = ipfs.AddTextAsync("I am pinned").Result;
            var id = result.Hash;

            ipfs.PinnedObjects.Add(id);
            Assert.IsTrue(ipfs.PinnedObjects.Contains(id));

            ipfs.PinnedObjects.Remove(id);
            Assert.IsFalse(ipfs.PinnedObjects.Contains(id));
        }


    }
}
