using Ipfs.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ipfs.Api
{

    [TestClass]
    public class KeyApiTest
    {

        [TestMethod]
        public void Api_Exists()
        {
            IpfsClient ipfs = TestFixture.Ipfs;
            Assert.IsNotNull(ipfs.Key);
        }

        [TestMethod]
        public async Task Self_Key_Exists()
        {
            IpfsClient ipfs = TestFixture.Ipfs;
            var keys = await ipfs.Key.ListAsync();
            var self = keys.Single(k => k.Name == "self");
            var me = await ipfs.IdAsync();
            Assert.AreEqual("self", self.Name);
            Assert.AreEqual(me.Id, self.Id);
        }

        [TestMethod]
        public async Task Create_RSA_Key()
        {
            var name = "net-api-test-create";
            IpfsClient ipfs = TestFixture.Ipfs;
            var key = await ipfs.Key.CreateAsync(name, "rsa", 1024);
            try
            {
                Assert.IsNotNull(key);
                Assert.IsNotNull(key.Id);
                Assert.AreEqual(name, key.Name);

                var keys = await ipfs.Key.ListAsync();
                var clone = keys.Single(k => k.Name == name);
                Assert.AreEqual(key.Name, clone.Name);
                Assert.AreEqual(key.Id, clone.Id);
            }
            finally
            {
                await ipfs.Key.RemoveAsync(name);
            }
        }

        [TestMethod]
        public async Task Remove_Key()
        {
            var name = "net-api-test-remove";
            IpfsClient ipfs = TestFixture.Ipfs;
            var key = await ipfs.Key.CreateAsync(name, "rsa", 1024);
            var keys = await ipfs.Key.ListAsync();
            var clone = keys.Single(k => k.Name == name);
            Assert.IsNotNull(clone);

            var removed = await ipfs.Key.RemoveAsync(name);
            Assert.IsNotNull(removed);
            Assert.AreEqual(key.Name, removed.Name);
            Assert.AreEqual(key.Id, removed.Id);

            keys = await ipfs.Key.ListAsync();
            Assert.IsFalse(keys.Any(k => k.Name == name));
        }

        [TestMethod]
        public async Task Rename_Key()
        {
            var oname = "net-api-test-rename1";
            var rname = "net-api-test-rename2";
            IpfsClient ipfs = TestFixture.Ipfs;
            var okey = await ipfs.Key.CreateAsync(oname, "rsa", 1024);
            try
            {
                Assert.AreEqual(oname, okey.Name);

                var rkey = await ipfs.Key.RenameAsync(oname, rname);
                Assert.AreEqual(okey.Id, rkey.Id);
                Assert.AreEqual(rname, rkey.Name);

                var keys = await ipfs.Key.ListAsync();
                Assert.IsTrue(keys.Any(k => k.Name == rname));
                Assert.IsFalse(keys.Any(k => k.Name == oname));
            }
            finally
            {
                try
                {
                    await ipfs.Key.RemoveAsync(oname);
                }
                catch (Exception) { }
                try
                {
                    await ipfs.Key.RemoveAsync(rname);
                }
                catch (Exception) { }
            }
        }

    }
}
