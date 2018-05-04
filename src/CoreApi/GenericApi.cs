using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using Ipfs.CoreApi;
using Newtonsoft.Json.Linq;

namespace Ipfs.Api
{
    public partial class IpfsClient : IGenericApi
    {
        /// <inheritdoc />
        public Task<Peer> IdAsync(MultiHash peer = null, CancellationToken cancel = default(CancellationToken))
        {
            return DoCommandAsync<Peer>("id", cancel, peer?.ToString());
        }

        public async Task<string> ResolveAsync(string name, bool recursive = false, CancellationToken cancel = default(CancellationToken))
        {
            var json = await DoCommandAsync("resolve", cancel,
                name,
                $"recursive={recursive.ToString().ToLowerInvariant()}");
            var path = (string)(JObject.Parse(json)["Path"]);
            return path;
        }

        /// <inheritdoc />
        public async Task ShutdownAsync()
        {
            await DoCommandAsync("shutdown", default(CancellationToken));
        }

        /// <inheritdoc />
        public Task<Dictionary<string, string>> VersionAsync(CancellationToken cancel = default(CancellationToken))
        {
            return DoCommandAsync<Dictionary<string, string>>("version", cancel);
        }

    }
}
