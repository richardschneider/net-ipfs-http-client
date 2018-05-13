using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Ipfs.Api
{
    [TestClass]
    public class DagApiTest
    {
        class Name
        {
            public string First { get; set; }
            public string Last { get; set; }
        }

        [TestMethod]
        public async Task PutAndGet_JSON()
        {
            var ipfs = TestFixture.Ipfs;
            var expected = new JObject();
            expected["a"] = "alpha";
            var id = await ipfs.Dag.PutAsync(expected);
            Assert.IsNotNull(id);

            var actual = await ipfs.Dag.GetAsync(id);
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected["a"], actual["a"]);

            var value = (string) await ipfs.Dag.GetAsync(id.Encode() + "/a");
            Assert.AreEqual(expected["a"], value);
        }

        [TestMethod]
        public async Task PutAndGet_POCO()
        {
            var ipfs = TestFixture.Ipfs;
            var expected = new Name { First = "John", Last = "Smith" };
            var id = await ipfs.Dag.PutAsync(expected);
            Assert.IsNotNull(id);

            var actual = await ipfs.Dag.GetAsync<Name>(id);
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.First, actual.First);
            Assert.AreEqual(expected.Last, actual.Last);

            var value = (string)await ipfs.Dag.GetAsync(id.Encode() + "/Last");
            Assert.AreEqual(expected.Last, value);
        }
    }
}

