using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ipfs.Http
{
 
    [TestClass]
    public class DhtApiTest
    {
        const string helloWorldID = "QmT78zSuBmuS4z925WZfrqQ1qHaJ56DQaTfyMUF7F8ff5o";

        [TestMethod]
        public async Task FindPeer()
        {
            var ipfs = TestFixture.Ipfs;
            var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
            var mars = await ipfs.Dht.FindPeerAsync("QmSoLMeWqB7YGVLJN3pNLQpmmEk35v6wYtsMGLzSr5QBU3", cts.Token);
            Assert.IsNotNull(mars);
        }

        [TestMethod]
        public async Task FindProviders()
        {
            var ipfs = TestFixture.Ipfs;
            var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
            var providers = await ipfs.Dht.FindProvidersAsync(helloWorldID, 1, cancel: cts.Token);
            Assert.AreNotEqual(0, providers.Count());
        }

    }
}
