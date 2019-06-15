using Ipfs.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ipfs.Http
{

    [TestClass]
    public class BlockApiTest
    {
        IpfsClient ipfs = TestFixture.Ipfs;
        string id = "QmPv52ekjS75L4JmHpXVeuJ5uX2ecSfSZo88NSyxwA3rAQ";
        byte[] blob = Encoding.UTF8.GetBytes("blorb");

        [TestMethod]
        public void Put_Bytes()
        {
            var cid = ipfs.Block.PutAsync(blob).Result;
            Assert.AreEqual(id, (string)cid);

            var data = ipfs.Block.GetAsync(cid).Result;
            Assert.AreEqual(blob.Length, data.Size);
            CollectionAssert.AreEqual(blob, data.DataBytes);
        }

        [TestMethod]
        public void Put_Bytes_ContentType()
        {
            var cid = ipfs.Block.PutAsync(blob, contentType: "raw").Result;
            Assert.AreEqual("bafkreiaxnnnb7qz2focittuqq3ya25q7rcv3bqynnczfzako47346wosmu", (string)cid);

            var data = ipfs.Block.GetAsync(cid).Result;
            Assert.AreEqual(blob.Length, data.Size);
            CollectionAssert.AreEqual(blob, data.DataBytes);
        }

        [TestMethod]
        public void Put_Bytes_Hash()
        {
            var cid = ipfs.Block.PutAsync(blob, "raw", "sha2-512").Result;
            Assert.AreEqual("bafkrgqelljziv4qfg5mefz36m2y3h6voaralnw6lwb4f53xcnrf4mlsykkn7vt6eno547tw5ygcz62kxrle45wnbmpbofo5tvu57jvuaf7k7e", (string)cid);

            var data = ipfs.Block.GetAsync(cid).Result;
            Assert.AreEqual(blob.Length, data.Size);
            CollectionAssert.AreEqual(blob, data.DataBytes);
        }

        [TestMethod]
        public void Put_Bytes_Pinned()
        {
            var data1 = new byte[] { 23, 24, 127 };
            var cid1 = ipfs.Block.PutAsync(data1, contentType: "raw", pin: true).Result;
            var pins = ipfs.Pin.ListAsync().Result;
            Assert.IsTrue(pins.Any(pin => pin == cid1));

            var data2 = new byte[] { 123, 124, 27 };
            var cid2 = ipfs.Block.PutAsync(data2, contentType: "raw", pin: false).Result;
            pins = ipfs.Pin.ListAsync().Result;
            Assert.IsFalse(pins.Any(pin => pin == cid2));
        }

        [TestMethod]
        public void Put_Stream()
        {
            var cid = ipfs.Block.PutAsync(new MemoryStream(blob)).Result;
            Assert.AreEqual(id, (string)cid);

            var data = ipfs.Block.GetAsync(cid).Result;
            Assert.AreEqual(blob.Length, data.Size);
            CollectionAssert.AreEqual(blob, data.DataBytes);
        }

        [TestMethod]
        public void Put_Stream_ContentType()
        {
            var cid = ipfs.Block.PutAsync(new MemoryStream(blob), contentType: "raw").Result;
            Assert.AreEqual("bafkreiaxnnnb7qz2focittuqq3ya25q7rcv3bqynnczfzako47346wosmu", (string)cid);

            var data = ipfs.Block.GetAsync(cid).Result;
            Assert.AreEqual(blob.Length, data.Size);
            CollectionAssert.AreEqual(blob, data.DataBytes);
        }

        [TestMethod]
        public void Put_Stream_Hash()
        {
            var cid = ipfs.Block.PutAsync(new MemoryStream(blob), "raw", "sha2-512").Result;
            Assert.AreEqual("bafkrgqelljziv4qfg5mefz36m2y3h6voaralnw6lwb4f53xcnrf4mlsykkn7vt6eno547tw5ygcz62kxrle45wnbmpbofo5tvu57jvuaf7k7e", (string)cid);

            var data = ipfs.Block.GetAsync(cid).Result;
            Assert.AreEqual(blob.Length, data.Size);
            CollectionAssert.AreEqual(blob, data.DataBytes);
        }

        [TestMethod]
        public void Put_Stream_Pinned()
        {
            var data1 = new MemoryStream(new byte[] { 23, 24, 127 });
            var cid1 = ipfs.Block.PutAsync(data1, contentType: "raw", pin: true).Result;
            var pins = ipfs.Pin.ListAsync().Result;
            Assert.IsTrue(pins.Any(pin => pin == cid1));

            var data2 = new MemoryStream(new byte[] { 123, 124, 27 });
            var cid2 = ipfs.Block.PutAsync(data2, contentType: "raw", pin: false).Result;
            pins = ipfs.Pin.ListAsync().Result;
            Assert.IsFalse(pins.Any(pin => pin == cid2));
        }

        [TestMethod]
        public void Get()
        {
            var _ = ipfs.Block.PutAsync(blob).Result;
            var block = ipfs.Block.GetAsync(id).Result;
            Assert.AreEqual(id, (string)block.Id);
            CollectionAssert.AreEqual(blob, block.DataBytes);
            var blob1 = new byte[blob.Length];
            block.DataStream.Read(blob1, 0, blob1.Length);
            CollectionAssert.AreEqual(blob, blob1);
        }

        [TestMethod]
        public void Stat()
        {
            var _ = ipfs.Block.PutAsync(blob).Result;
            var info = ipfs.Block.StatAsync(id).Result;
            Assert.AreEqual(id, (string)info.Id);
            Assert.AreEqual(5, info.Size);
        }

        [TestMethod]
        public async Task Remove()
        {
            var _ = ipfs.Block.PutAsync(blob).Result;
            var cid = await ipfs.Block.RemoveAsync(id);
            Assert.AreEqual(id, (string)cid);
        }

        [TestMethod]
        public void Remove_Unknown()
        {
            ExceptionAssert.Throws<Exception>(() => { var _ = ipfs.Block.RemoveAsync("QmPv52ekjS75L4JmHpXVeuJ5uX2ecSfSZo88NSyxwA3rFF").Result; });
        }

        [TestMethod]
        public async Task Remove_Unknown_OK()
        {
            var cid = await ipfs.Block.RemoveAsync("QmPv52ekjS75L4JmHpXVeuJ5uX2ecSfSZo88NSyxwA3rFF", true);
        }

    }
}
