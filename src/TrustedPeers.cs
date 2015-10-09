using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace Ipfs.Api
{
    public partial class IpfsClient
    {
        class BootstrapListResponse
        {
            public MultiAddress[] Peers { get; set; }
        }

        /// <summary>
        ///   Gets a list of trusted peers.
        /// </summary>
        /// <remarks>
        ///   This is the list of peers that are initially trusted by IPNS. Its equivalent to the
        ///   <c>ipfs bootstrap list</c> command.
        /// </remarks>
        /// <returns>
        ///   A series of <see cref="MultiAddress"/>.  Each address ends with an IPNS node id, for
        ///   example "/ip4/104.131.131.82/tcp/4001/ipfs/QmaCpDMGvV2BGHeYERUEnRQAwe3N8SzbUtfsmvsqQLuvuJ".
        /// </returns>
        public IEnumerable<MultiAddress> TrustedPeers()
        {
            var result = Api().DownloadString(BuildCommand("bootstrap/list"));
            return JsonConvert.DeserializeObject<BootstrapListResponse>(result).Peers;
        }

    }
}
