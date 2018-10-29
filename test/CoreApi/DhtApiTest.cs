using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ipfs.Api
{
 
    [TestClass]
    public class DhtApiTest
    {
        const string helloWorldID = "QmT78zSuBmuS4z925WZfrqQ1qHaJ56DQaTfyMUF7F8ff5o";

        [TestMethod]
        public async Task FindPeer()
        {
            var ipfs = TestFixture.Ipfs;
            Peer node;
            foreach (var peer in await ipfs.Bootstrap.ListAsync())
            {
                try
                {
                    node = await ipfs.Dht.FindPeerAsync(peer.PeerId);
                }
                catch (Exception)
                {
                    continue;
                }

                // If DHT can't find a peer, it will return a 'closer' peer.
                Assert.IsNotNull(peer);
                break;
            }
        }

        [TestMethod]
        public async Task FindProviders()
        {
            var ipfs = TestFixture.Ipfs;
            var providers = await ipfs.Dht.FindProvidersAsync(helloWorldID);
            Assert.AreNotEqual(0, providers.Count());
        }

    }
}
