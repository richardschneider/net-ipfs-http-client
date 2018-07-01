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
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(() => ipfs.Bitswap.GetAsync(block.Id).Wait());

            var endTime = DateTime.Now.AddSeconds(10);
            while (DateTime.Now < endTime)
            {
                await Task.Delay(100);
                var wants = await ipfs.Bitswap.WantsAsync();
                if (wants.Contains(block.Id))
                    return;
            }
            Assert.Fail("wanted block is missing");
        }

        [TestMethod]
        [Ignore("doesn't work on go ipfs")]
        public async Task Unwant()
        {
            var block = new DagNode(Encoding.UTF8.GetBytes("BitswapApiTest unknown block 2"));
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(() => ipfs.Bitswap.GetAsync(block.Id).Wait());

            var endTime = DateTime.Now.AddSeconds(10);
            while (true)
            {
                if (DateTime.Now > endTime)
                    Assert.Fail("wanted block is missing");
                await Task.Delay(100);
                var w = await ipfs.Bitswap.WantsAsync();
                if (w.Contains(block.Id))
                    break;
            }

            await ipfs.Bitswap.UnwantAsync(block.Id);
            var wants = await ipfs.Bitswap.WantsAsync();
            CollectionAssert.DoesNotContain(wants.ToArray(), block.Id);
        }
    }
}
