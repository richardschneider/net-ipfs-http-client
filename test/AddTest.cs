using Ipfs.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace Ipfs.Api
{

    public partial class IpfsClientTest
    {

        [TestMethod]
        public void AddText()
        {
            var ipfs = new IpfsClient();
            var result = ipfs.AddTextAsync("hello world").Result;
            Assert.AreEqual("Qmf412jQZiuVUtdgnB36FXFX7xg5V6KEbSJ4dpQuhkLyfD", result.Hash);
        }

        [TestMethod]
        public void AddFile()
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, "hello world");
            try
            {
                var ipfs = new IpfsClient();
                var result = ipfs.AddFileAsync(path).Result;
                Assert.AreEqual("Qmf412jQZiuVUtdgnB36FXFX7xg5V6KEbSJ4dpQuhkLyfD", result.Hash);
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}
