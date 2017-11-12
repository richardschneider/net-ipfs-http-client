using Ipfs.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

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
        ///   A test for IpfsClient Constructor
        ///</summary>
        [TestMethod]
        public void Can_Create()
        {
            IpfsClient target = TestFixture.Ipfs;
            Assert.IsNotNull(target);
        }

        [TestMethod]
        public void Do_Command_Throws_Exception_On_Invalid_Command()
        {
            IpfsClient target = TestFixture.Ipfs;
            object unknown;
            ExceptionAssert.Throws<Exception>(() => unknown = target.DoCommandAsync("foobar", default(CancellationToken)).Result);
        }

    }
}
