using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ipfs.Api
{
    /// <summary>
    ///   Information about a peer node.
    /// </summary>
    public class PeerNode
    {
        /// <summary>
        ///   Unique identifier (multihash)
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///   The public key of the node.
        /// </summary>
        public string PublicKey { get; set; }

        /// <summary>
        ///   The multi addresses of the node.
        /// </summary>
        public IEnumerable<MultiAddress> Addresses { get; set; }

        /// <summary>
        ///  TODO
        /// </summary>
        public string AgentVersion { get; set; }

        /// <summary>
        ///  TODO
        /// </summary>
        public string ProtocolVersion { get; set; }
    }
}
