using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ipfs.CoreApi;
using System.IO;

namespace Ipfs.Api
{

    class BitswapApi : IBitswapApi
    {
        IpfsClient ipfs;

        internal BitswapApi(IpfsClient ipfs)
        {
            this.ipfs = ipfs;
        }
    }

}
