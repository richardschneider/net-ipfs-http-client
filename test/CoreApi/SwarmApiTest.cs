using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ipfs.Api
{
 
    [TestClass]
    public class SwarmApiTest
    {

        [TestMethod]
        public async Task Addresses()
        {
            var ipfs = TestFixture.Ipfs;
            var swarm = await ipfs.Swarm.AddressesAsync();
            foreach (var peer in swarm)
            {
                Assert.IsNotNull(peer.Id);
                Assert.IsNotNull(peer.Addresses);
                Assert.AreNotEqual(0, peer.Addresses.Count());
            }
        }

        [TestMethod]
        public async Task Peers()
        {
            var ipfs = TestFixture.Ipfs;
            var peers = await ipfs.Swarm.PeersAsync();
            Assert.AreNotEqual(0, peers.Count());
            foreach (var peer in peers)
            {
                Assert.IsNotNull(peer.Id);
                Assert.IsNotNull(peer.ConnectedAddress);
                Assert.AreEqual(peer.Id, peer.ConnectedAddress.Protocols.First(p => p.Name == "ipfs").Value);
            }
        }

        [TestMethod]
        public async Task Peers_Info()
        {
            var ipfs = TestFixture.Ipfs;
            var peers = await ipfs.Swarm.PeersAsync();
            await Task.WhenAll(peers
                .Where(p => p.Latency != TimeSpan.Zero)
                .OrderBy(p => p.Latency)
                .Take(1)
                .Select(async p =>
                {
                    var peer = await ipfs.IdAsync(p.Id.ToString()); // TODO: IdAsync should accept multihash
                    Assert.AreNotEqual("", peer.PublicKey);
                }));
        }

        [TestMethod]
        public async Task Connection()
        {
            var ipfs = TestFixture.Ipfs;
            var peers = await ipfs.Swarm.PeersAsync();

            // Sometimes we cannot connect to a specific peer.  This
            // tests that a connection can be made to at least one peer.
            foreach (var peer in peers)
            {
                try
                {
                    await ipfs.Swarm.DisconnectAsync(peer.ConnectedAddress);
                    await ipfs.Swarm.ConnectAsync(peer.ConnectedAddress);
                    return;
                }
                catch (Exception)
                {
                    // eat it
                }
            }

            Assert.Fail("Cannot connect to any peer");
        }
    }
}
