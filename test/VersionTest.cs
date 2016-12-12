using Ipfs.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ipfs.Api
{
    
    public partial class IpfsClientTest
    {

        [TestMethod]
        public void Version_Is_Present()
        {
            var ipfs = new IpfsClient();
            var version = ipfs.Version().Result;
            Console.WriteLine(version);
            Assert.IsNotNull(version);
        }
    }
}
