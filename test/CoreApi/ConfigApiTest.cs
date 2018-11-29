using Ipfs.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Ipfs.Http
{

    [TestClass]
    public class ConfigApiTest
    {
        const string apiAddress = "/ip4/127.0.0.1/tcp/";
        const string gatewayAddress = "/ip4/127.0.0.1/tcp/";

        [TestMethod]
        public void Get_Entire_Config()
        {
            IpfsClient ipfs = TestFixture.Ipfs;
            var config = ipfs.Config.GetAsync().Result;
            StringAssert.StartsWith(config["Addresses"]["API"].Value<string>(), apiAddress);
        }

        [TestMethod]
        public void Get_Scalar_Key_Value()
        {
            IpfsClient ipfs = TestFixture.Ipfs;
            var api = ipfs.Config.GetAsync("Addresses.API").Result;
            StringAssert.StartsWith(api.Value<string>(), apiAddress);
        }

        [TestMethod]
        public void Get_Object_Key_Value()
        {
            IpfsClient ipfs = TestFixture.Ipfs;
            var addresses = ipfs.Config.GetAsync("Addresses").Result;
            StringAssert.StartsWith(addresses["API"].Value<string>(), apiAddress);
            StringAssert.StartsWith(addresses["Gateway"].Value<string>(), gatewayAddress);
        }

        [TestMethod]
        public void Keys_are_Case_Sensitive()
        {
            IpfsClient ipfs = TestFixture.Ipfs;
            var api = ipfs.Config.GetAsync("Addresses.API").Result;
            StringAssert.StartsWith(api.Value<string>(), apiAddress);

            ExceptionAssert.Throws<Exception>(() => { var x = ipfs.Config.GetAsync("Addresses.api").Result; });
        }

        [TestMethod]
        public void Set_String_Value()
        {
            const string key = "foo";
            const string value = "foobar";
            IpfsClient ipfs = TestFixture.Ipfs;
            ipfs.Config.SetAsync(key, value).Wait();
            Assert.AreEqual(value, ipfs.Config.GetAsync(key).Result);
        }

        [TestMethod]
        public void Set_JSON_Value()
        {
            const string key = "API.HTTPHeaders.Access-Control-Allow-Origin";
            JToken value = JToken.Parse("['http://example.io']");
            IpfsClient ipfs = TestFixture.Ipfs;
            ipfs.Config.SetAsync(key, value).Wait();
            Assert.AreEqual("http://example.io", ipfs.Config.GetAsync(key).Result[0]);
        }

        [TestMethod]
        public async Task Replace_Entire_Config()
        {
            IpfsClient ipfs = TestFixture.Ipfs;
            var original = await ipfs.Config.GetAsync();
            try
            {
                var a = JObject.Parse("{ \"foo-x-bar\": 1 }");
                await ipfs.Config.ReplaceAsync(a);
            }
            finally
            {
                await ipfs.Config.ReplaceAsync(original);
            }
        }

    }
}
