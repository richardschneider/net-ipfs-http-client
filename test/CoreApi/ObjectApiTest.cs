using Ipfs.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ipfs.Http
{

    [TestClass]
    public class ObjectApiTest
    {
        IpfsClient ipfs = TestFixture.Ipfs;

        [TestMethod]
        public async Task New_Template_Null()
        {
            var node = await ipfs.Object.NewAsync();
            Assert.AreEqual("QmdfTbBqBPQ7VNxZEYEj14VmRuZBkqFbiwReogJgS1zR1n", (string)node.Id);
        }

        [TestMethod]
        public async Task New_Template_UnixfsDir()
        {
            var node = await ipfs.Object.NewAsync("unixfs-dir");
            Assert.AreEqual("QmUNLLsPACCz1vLxQVkXqqLX5R1X345qqfHbsf67hvA3Nn", (string)node.Id);

            node = await ipfs.Object.NewDirectoryAsync();
            Assert.AreEqual("QmUNLLsPACCz1vLxQVkXqqLX5R1X345qqfHbsf67hvA3Nn", (string)node.Id);

        }

        [TestMethod]
        public async Task Put_Get_Dag()
        {
            var adata = Encoding.UTF8.GetBytes("alpha");
            var bdata = Encoding.UTF8.GetBytes("beta");
            var alpha = new DagNode(adata);
            var beta = new DagNode(bdata, new[] { alpha.ToLink() });
            var x = await ipfs.Object.PutAsync(beta);
            var node = await ipfs.Object.GetAsync(x.Id);
            CollectionAssert.AreEqual(beta.DataBytes, node.DataBytes);
            Assert.AreEqual(beta.Links.Count(), node.Links.Count());
            Assert.AreEqual(beta.Links.First().Id, node.Links.First().Id);
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
            var node = await ipfs.Object.GetAsync(beta.Id);
            CollectionAssert.AreEqual(beta.DataBytes, node.DataBytes);
            Assert.AreEqual(beta.Links.Count(), node.Links.Count());
            Assert.AreEqual(beta.Links.First().Id, node.Links.First().Id);
            Assert.AreEqual(beta.Links.First().Name, node.Links.First().Name);
            Assert.AreEqual(beta.Links.First().Size, node.Links.First().Size);
        }

        [TestMethod]
        public async Task Data()
        {
            var adata = Encoding.UTF8.GetBytes("alpha");
            var node = await ipfs.Object.PutAsync(adata);
            using (var stream = await ipfs.Object.DataAsync(node.Id))
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
            var links = await ipfs.Object.LinksAsync(beta.Id);
            Assert.AreEqual(beta.Links.Count(),links.Count());
            Assert.AreEqual(beta.Links.First().Id, links.First().Id);
            Assert.AreEqual(beta.Links.First().Name, links.First().Name);
            Assert.AreEqual(beta.Links.First().Size, links.First().Size);
        }

#if falses
        [TestMethod]
        public async Task Stat()
        {
            var data1 = Encoding.UTF8.GetBytes("Some data 1");
            var data2 = Encoding.UTF8.GetBytes("Some data 2");
            var node2 = new DagNode(data2);
            var node1 = await ipfs.Object.PutAsync(data1,
                new[] { node2.ToLink("some-link") });
            var info = await ipfs.Object.StatAsync(node1.Id);
            Assert.AreEqual("QmPR7W4kaADkAo4GKEVVPQN81EDUFCHJtqejQZ5dEG7pBC", info.Hash);
            Assert.AreEqual(1, info.NumLinks);
            Assert.AreEqual(64, info.BlockSize);
            Assert.AreEqual(53, info.LinksSize);
            Assert.AreEqual(11, info.DataSize);
            Assert.AreEqual(77, info.CumulativeSize);
        }
#endif

        [TestMethod]
        public async Task Get_Nonexistent()
        {
            var data = Encoding.UTF8.GetBytes("Some data for net-ipfs-api-test that cannot be found");
            var node = new DagNode(data);
            var id = node.Id;
            var cs = new CancellationTokenSource(500);
            try
            {
                var _ = await ipfs.Object.GetAsync(id, cs.Token);
                Assert.Fail("Did not throw TaskCanceledException");
            }
            catch (TaskCanceledException)
            {
                return;
            }
        }

    }
}
