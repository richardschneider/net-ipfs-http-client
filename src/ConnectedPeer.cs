using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ipfs.Api
{
    /// <summary>
    ///   A <see cref="Peer"/> that is connected to the local peer node.
    /// </summary>
    public class ConnectedPeer : Peer
    {
        /// <summary>
        ///   The <see cref="MultiAddress"/> that the peer is connected on.
        /// </summary>
        /// <remarks>
        ///    The <b>MultiAddress</b> contains the IPFS <see cref="Peer.Id"/>, such as
        ///    <c>/ip4/104.131.131.82/tcp/4001/ipfs/QmaCpDMGvV2BGHeYERUEnRQAwe3N8SzbUtfsmvsqQLuvuJ</c>.
        /// </remarks>
        public MultiAddress ConnectedAddress { get; set; }

        /// <summary>
        /// The round-trip time it takes to get data from a peer.
        /// </summary>
        public TimeSpan Latency { get; set; }
    }
}
