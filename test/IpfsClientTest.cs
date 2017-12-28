using Ipfs.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Http;
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
            ExceptionAssert.Throws<HttpRequestException>(() => unknown = target.DoCommandAsync("foobar", default(CancellationToken)).Result);
        }

        [TestMethod]
        public void Do_Command_Throws_Exception_On_Missing_Argument()
        {
            IpfsClient target = TestFixture.Ipfs;
            object unknown;
            ExceptionAssert.Throws<HttpRequestException>(() => unknown = target.DoCommandAsync("key/gen", default(CancellationToken)).Result);
        }
    }
}
