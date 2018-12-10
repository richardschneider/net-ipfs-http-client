using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ipfs.Http
{
 
    [TestClass]
    public class StatsApiTest
    {

        [TestMethod]
        public async Task SmokeTest()
        {
            var ipfs = TestFixture.Ipfs;
            var bandwidth = await ipfs.Stats.BandwidthAsync();
            var bitswap = await ipfs.Stats.BitswapAsync();
            var repository = await ipfs.Stats.RepositoryAsync();
        }

    }
}
