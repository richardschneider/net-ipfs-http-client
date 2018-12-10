using Common.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ipfs.CoreApi;

namespace Ipfs.Http
{

    class StatApi : IStatsApi
    {
        IpfsClient ipfs;

        internal StatApi(IpfsClient ipfs)
        {
            this.ipfs = ipfs;
        }

        public Task<BandwidthData> BandwidthAsync(CancellationToken cancel = default(CancellationToken))
        {
            return ipfs.DoCommandAsync<BandwidthData>("stats/bw", cancel);
        }

        public async Task<BitswapData> BitswapAsync(CancellationToken cancel = default(CancellationToken))
        {
            var json = await ipfs.DoCommandAsync("stats/bitswap", cancel);
            var stat = JObject.Parse(json);
            return new BitswapData
            {
                BlocksReceived = (ulong)stat["BlocksReceived"],
                DataReceived = (ulong)stat["DataReceived"],
                BlocksSent = (ulong)stat["BlocksSent"],
                DataSent = (ulong)stat["DataSent"],
                DupBlksReceived = (ulong)stat["DupBlksReceived"],
                DupDataReceived = (ulong)stat["DupDataReceived"],
                ProvideBufLen = (int)stat["ProvideBufLen"],
                Peers = ((JArray)stat["Peers"]).Select(s => new MultiHash((string)s)),
                Wantlist = ((JArray)stat["Wantlist"]).Select(o => Cid.Decode(o["/"].ToString()))
            };
        }

        public Task<RepositoryData> RepositoryAsync(CancellationToken cancel = default(CancellationToken))
        {
            return ipfs.DoCommandAsync<RepositoryData>("stats/repo", cancel);
        }


    }
}
