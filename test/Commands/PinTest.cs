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
    public class PinTest
    {
        [TestMethod]
        public void List()
        {
            var ipfs = TestFixture.Ipfs;
            var pins = ipfs.Pin.ListAsync().Result;
            Assert.IsNotNull(pins);
            Assert.IsTrue(pins.Length > 0);
        }

        [TestMethod]
        public async Task List_Filtered()
        {
            var ipfs = TestFixture.Ipfs;
            var all = await ipfs.Pin.ListAsync();
            var some = await ipfs.Pin.ListAsync(PinMode.Direct);
            Assert.AreNotEqual(all.Length, some.Length);
        }

        [TestMethod]
        public async Task Add_Remove()
        {
            var ipfs = TestFixture.Ipfs;
            var result = await ipfs.AddTextAsync("I am pinned");
            var id = result.Hash;

            var pins = await ipfs.Pin.AddAsync(id);
            Assert.IsTrue(pins.Any(pin => pin.Id == id));
            var all = await ipfs.Pin.ListAsync();
            Assert.IsTrue(all.Any(pin => pin.Id == id));

            pins = await ipfs.Pin.RemoveAsync(id);
            Assert.IsTrue(pins.Any(pin => pin.Id == id));
            all = await ipfs.Pin.ListAsync();
            Assert.IsFalse(all.Any(pin => pin.Id == id));
        }

    }
}
