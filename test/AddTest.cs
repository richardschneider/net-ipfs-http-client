using Ipfs.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ipfs.Api
{

    public partial class IpfsClientTest
    {

        [TestMethod]
        public void AddText()
        {
            var ipfs = new IpfsClient();
            var result = ipfs.AddTextAsync("hello world").Result; ;
            Assert.AreEqual("Qmf412jQZiuVUtdgnB36FXFX7xg5V6KEbSJ4dpQuhkLyfD", result.Hash);
        }

        [TestMethod]
        public void AddFile()
        {
            var ipfs = new IpfsClient();
            var result = ipfs.AddFileAsync("helloworld.txt").Result; ;
            Assert.AreEqual("Qmf412jQZiuVUtdgnB36FXFX7xg5V6KEbSJ4dpQuhkLyfD", result.Hash);
        }
    }
}
