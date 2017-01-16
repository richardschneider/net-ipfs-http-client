using Ipfs.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ipfs.Api
{

    [TestClass]
    public class ObjectTest
    {
        IpfsClient ipfs = new IpfsClient();

        [TestMethod]
        public async Task New_Template_Null()
        {
            var node = await ipfs.Object.NewAsync();
            Assert.AreEqual("QmdfTbBqBPQ7VNxZEYEj14VmRuZBkqFbiwReogJgS1zR1n", node.Hash);
        }

        [TestMethod]
        public async Task New_Template_UnixfsDir()
        {
            var node = await ipfs.Object.NewAsync("unixfs-dir");
            Assert.AreEqual("QmUNLLsPACCz1vLxQVkXqqLX5R1X345qqfHbsf67hvA3Nn", node.Hash);

            node = await ipfs.Object.NewDirectoryAsync();
            Assert.AreEqual("QmUNLLsPACCz1vLxQVkXqqLX5R1X345qqfHbsf67hvA3Nn", node.Hash);

        }

        [TestMethod]
        public async Task Put_Get_Dag()
        {
            var adata = Encoding.UTF8.GetBytes("alpha");
            var bdata = Encoding.UTF8.GetBytes("beta");
            var alpha = new DagNode(adata);
            var beta = new DagNode(bdata, new[] { alpha.ToLink() });
            var x = await ipfs.Object.PutAsync(beta);
            var node = await ipfs.Object.GetAsync(x.Hash);
            CollectionAssert.AreEqual(beta.DataBytes, node.DataBytes);
            Assert.AreEqual(beta.Links.Count(), node.Links.Count());
            Assert.AreEqual(beta.Links.First().Hash, node.Links.First().Hash);
            Assert.AreEqual(beta.Links.First().Name, node.Links.First().Name);
            Assert.AreEqual(beta.Links.First().Size, node.Links.First().Size);
        }

        [TestMethod]
        public async Task Put_Get_Data()
        {
            var adata = Encoding.UTF8.GetBytes("alpha");
            var bdata = Encoding.UTF8.GetBytes("beta");
            var alpha = new DagNode(adata);
            var beta = await ipfs.Object.PutAsync(bdata, new[] { alpha.ToLink() });
            var node = await ipfs.Object.GetAsync(beta.Hash);
            CollectionAssert.AreEqual(beta.DataBytes, node.DataBytes);
            Assert.AreEqual(beta.Links.Count(), node.Links.Count());
            Assert.AreEqual(beta.Links.First().Hash, node.Links.First().Hash);
            Assert.AreEqual(beta.Links.First().Name, node.Links.First().Name);
            Assert.AreEqual(beta.Links.First().Size, node.Links.First().Size);
        }

        [TestMethod]
        public async Task Data()
        {
            var adata = Encoding.UTF8.GetBytes("alpha");
            var node = await ipfs.Object.PutAsync(adata);
            using (var stream = await ipfs.Object.DataAsync(node.Hash))
            {
                var bdata = new byte[adata.Length];
                stream.Read(bdata, 0, bdata.Length);
                CollectionAssert.AreEqual(adata, bdata);
            }
        }

        [TestMethod]
        public async Task Links()
        {
            var adata = Encoding.UTF8.GetBytes("alpha");
            var bdata = Encoding.UTF8.GetBytes("beta");
            var alpha = new DagNode(adata);
            var beta = await ipfs.Object.PutAsync(bdata, new[] { alpha.ToLink() });
            var links = await ipfs.Object.LinksAsync(beta.Hash);
            Assert.AreEqual(beta.Links.Count(),links.Count());
            Assert.AreEqual(beta.Links.First().Hash, links.First().Hash);
            Assert.AreEqual(beta.Links.First().Name, links.First().Name);
            Assert.AreEqual(beta.Links.First().Size, links.First().Size);
        }

        [TestMethod]
        public async Task Stat()
        {
            var data1 = Encoding.UTF8.GetBytes("Some data 1");
            var data2 = Encoding.UTF8.GetBytes("Some data 2");
            var node2 = new DagNode(data2);
            var node1 = await ipfs.Object.PutAsync(data1,
                new[] { node2.ToLink("some-link") });
            var info = await ipfs.Object.StatAsync(node1.Hash);
            Assert.AreEqual("QmPR7W4kaADkAo4GKEVVPQN81EDUFCHJtqejQZ5dEG7pBC", info.Hash);
            Assert.AreEqual(1, info.NumLinks);
            Assert.AreEqual(64, info.BlockSize);
            Assert.AreEqual(53, info.LinksSize);
            Assert.AreEqual(11, info.DataSize);
            Assert.AreEqual(77, info.CumulativeSize);
        }

    }
}
