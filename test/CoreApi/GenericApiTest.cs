using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Ipfs.Api
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

    }
}

