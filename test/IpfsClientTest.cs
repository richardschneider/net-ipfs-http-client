using Ipfs.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ipfs.Api
{
    
    
    /// <summary>
    ///This is a test class for IpfsClientTest and is intended
    ///to contain all IpfsClientTest Unit Tests
    ///</summary>
    [TestClass]
    public class IpfsClientTest
    {

        /// <summary>
        ///   A test for IpfsClient Constructor
        ///</summary>
        [TestMethod]
        public void Can_Create()
        {
            IpfsClient target = new IpfsClient();
            Assert.IsNotNull(target);
        }
    }
}
