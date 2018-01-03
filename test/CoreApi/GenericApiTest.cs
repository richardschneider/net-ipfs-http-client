using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Text;

namespace Ipfs.Api
{
    [TestClass]
    public class GenericApiTest
    {
        const string marsId = "QmSoLMeWqB7YGVLJN3pNLQpmmEk35v6wYtsMGLzSr5QBU3";

        [TestMethod]
        public void Local_Node_Info()
        {
            var ipfs = TestFixture.Ipfs;
            var node = ipfs.IdAsync().Result;
            Assert.IsInstanceOfType(node, typeof(Peer));
        }

        [TestMethod]
        public void Mars_Node_Info()
        {
            var ipfs = TestFixture.Ipfs;
            var node = ipfs.IdAsync(marsId).Result;
            Assert.IsInstanceOfType(node, typeof(Peer));
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

    }
}

