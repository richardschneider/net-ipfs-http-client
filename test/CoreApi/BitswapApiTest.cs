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
        [Ignore("https://github.com/ipfs/go-ipfs/issues/5295")]
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
                var wants = await ipfs.Bitswap.WantsAsync();
                if (wants.Contains(block.Id))
                    break;
            }

            await ipfs.Bitswap.UnwantAsync(block.Id);
            endTime = DateTime.Now.AddSeconds(10);
            while (true)
            {
                if (DateTime.Now > endTime)
                    Assert.Fail("unwanted block is present");
                await Task.Delay(100);
                var wants = await ipfs.Bitswap.WantsAsync();
                if (!wants.Contains(block.Id))
                    break;
            }
        }

        [TestMethod]
        public async Task Ledger()
        {
            var peer = new Peer { Id = "QmSoLMeWqB7YGVLJN3pNLQpmmEk35v6wYtsMGLzSr5QBU3" };
            var ledger = await ipfs.Bitswap.LedgerAsync(peer);
            Assert.IsNotNull(ledger);
            Assert.AreEqual(peer.Id, ledger.Peer.Id);
        }
    }
}
