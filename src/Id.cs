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
        ///   Information about an IPFS node.
        /// </summary>
        /// <param name="peer">
        ///   The id of the peer IPFS node.  If not specified (e.g. null), then the local
        ///   peer node is used.
        /// </param>
        /// <returns>
        /// </returns>
        public Task<PeerNode> Id(string peer = null)
        {
            return DoCommandAsync<PeerNode>("id", peer);
        }
    }
}
