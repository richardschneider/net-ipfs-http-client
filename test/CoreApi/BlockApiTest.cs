using Ipfs.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Ipfs.Api
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
            Assert.AreEqual("zb2rhYDhWhxyHN6HFAKGvHnLogYfnk9KvzBUZvCg7sYhS22N8", (string)cid);

            var data = ipfs.Block.GetAsync(cid).Result;
            Assert.AreEqual(blob.Length, data.Size);
            CollectionAssert.AreEqual(blob, data.DataBytes);
        }

        [TestMethod]
        public void Put_Bytes_Hash()
        {
            var cid = ipfs.Block.PutAsync(blob, "raw", "sha2-512").Result;
            Assert.AreEqual("zB7NCfbtX9WqFowgroqE19J841VESUhLc1enF7faMSMhTPMR4M3kWq7rS2AfCvdHeZ3RdfoSM45q7svoMQmw2NDD37z9F", (string)cid);

            var data = ipfs.Block.GetAsync(cid).Result;
            Assert.AreEqual(blob.Length, data.Size);
            CollectionAssert.AreEqual(blob, data.DataBytes);
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
            Assert.AreEqual("zb2rhYDhWhxyHN6HFAKGvHnLogYfnk9KvzBUZvCg7sYhS22N8", (string)cid);

            var data = ipfs.Block.GetAsync(cid).Result;
            Assert.AreEqual(blob.Length, data.Size);
            CollectionAssert.AreEqual(blob, data.DataBytes);
        }

        [TestMethod]
        public void Put_Stream_Hash()
        {
            var cid = ipfs.Block.PutAsync(new MemoryStream(blob), "raw", "sha2-512").Result;
            Assert.AreEqual("zB7NCfbtX9WqFowgroqE19J841VESUhLc1enF7faMSMhTPMR4M3kWq7rS2AfCvdHeZ3RdfoSM45q7svoMQmw2NDD37z9F", (string)cid);

            var data = ipfs.Block.GetAsync(cid).Result;
            Assert.AreEqual(blob.Length, data.Size);
            CollectionAssert.AreEqual(blob, data.DataBytes);
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
            Assert.AreEqual(null, cid);
        }

    }
}
