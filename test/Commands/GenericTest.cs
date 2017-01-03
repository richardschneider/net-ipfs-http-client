using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Text;

namespace Ipfs.Api
{
    [TestClass]
    public class GenericTest
    {
        const string marsId = "QmSoLMeWqB7YGVLJN3pNLQpmmEk35v6wYtsMGLzSr5QBU3";

        [TestMethod]
        public void Local_Node_Info()
        {
            var ipfs = new IpfsClient();
            var node = ipfs.IdAsync().Result;
            Assert.IsInstanceOfType(node, typeof(PeerNode));
        }

        [TestMethod]
        public void Mars_Node_Info()
        {
            var ipfs = new IpfsClient();
            var node = ipfs.IdAsync(marsId).Result;
            Assert.IsInstanceOfType(node, typeof(PeerNode));
        }

        [TestMethod]
        public void Version_Info()
        {
            var ipfs = new IpfsClient();
            var versions = ipfs.VersionAsync().Result;
            Assert.IsNotNull(versions);
            Assert.IsTrue(versions.ContainsKey("Version"));
            Assert.IsTrue(versions.ContainsKey("Repo"));
        }

    }
}

