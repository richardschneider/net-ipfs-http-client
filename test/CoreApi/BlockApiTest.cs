using Ipfs.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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
            var block = ipfs.Block.PutAsync(blob).Result;
            Assert.AreEqual(id, (string)block.Id);
            CollectionAssert.AreEqual(blob, block.DataBytes);
        }

        [TestMethod]
        public void Put_Block()
        {
            var block1 = new Block { DataBytes = blob };
            var block2 = ipfs.Block.PutAsync(block1).Result;
            Assert.AreEqual(id, (string)block2.Id);
            CollectionAssert.AreEqual(blob, block2.DataBytes);
        }

        [TestMethod]
        public void Get()
        {
            var _ = ipfs.Block.PutAsync(blob).Result;
            var block = ipfs.Block.GetAsync(id).Result;
            Assert.AreEqual(id, (string)block.Id);
            CollectionAssert.AreEqual(blob, block.DataBytes);
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
            var removed = await ipfs.Block.RemoveAsync(id);
            Assert.AreEqual(id, removed);
        }

        [TestMethod]
        public void Remove_Unknown()
        {
            ExceptionAssert.Throws<Exception>(() => { var _ = ipfs.Block.RemoveAsync("QmPv52ekjS75L4JmHpXVeuJ5uX2ecSfSZo88NSyxwA3rFF").Result; });
        }

        [TestMethod]
        public async Task Remove_Unknown_OK()
        {
            var removed = await ipfs.Block.RemoveAsync("QmPv52ekjS75L4JmHpXVeuJ5uX2ecSfSZo88NSyxwA3rFF", true);
            Assert.AreEqual("", removed);
        }

    }
}
