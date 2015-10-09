using Ipfs.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Ipfs.Api
{
    
    public partial class IpfsClientTest
    {
        [TestMethod]
        public void Trusted_Peers_List()
        {
            var ipfs = new IpfsClient();
            var peers = ipfs.TrustedPeers().ToArray();
            Assert.IsNotNull(peers);
            Assert.IsTrue(peers.Length > 0);
        }

    }
}
