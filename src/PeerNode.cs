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
        public string Id { get; set; }
        public string PublicKey { get; set; }
        public string[] Addresses { get; set; }
        public string AgentVersion { get; set; }
        public string ProtocolVersion { get; set; }
    }
}
