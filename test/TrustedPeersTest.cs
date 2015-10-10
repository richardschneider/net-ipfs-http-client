using Ipfs.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Ipfs.Api
{
    
    public partial class IpfsClientTest
    {
        MultiAddress newTrustedPeer = new MultiAddress("/ip4/25.196.147.100/tcp/4001/ipfs/QmaMqSwWShsPg2RbredZtoneFjXhim7AQkqbLxib45Lx4S");

        [TestMethod]
        public void Trusted_Peers_List()
        {
            var ipfs = new IpfsClient();
            Assert.IsNotNull(ipfs.TrustedPeers);
            Assert.IsTrue(ipfs.TrustedPeers.Count > 0);
        }

        [TestMethod]
        public void Trusted_Peers_Add_Remove()
        {
            var ipfs = new IpfsClient();
            Assert.IsFalse(ipfs.TrustedPeers.Contains(newTrustedPeer));

            ipfs.TrustedPeers.Add(newTrustedPeer);
            Assert.IsTrue(ipfs.TrustedPeers.Contains(newTrustedPeer));

            ipfs.TrustedPeers.Remove(newTrustedPeer);
            Assert.IsFalse(ipfs.TrustedPeers.Contains(newTrustedPeer));
        }

        [TestMethod]
        public void Trusted_Peers_Add_Missing_Peer_ID()
        {
            var missingPeerId = new MultiAddress("/ip4/25.196.147.100/tcp/4001");
            var ipfs = new IpfsClient();
            ExceptionAssert.Throws<IpfsException>(() => ipfs.TrustedPeers.Add(missingPeerId), "invalid ipfs address");
        }

        [TestMethod]
        public void Trusted_Peers_Clear()
        {
            var ipfs = new IpfsClient();
            var original = ipfs.TrustedPeers.ToArray();
            ipfs.TrustedPeers.Clear();
            Assert.AreEqual(0, ipfs.TrustedPeers.Count);

            foreach (var a in original)
                ipfs.TrustedPeers.Add(a);
        }
    }
}
