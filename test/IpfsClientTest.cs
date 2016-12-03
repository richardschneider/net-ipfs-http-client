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
    public partial class IpfsClientTest
    {
        /// <summary>
        ///   Fiddler cannot see localhost traffic because .Net bypasses the network stack for localhost/127.0.0.1. 
        ///   By using "127.0.0.1." (note trailing dot) fiddler will receive the traffic and if its not running
        ///   the localhost will get it!
        /// </summary>
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            //IpfsClient.DefaultApiUri = new Uri("http://127.0.0.1.:5001");
        }

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
