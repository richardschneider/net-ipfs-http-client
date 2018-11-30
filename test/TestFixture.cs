using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ipfs.Http
{
    public static class TestFixture
    {
        /// <summary>
        ///   Fiddler cannot see localhost traffic because .Net bypasses the network stack for localhost/127.0.0.1. 
        ///   By using "127.0.0.1." (note trailing dot) fiddler will receive the traffic and if its not running
        ///   the localhost will get it!
        /// </summary>
        //IpfsClient.DefaultApiUri = new Uri("http://127.0.0.1.:5001");

        public static IpfsClient Ipfs = new IpfsClient();
    }
}
