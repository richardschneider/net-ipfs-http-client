using Ipfs.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ipfs.Api
{

    [TestClass]
    public class PubSubApiTest
    {

        [TestMethod]
        public void Api_Exists()
        {
            IpfsClient ipfs = TestFixture.Ipfs;
            Assert.IsNotNull(ipfs.PubSub);
        }

        [TestMethod]
        public void Peers()
        {
            var ipfs = TestFixture.Ipfs;
            var peers = ipfs.PubSub.PeersAsync().Result.ToArray();
            Assert.IsTrue(peers.Length > 0);
        }

        [TestMethod]
        public void Subscribed_Topics()
        {
            var ipfs = TestFixture.Ipfs;
            var topics = ipfs.PubSub.SubscribedTopicsAsync().Result.ToArray();
            // TODO: Assert.IsTrue(peers.Length > 0);
        }

        [TestMethod]
        public void Publish()
        {
            var ipfs = TestFixture.Ipfs;
            ipfs.PubSub.Publish("foo", "hello world test").Wait();
            // TODO: test if published
        }

        [TestMethod]
        public async Task Subscribe()
        {
            int messageCount = 0;
            var ipfs = TestFixture.Ipfs;
            var topic = "net-ipfs-api-test";
            await ipfs.PubSub.Subscribe(topic, msg =>
            {
                Console.WriteLine("got msg");
                ++messageCount;
            });
            await ipfs.PubSub.Publish(topic, "hello world");
            await ipfs.PubSub.Publish(topic, "hello world");
            await Task.Delay(5000);
            Assert.AreEqual(2, messageCount);
        }

    }
}
