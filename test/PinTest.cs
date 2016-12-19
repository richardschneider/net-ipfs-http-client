using Ipfs.Api;
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
            var ipfs = new IpfsClient();
            Assert.IsNotNull(ipfs.PinnedObjects);
            Assert.IsTrue(ipfs.PinnedObjects.Count > 0);
        }

    }
}
