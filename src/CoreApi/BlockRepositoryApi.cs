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

namespace Ipfs.Http
{

    class BlockRepositoryApi : IBlockRepositoryApi
    {
        IpfsClient ipfs;

        internal BlockRepositoryApi(IpfsClient ipfs)
        {
            this.ipfs = ipfs;
        }

        public async Task RemoveGarbageAsync(CancellationToken cancel = default(CancellationToken))
        {
            await ipfs.DoCommandAsync("repo/gc", cancel);
        }

        public Task<RepositoryData> StatisticsAsync(CancellationToken cancel = default(CancellationToken))
        {
            return ipfs.DoCommandAsync<RepositoryData>("repo/stat", cancel);
        }

        public async Task VerifyAsync(CancellationToken cancel = default(CancellationToken))
        {
            await ipfs.DoCommandAsync("repo/verify", cancel);
        }

        public async Task<string> VersionAsync(CancellationToken cancel = default(CancellationToken))
        {
            var json = await ipfs.DoCommandAsync("repo/version", cancel);
            var info = JObject.Parse(json);
            return (string)info["Version"];
        }
    }
}
