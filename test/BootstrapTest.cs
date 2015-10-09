using Ipfs.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Ipfs.Api
{
    
    public partial class IpfsClientTest
    {
        [TestMethod]
        public void Bootstrap_Peers()
        {
            var ipfs = new IpfsClient();
            var peers = ipfs.BootstrapPeers().ToArray();
            Assert.IsNotNull(peers);
            Assert.IsTrue(peers.Length > 0);
        }

    }
}
