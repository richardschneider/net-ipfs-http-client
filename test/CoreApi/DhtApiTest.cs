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
        const string marsId = "QmSoLMeWqB7YGVLJN3pNLQpmmEk35v6wYtsMGLzSr5QBU3";
        const string marsPublicKey = "CAASogEwgZ8wDQYJKoZIhvcNAQEBBQADgY0AMIGJAoGBAKGUtbRQf+a9SBHFEruNAUatS/tsGUnHuCtifGrlbYPELD3UyyhWf/FYczBCavx3i8hIPEW2jQv4ehxQxi/cg9SHswZCQblSi0ucwTBFr8d40JEiyB9CcapiMdFQxdMgGvXEOQdLz1pz+UPUDojkdKZq8qkkeiBn7KlAoGEocnmpAgMBAAE=";
        const string helloWorldID = "QmT78zSuBmuS4z925WZfrqQ1qHaJ56DQaTfyMUF7F8ff5o";

        [TestMethod]
        public async Task FindPeer()
        {
            var ipfs = TestFixture.Ipfs;
            var peer = await ipfs.Dht.FindPeerAsync(marsId);

            // If DHT can't find a peer, it will return a 'closer' peer.
            Assert.IsNotNull(peer);
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
