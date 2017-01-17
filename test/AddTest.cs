using Ipfs.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text;

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
                Assert.AreEqual(0, result.Links.Count());
            }
            finally
            {
                File.Delete(path);
            }
        }

        [TestMethod]
        public void AddDirectory()
        {
            var ipfs = new IpfsClient();
            var temp = MakeTemp();
            try
            {
                var dir = ipfs.AddDirectoryAsync(temp, false).Result;
                Assert.IsTrue(dir.IsDirectory);

                var files = dir.Links.ToArray();
                Assert.AreEqual(2, files.Length);
                Assert.AreEqual("alpha.txt", files[0].Name);
                Assert.AreEqual("beta.txt", files[1].Name);
                Assert.IsFalse(files[0].IsDirectory);
                Assert.IsFalse(files[1].IsDirectory);

                Assert.AreEqual("alpha", ipfs.ReadAllTextAsync(files[0].Hash).Result);
                Assert.AreEqual("beta", ipfs.ReadAllTextAsync(files[1].Hash).Result);

                Assert.AreEqual("alpha", ipfs.ReadAllTextAsync(dir.Hash + "/alpha.txt").Result);
                Assert.AreEqual("beta", ipfs.ReadAllTextAsync(dir.Hash + "/beta.txt").Result);
            }
            finally
            {
                Directory.Delete(temp, true);
            }
        }

        [TestMethod]
        public void AddDirectoryRecursive()
        {
            var ipfs = new IpfsClient();
            var temp = MakeTemp();
            try
            {
                var dir = ipfs.AddDirectoryAsync(temp, true).Result;
                Assert.IsTrue(dir.IsDirectory);
                Assert.AreEqual(0, dir.Size);

                var files = dir.Links.ToArray();
                Assert.AreEqual(3, files.Length);
                Assert.AreEqual("alpha.txt", files[0].Name);
                Assert.AreEqual("beta.txt", files[1].Name);
                Assert.AreEqual("x", files[2].Name);
                Assert.IsFalse(files[0].IsDirectory);
                Assert.IsFalse(files[1].IsDirectory);
                Assert.IsTrue(files[2].IsDirectory);
                Assert.AreEqual(5, files[0].Size);
                Assert.AreEqual(4, files[1].Size);
                Assert.AreEqual(0, files[2].Size);

                var xfiles = new FileSystemNode { Hash = files[2].Hash }.Links.ToArray();
                Assert.AreEqual(2, xfiles.Length);
                Assert.AreEqual("x.txt", xfiles[0].Name);
                Assert.AreEqual("y", xfiles[1].Name);
                Assert.IsFalse(xfiles[0].IsDirectory);
                Assert.IsTrue(xfiles[1].IsDirectory);

                var yfiles = new FileSystemNode { Hash = xfiles[1].Hash }.Links.ToArray();
                Assert.AreEqual(1, yfiles.Length);
                Assert.AreEqual("y.txt", yfiles[0].Name);
                Assert.IsFalse(yfiles[0].IsDirectory);

                var y = new FileSystemNode { Hash = yfiles[0].Hash };
                Assert.AreEqual("y", Encoding.UTF8.GetString(y.DataBytes));
                Assert.AreEqual("y", ipfs.ReadAllTextAsync(dir.Hash + "/x/y/y.txt").Result);
            }
            finally
            {
                Directory.Delete(temp, true);
            }
        }

        string MakeTemp()
        {
            var temp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var x = Path.Combine(temp, "x");
            var xy = Path.Combine(x, "y");
            Directory.CreateDirectory(temp);
            Directory.CreateDirectory(x);
            Directory.CreateDirectory(xy);

            File.WriteAllText(Path.Combine(temp, "alpha.txt"), "alpha");
            File.WriteAllText(Path.Combine(temp, "beta.txt"), "beta");
            File.WriteAllText(Path.Combine(x, "x.txt"), "x");
            File.WriteAllText(Path.Combine(xy, "y.txt"), "y");
            return temp;
        }
    }
}
