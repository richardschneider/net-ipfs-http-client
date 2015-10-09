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
            Assert.IsNotNull(ipfs.TrustedPeers);
            Assert.IsTrue(ipfs.TrustedPeers.Count > 0);
        }

    }
}
