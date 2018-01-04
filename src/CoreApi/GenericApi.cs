using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Threading;

namespace Ipfs.Api
{
    public partial class IpfsClient
    {
        /// <summary>
        ///   Information about an IPFS peer.
        /// </summary>
        /// <param name="peer">
        ///   The id of the IPFS peer.  If not specified (e.g. null), then the local
        ///   peer is used.
        /// </param>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
        /// </param>
        /// <returns>
        ///   Information on the peer node.
        /// </returns>
        public Task<Peer> IdAsync(MultiHash peer = null, CancellationToken cancel = default(CancellationToken))
        {
            return DoCommandAsync<Peer>("id", cancel, peer?.ToString());
        }

        /// <summary>
        ///   Get the version information of the API server.
        /// </summary>
        public Task<Dictionary<string, string>> VersionAsync(CancellationToken cancel = default(CancellationToken))
        {
            return DoCommandAsync<Dictionary<string, string>>("version", cancel);
        }

    }
}
