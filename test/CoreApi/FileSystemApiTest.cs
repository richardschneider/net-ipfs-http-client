using Ipfs.Http;
using Ipfs.CoreApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ipfs.Http
{
    [TestClass]
    public class FileSystemApiTest
    {

        [TestMethod]
        public void AddText()
        {
            var ipfs = TestFixture.Ipfs;
            var result = ipfs.FileSystem.AddTextAsync("hello world").Result;
            Assert.AreEqual("Qmf412jQZiuVUtdgnB36FXFX7xg5V6KEbSJ4dpQuhkLyfD", (string)result.Id);
        }

        [TestMethod]
        public void ReadText()
        {
            var ipfs = TestFixture.Ipfs;
            var node = ipfs.FileSystem.AddTextAsync("hello world").Result;
            var text = ipfs.FileSystem.ReadAllTextAsync(node.Id).Result;
            Assert.AreEqual("hello world", text);
        }

        [TestMethod]
        public void AddFile()
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, "hello world");
            try
            {
                var ipfs = TestFixture.Ipfs;
                var result = ipfs.FileSystem.AddFileAsync(path).Result;
                Assert.AreEqual("Qmf412jQZiuVUtdgnB36FXFX7xg5V6KEbSJ4dpQuhkLyfD", (string)result.Id);
                Assert.AreEqual(0, result.Links.Count());
            }
            finally
            {
                File.Delete(path);
            }
        }

        [TestMethod]
        public void Read_With_Offset()
        {
            var ipfs = TestFixture.Ipfs;
            var indata = new MemoryStream(new byte[] { 10, 20, 30 });
            var node = ipfs.FileSystem.AddAsync(indata).Result;
            using (var outdata = ipfs.FileSystem.ReadFileAsync(node.Id, offset: 1).Result)
            {
                Assert.AreEqual(20, outdata.ReadByte());
                Assert.AreEqual(30, outdata.ReadByte());
                Assert.AreEqual(-1, outdata.ReadByte());
            }
        }

        [TestMethod]
        public void Read_With_Offset_Length_1()
        {
            var ipfs = TestFixture.Ipfs;
            var indata = new MemoryStream(new byte[] { 10, 20, 30 });
            var node = ipfs.FileSystem.AddAsync(indata).Result;
            using (var outdata = ipfs.FileSystem.ReadFileAsync(node.Id, offset: 1, count: 1).Result)
            {
                Assert.AreEqual(20, outdata.ReadByte());
                Assert.AreEqual(-1, outdata.ReadByte());
            }
        }

        [TestMethod]
        public void Read_With_Offset_Length_2()
        {
            var ipfs = TestFixture.Ipfs;
            var indata = new MemoryStream(new byte[] { 10, 20, 30 });
            var node = ipfs.FileSystem.AddAsync(indata).Result;
            using (var outdata = ipfs.FileSystem.ReadFileAsync(node.Id, offset: 1, count: 2).Result)
            {
                Assert.AreEqual(20, outdata.ReadByte());
                Assert.AreEqual(30, outdata.ReadByte());
                Assert.AreEqual(-1, outdata.ReadByte());
            }
        }

        [TestMethod]
        public void Add_NoPin()
        {
            var ipfs = TestFixture.Ipfs;
            var data = new MemoryStream(new byte[] { 11, 22, 33 });
            var options = new AddFileOptions { Pin = false };
            var node = ipfs.FileSystem.AddAsync(data, "", options).Result;
            var pins = ipfs.Pin.ListAsync().Result;
            Assert.IsFalse(pins.Any(pin => pin == node.Id));
        }

        [TestMethod]
        public async Task Add_Wrap()
        {
            var path = "hello.txt";
            File.WriteAllText(path, "hello world");
            try
            {
                var ipfs = TestFixture.Ipfs;
                var options = new AddFileOptions
                {
                    Wrap = true
                };
                var node = await ipfs.FileSystem.AddFileAsync(path, options);
                Assert.AreEqual("QmNxvA5bwvPGgMXbmtyhxA1cKFdvQXnsGnZLCGor3AzYxJ", (string)node.Id);
                Assert.AreEqual(true, node.IsDirectory);
                Assert.AreEqual(1, node.Links.Count());
                Assert.AreEqual("hello.txt", node.Links.First().Name);
                Assert.AreEqual("Qmf412jQZiuVUtdgnB36FXFX7xg5V6KEbSJ4dpQuhkLyfD", (string)node.Links.First().Id);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [TestMethod]
        public async Task Add_SizeChunking()
        {
            var ipfs = TestFixture.Ipfs;
            var options = new AddFileOptions
            {
                ChunkSize = 3
            };
            options.Pin = true;
            var node = await ipfs.FileSystem.AddTextAsync("hello world", options);
            Assert.AreEqual("QmVVZXWrYzATQdsKWM4knbuH5dgHFmrRqW3nJfDgdWrBjn", (string)node.Id);
            Assert.AreEqual(false, node.IsDirectory);

            var links = (await ipfs.Object.LinksAsync(node.Id)).ToArray();
            Assert.AreEqual(4, links.Length);
            Assert.AreEqual("QmevnC4UDUWzJYAQtUSQw4ekUdqDqwcKothjcobE7byeb6", (string)links[0].Id);
            Assert.AreEqual("QmTdBogNFkzUTSnEBQkWzJfQoiWbckLrTFVDHFRKFf6dcN", (string)links[1].Id);
            Assert.AreEqual("QmPdmF1n4di6UwsLgW96qtTXUsPkCLN4LycjEUdH9977d6", (string)links[2].Id);
            Assert.AreEqual("QmXh5UucsqF8XXM8UYQK9fHXsthSEfi78kewr8ttpPaLRE", (string)links[3].Id);

            var text = await ipfs.FileSystem.ReadAllTextAsync(node.Id);
            Assert.AreEqual("hello world", text);
        }

        [TestMethod]
        public async Task Add_Raw()
        {
            var ipfs = TestFixture.Ipfs;
            var options = new AddFileOptions
            {
                RawLeaves = true
            };
            var node = await ipfs.FileSystem.AddTextAsync("hello world", options);
            Assert.AreEqual("zb2rhj7crUKTQYRGCRATFaQ6YFLTde2YzdqbbhAASkL9uRDXn", (string)node.Id);
            Assert.AreEqual(11, node.Size);

            var text = await ipfs.FileSystem.ReadAllTextAsync(node.Id);
            Assert.AreEqual("hello world", text);
        }

        [TestMethod]
        public async Task Add_RawAndChunked()
        {
            var ipfs = TestFixture.Ipfs;
            var options = new AddFileOptions
            {
                RawLeaves = true,
                ChunkSize = 3
            };
            var node = await ipfs.FileSystem.AddTextAsync("hello world", options);
            Assert.AreEqual("QmUuooB6zEhMmMaBvMhsMaUzar5gs5KwtVSFqG4C1Qhyhs", (string)node.Id);
            Assert.AreEqual(false, node.IsDirectory);

            var links = (await ipfs.Object.LinksAsync(node.Id)).ToArray();
            Assert.AreEqual(4, links.Length);
            Assert.AreEqual("zb2rhm6D8PTYoMh7PSFKbCxxcD1yjWPD5KPr6nVRuw9ymDyUL", (string)links[0].Id);
            Assert.AreEqual("zb2rhgo7y6J7p76kCrXs4pmmMQx56fZeWJkC3sfbjeay4UruU", (string)links[1].Id);
            Assert.AreEqual("zb2rha4Pd2AruByr2RwzhRCVxRCqBC67h7ukTJd99jCjUtmyM", (string)links[2].Id);
            Assert.AreEqual("zb2rhn6eZLLj7vdVizbNxpASGoVw4vcSmc8avHXmDMVu5ZA6Q", (string)links[3].Id);

            var text = await ipfs.FileSystem.ReadAllTextAsync(node.Id);
            Assert.AreEqual("hello world", text);
        }

        [TestMethod]
        public void AddDirectory()
        {
            var ipfs = TestFixture.Ipfs;
            var temp = MakeTemp();
            try
            {
                var dir = ipfs.FileSystem.AddDirectoryAsync(temp, false).Result;
                Assert.IsTrue(dir.IsDirectory);

                var files = dir.Links.ToArray();
                Assert.AreEqual(2, files.Length);
                Assert.AreEqual("alpha.txt", files[0].Name);
                Assert.AreEqual("beta.txt", files[1].Name);
                Assert.IsFalse(files[0].IsDirectory);
                Assert.IsFalse(files[1].IsDirectory);

                Assert.AreEqual("alpha", ipfs.FileSystem.ReadAllTextAsync(files[0].Id).Result);
                Assert.AreEqual("beta", ipfs.FileSystem.ReadAllTextAsync(files[1].Id).Result);

                Assert.AreEqual("alpha", ipfs.FileSystem.ReadAllTextAsync(dir.Id + "/alpha.txt").Result);
                Assert.AreEqual("beta", ipfs.FileSystem.ReadAllTextAsync(dir.Id + "/beta.txt").Result);
            }
            finally
            {
                DeleteTemp(temp);
            }
        }

        [TestMethod]
        public void AddDirectoryRecursive()
        {
            var ipfs = TestFixture.Ipfs;
            var temp = MakeTemp();
            try
            {
                var dir = ipfs.FileSystem.AddDirectoryAsync(temp, true).Result;
                Assert.IsTrue(dir.IsDirectory);

                var files = dir.Links.ToArray();
                Assert.AreEqual(3, files.Length);
                Assert.AreEqual("alpha.txt", files[0].Name);
                Assert.AreEqual("beta.txt", files[1].Name);
                Assert.AreEqual("x", files[2].Name);
                Assert.IsFalse(files[0].IsDirectory);
                Assert.IsFalse(files[1].IsDirectory);
                Assert.IsTrue(files[2].IsDirectory);
                Assert.AreNotEqual(0, files[0].Size);
                Assert.AreNotEqual(0, files[1].Size);

                var xfiles = new FileSystemNode
                {
                    Id = files[2].Id,
                    IpfsClient = ipfs,
                }.Links.ToArray();
                Assert.AreEqual(2, xfiles.Length);
                Assert.AreEqual("x.txt", xfiles[0].Name);
                Assert.AreEqual("y", xfiles[1].Name);
                Assert.IsFalse(xfiles[0].IsDirectory);
                Assert.IsTrue(xfiles[1].IsDirectory);

                var yfiles = new FileSystemNode
                {
                    Id = xfiles[1].Id,
                    IpfsClient = ipfs
                }.Links.ToArray();
                Assert.AreEqual(1, yfiles.Length);
                Assert.AreEqual("y.txt", yfiles[0].Name);
                Assert.IsFalse(yfiles[0].IsDirectory);

                var y = new FileSystemNode
                {
                    Id = yfiles[0].Id,
                    IpfsClient = ipfs
                };
                Assert.AreEqual("y", Encoding.UTF8.GetString(y.DataBytes));
                Assert.AreEqual("y", ipfs.FileSystem.ReadAllTextAsync(dir.Id + "/x/y/y.txt").Result);
            }
            finally
            {
                DeleteTemp(temp);
            }
        }

        [TestMethod]
        public async Task GetTar_EmptyDirectory()
        {
            var ipfs = TestFixture.Ipfs;
            var temp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(temp);
            try
            {
                var dir = ipfs.FileSystem.AddDirectoryAsync(temp, true).Result;
                var dirid = dir.Id.Encode();

                using (var tar = await ipfs.FileSystem.GetAsync(dir.Id))
                {
                    var buffer = new byte[3 * 512];
                    var offset = 0;
                    while (offset < buffer.Length)
                    {
                        var n = await tar.ReadAsync(buffer, offset, buffer.Length - offset);
                        Assert.IsTrue(n > 0);
                        offset += n;
                    }
                    Assert.AreEqual(-1, tar.ReadByte());
                }
            }
            finally
            {
                DeleteTemp(temp);
            }
        }


        [TestMethod]
        public async Task AddFile_WithProgress()
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, "hello world");
            try
            {
                var ipfs = TestFixture.Ipfs;
                var bytesTransferred = 0UL;
                var options = new AddFileOptions
                {
                    Progress = new Progress<TransferProgress>(t =>
                    {
                        Console.WriteLine("got it");
                        bytesTransferred += t.Bytes;
                    })
                };
                var result = await ipfs.FileSystem.AddFileAsync(path, options);
                Assert.AreEqual("Qmf412jQZiuVUtdgnB36FXFX7xg5V6KEbSJ4dpQuhkLyfD", (string)result.Id);

                // Progress reports get posted on another synchronisation context.
                var stop = DateTime.Now.AddSeconds(3);
                while (DateTime.Now < stop)
                {
                    if (bytesTransferred == 11UL)
                        break;
                    await Task.Delay(10);
                }
                Assert.AreEqual(11UL, bytesTransferred);
            }
            finally
            {
                File.Delete(path);
            }
        }

        void DeleteTemp(string temp)
        {
            while (true)
            {
                try
                {
                    Directory.Delete(temp, true);
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(1);
                    continue;  // most likely anti-virus is reading a file
                }
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
