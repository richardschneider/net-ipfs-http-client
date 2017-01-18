using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Text;

namespace Ipfs.Api
{
    [TestClass]
    public class DagApiTest
    {
        [TestMethod]
        public void GetAsync()
        {
            var ipfs = TestFixture.Ipfs;
            ExceptionAssert.Throws<NotImplementedException>(() => ipfs.Dag.GetAsync("cid"));
        }

        [TestMethod]
        public void PutAsync()
        {
            var ipfs = TestFixture.Ipfs;
            ExceptionAssert.Throws<NotImplementedException>(() => ipfs.Dag.PutAsync(null, null));
        }

    }
}

