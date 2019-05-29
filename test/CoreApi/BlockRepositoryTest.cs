using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ipfs.Http
{
 
    [TestClass]
    public class BlockRepositoryTest
    {

        [TestMethod]
        public async Task Stats()
        {
            var ipfs = TestFixture.Ipfs;
            var stats = await ipfs.BlockRepository.StatisticsAsync();
            Assert.IsNotNull(stats);
        }

        [TestMethod]
        public async Task Version()
        {
            var ipfs = TestFixture.Ipfs;
            var version = await ipfs.BlockRepository.VersionAsync();
            Assert.IsFalse(string.IsNullOrWhiteSpace(version));
        }

    }
}
