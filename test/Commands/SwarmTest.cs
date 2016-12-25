using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ipfs.Api
{
 
    [TestClass]
    public class SwarmTest
    {

        [TestMethod]
        public async Task Addresses()
        {
            var ipfs = new IpfsClient();
            var swarm = await ipfs.Swarm.Addresses();
            foreach (var peer in swarm)
            {
                Assert.IsNotNull(peer.Id);
                Assert.IsNotNull(peer.Addresses);
                Assert.AreNotEqual(0, peer.Addresses.Length);
            }
        }

        [TestMethod]
        public async Task Peers()
        {
            var ipfs = new IpfsClient();
            var peers = await ipfs.Swarm.Peers();
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
            var ipfs = new IpfsClient();
            var peers = await ipfs.Swarm.Peers();
            await Task.WhenAll(peers
                .Take(10)
                .Select(async p =>
                {
                    var peer = await ipfs.Id(p.Id);
                    Assert.AreNotEqual("", peer.PublicKey);
                }));
        }

        [TestMethod]
        public async Task Connection()
        {
            var ipfs = new IpfsClient();
            var peers = await ipfs.Swarm.Peers();
            var peer = peers.First();
            await ipfs.Swarm.Disconnect(peer.ConnectedAddress);
            await ipfs.Swarm.Connect(peer.ConnectedAddress);
        }
    }
}
