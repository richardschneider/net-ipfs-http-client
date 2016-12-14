using Ipfs.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ipfs.Api
{
    
    public partial class IpfsClientTest
    {
        const string marsId = "QmSoLMeWqB7YGVLJN3pNLQpmmEk35v6wYtsMGLzSr5QBU3";

        [TestMethod]
        public void Local_Node_Info()
        {
            var ipfs = new IpfsClient();
            var node = ipfs.Id().Result;
            Assert.IsInstanceOfType(node, typeof(PeerNode));
        }

        [TestMethod]
        public void Mars_Node_Info()
        {
            var ipfs = new IpfsClient();
            var node = ipfs.Id(marsId).Result;
            Assert.IsInstanceOfType(node, typeof(PeerNode));
        }
    }
}
