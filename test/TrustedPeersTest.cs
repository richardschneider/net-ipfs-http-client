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
            var ipfs = TestFixture.Ipfs;
            Assert.IsNotNull(ipfs.TrustedPeers);
            Assert.IsTrue(ipfs.TrustedPeers.Count > 0);
        }

        [TestMethod]
        public void Trusted_Peers_Add_Remove()
        {
            var ipfs = TestFixture.Ipfs;
            Assert.IsFalse(ipfs.TrustedPeers.Contains(newTrustedPeer));

            ipfs.TrustedPeers.Add(newTrustedPeer);
            Assert.IsTrue(ipfs.TrustedPeers.Contains(newTrustedPeer));

            ipfs.TrustedPeers.Remove(newTrustedPeer);
            Assert.IsFalse(ipfs.TrustedPeers.Contains(newTrustedPeer));
        }

        // js-ipfs does NOT check IPFS addresses.
        // And this bad addr eventually breaks the server.
        // https://github.com/ipfs/js-ipfs/issues/1066
#if false
        [TestMethod]
        public void Trusted_Peers_Add_Missing_Peer_ID()
        {
            var missingPeerId = new MultiAddress("/ip4/25.196.147.100/tcp/4001");
            var ipfs = TestFixture.Ipfs;
            ExceptionAssert.Throws<Exception>(() => ipfs.TrustedPeers.Add(missingPeerId), "invalid IPFS address");
        }
#endif

        [TestMethod]
        public void Trusted_Peers_Clear()
        {
            var ipfs = TestFixture.Ipfs;
            var original = ipfs.TrustedPeers.ToArray();

            ipfs.TrustedPeers.Clear();
            Assert.AreEqual(0, ipfs.TrustedPeers.Count);

            foreach (var a in original)
                ipfs.TrustedPeers.Add(a);
        }

        [TestMethod]
        public void Trusted_Peers_Add_Default_Nodes()
        {
            var ipfs = TestFixture.Ipfs;
            var original = ipfs.TrustedPeers.ToArray();

            ipfs.TrustedPeers.Clear();
            Assert.AreEqual(0, ipfs.TrustedPeers.Count);
            ipfs.TrustedPeers.AddDefaultNodes();
            Assert.AreNotEqual(0, ipfs.TrustedPeers.Count);

            ipfs.TrustedPeers.Clear();
            foreach (var a in original)
                ipfs.TrustedPeers.Add(a);
        }
    }
}
