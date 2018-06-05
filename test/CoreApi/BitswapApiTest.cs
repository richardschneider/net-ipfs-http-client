using Ipfs.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ipfs.Api
{

    [TestClass]
    public class BitswapApiTest
    {
        IpfsClient ipfs = TestFixture.Ipfs;

        [TestMethod]
        public async Task Wants()
        {
            var block = new DagNode(Encoding.UTF8.GetBytes("BitswapApiTest unknown block"));
            Task.Run(() => ipfs.Block.GetAsync(block.Id).Wait());
            await Task.Delay(1300);
            var wants = await ipfs.Bitswap.WantsAsync();
            CollectionAssert.Contains(wants.ToArray(), block.Id);
        }

    }
}
