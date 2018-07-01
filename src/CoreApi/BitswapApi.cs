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

        public Task<IDataBlock> GetAsync(Cid id, CancellationToken cancel = default(CancellationToken))
        {
            return ipfs.Block.GetAsync(id, cancel);
        }

        public async Task<IEnumerable<Cid>> WantsAsync(MultiHash peer = null, CancellationToken cancel = default(CancellationToken))
        {
            var json = await ipfs.DoCommandAsync("bitswap/wantlist", cancel, peer?.ToString());
            var keys = (JArray)(JObject.Parse(json)["Keys"]);
            // https://github.com/ipfs/go-ipfs/issues/5077
            return keys
                .Select(k => 
                {
                    if (k.Type == JTokenType.String)
                        return Cid.Decode(k.ToString());
                    var obj = (JObject)k;
                    return Cid.Decode(obj["/"].ToString());
                });
        }

        public async Task UnwantAsync(Cid id, CancellationToken cancel = default(CancellationToken))
        {
            await ipfs.DoCommandAsync("bitswap/unwant", cancel, id);
        }
    }

}
