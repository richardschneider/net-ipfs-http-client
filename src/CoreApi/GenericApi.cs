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

namespace Ipfs.Api
{
    public partial class IpfsClient : IGenericApi
    {
        /// <inheritdoc />
        public Task<Peer> IdAsync(MultiHash peer = null, CancellationToken cancel = default(CancellationToken))
        {
            return DoCommandAsync<Peer>("id", cancel, peer?.ToString());
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
