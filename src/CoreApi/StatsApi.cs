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

        public Task<BitswapData> BitswapAsync(CancellationToken cancel = default(CancellationToken))
        {
            return ipfs.DoCommandAsync<BitswapData>("stats/bitswap", cancel);
        }

        public Task<RepositoryData> RepositoryAsync(CancellationToken cancel = default(CancellationToken))
        {
            return ipfs.DoCommandAsync<RepositoryData>("stats/repo", cancel);
        }


    }
}
