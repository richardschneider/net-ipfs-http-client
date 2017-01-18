using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ipfs.Api
{
 
    [TestClass]
    public class DhtTest
    {
        const string marsId = "QmSoLMeWqB7YGVLJN3pNLQpmmEk35v6wYtsMGLzSr5QBU3";
        const string marsPublicKey = "CAASogEwgZ8wDQYJKoZIhvcNAQEBBQADgY0AMIGJAoGBAKGUtbRQf+a9SBHFEruNAUatS/tsGUnHuCtifGrlbYPELD3UyyhWf/FYczBCavx3i8hIPEW2jQv4ehxQxi/cg9SHswZCQblSi0ucwTBFr8d40JEiyB9CcapiMdFQxdMgGvXEOQdLz1pz+UPUDojkdKZq8qkkeiBn7KlAoGEocnmpAgMBAAE=";
        const string helloWorldID = "QmT78zSuBmuS4z925WZfrqQ1qHaJ56DQaTfyMUF7F8ff5o";

        [TestMethod]
        public async Task FindPeer()
        {
            var ipfs = TestFixture.Ipfs;
            var mars = await ipfs.Dht.FindPeerAsync(marsId);
            Assert.AreEqual(marsId, mars.Id);

            // Sometimes the public key is not returned!
            if (!string.IsNullOrEmpty(mars.PublicKey))
                Assert.AreEqual(marsPublicKey, mars.PublicKey);
        }

        [TestMethod]
        public async Task FindProviders()
        {
            var ipfs = TestFixture.Ipfs;
            var providers = await ipfs.Dht.FindProvidersAsync(helloWorldID);
            Assert.IsFalse(providers.Take(3).Contains(""));
        }

    }
}
