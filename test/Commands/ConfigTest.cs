using Ipfs.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Text;

namespace Ipfs.Api
{

    [TestClass]
    public class ConfigTest
    {
        const string apiAddress = "/ip4/127.0.0.1/tcp/5001";
        const string gatewayAddress = "/ip4/127.0.0.1/tcp/8080";

        [TestMethod]
        public void Get_Entire_Config()
        {
            IpfsClient ipfs = new IpfsClient();
            var config = ipfs.Config.GetAsync().Result;
            Assert.AreEqual(apiAddress, config["Addresses"]["API"]);
        }

        [TestMethod]
        public void Get_Scalar_Key_Value()
        {
            IpfsClient ipfs = new IpfsClient();
            var api = ipfs.Config.GetAsync("Addresses.API").Result;
            Assert.AreEqual(apiAddress, api);
        }

        [TestMethod]
        public void Get_Object_Key_Value()
        {
            IpfsClient ipfs = new IpfsClient();
            var addresses = ipfs.Config.GetAsync("Addresses").Result;
            Assert.AreEqual(apiAddress, addresses["API"]);
            Assert.AreEqual(gatewayAddress, addresses["Gateway"]);
        }

        [TestMethod]
        public void Keys_are_Case_Sensitive()
        {
            IpfsClient ipfs = new IpfsClient();
            var api = ipfs.Config.GetAsync("Addresses.API").Result;
            Assert.AreEqual(apiAddress, api);

            ExceptionAssert.Throws<IpfsException>(() => { var x = ipfs.Config.GetAsync("Addresses.api").Result; });
        }

        [TestMethod]
        public void Set_String_Value()
        {
            const string key = "foo";
            const string value = "foobar";
            IpfsClient ipfs = new IpfsClient();
            ipfs.Config.SetAsync(key, value).Wait();
            Assert.AreEqual(value, ipfs.Config.GetAsync(key).Result);
        }

        [TestMethod]
        public void Set_JSON_Value()
        {
            const string key = "API.HTTPHeaders.Access-Control-Allow-Origin";
            JToken value = JToken.Parse("['http://example.io']");
            IpfsClient ipfs = new IpfsClient();
            ipfs.Config.SetAsync(key, value).Wait();
            Assert.AreEqual("http://example.io", ipfs.Config.GetAsync(key).Result[0]);
        }

    }
}
