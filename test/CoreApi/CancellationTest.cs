using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ipfs.Api
{
    [TestClass]
    public class CancellationTest
    {
        [TestMethod]
        public async Task Cancel_Operation()
        {
            var ipfs = TestFixture.Ipfs;
            var cs = new CancellationTokenSource(500);
            try
            {
                await Task.Delay(1000);
                var result = await ipfs.IdAsync(cancel: cs.Token);
                Assert.Fail("Did not throw TaskCanceledException");
            }
            catch (TaskCanceledException)
            {
                return;
            }
        }
    }
}
