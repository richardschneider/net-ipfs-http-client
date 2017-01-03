using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Threading.Tasks;

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
        /// <returns>
        /// </returns>
        public Task<PeerNode> IdAsync(string peer = null)
        {
            return DoCommandAsync<PeerNode>("id", peer);
        }

        /// <summary>
        ///   Get the version information of the API server.
        /// </summary>
        public Task<Dictionary<string, string>> VersionAsync()
        {
            return DoCommandAsync<Dictionary<string, string>>("version");
        }

    }
}
