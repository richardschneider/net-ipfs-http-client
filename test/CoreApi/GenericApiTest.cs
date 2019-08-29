using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Ipfs.Http
{
    [TestClass]
    public class GenericApiTest
    {
        [TestMethod]
        public void Local_Node_Info()
        {
            var ipfs = TestFixture.Ipfs;
            var node = ipfs.IdAsync().Result;
            Assert.IsInstanceOfType(node, typeof(Peer));
        }

        [TestMethod]
        public async Task Peer_Node_Info()
        {
            var ipfs = TestFixture.Ipfs;
            var mars = await ipfs.IdAsync("QmSoLMeWqB7YGVLJN3pNLQpmmEk35v6wYtsMGLzSr5QBU3");
            Assert.IsNotNull(mars);
        }

        [TestMethod]
        public void Version_Info()
        {
            var ipfs = TestFixture.Ipfs;
            var versions = ipfs.VersionAsync().Result;
            Assert.IsNotNull(versions);
            Assert.IsTrue(versions.ContainsKey("Version"));
            Assert.IsTrue(versions.ContainsKey("Repo"));
        }

        [TestMethod]
        public void Resolve()
        {
            var ipfs = TestFixture.Ipfs;
            var path = ipfs.ResolveAsync("QmYNQJoKGNHTpPxCBPh9KkDpaExgd2duMa3aF6ytMpHdao").Result;
            Assert.AreEqual("/ipfs/QmYNQJoKGNHTpPxCBPh9KkDpaExgd2duMa3aF6ytMpHdao", path);
        }

        [TestMethod]
        public async Task Ping_Peer()
        {
            var ipfs = TestFixture.Ipfs;
            MultiHash peer = "QmSoLMeWqB7YGVLJN3pNLQpmmEk35v6wYtsMGLzSr5QBU3";
            var actual = await ipfs.Generic.PingAsync(peer, count: 1);
            Assert.IsNotNull(actual);
            Assert.AreNotEqual(0, actual.Count());
        }

        [TestMethod]
        public async Task Ping_Address()
        {
            var ipfs = TestFixture.Ipfs;
            MultiAddress addr = "/ip4/104.236.179.241/tcp/4001/ipfs/QmSoLPppuBtQSGwKDZT2M73ULpjvfd3aZ6ha4oFGL1KrGM";
            var actual = await ipfs.Generic.PingAsync(addr, count: 1);
            Assert.IsNotNull(actual);
            Assert.AreNotEqual(0, actual.Count());
        }
    }
}

