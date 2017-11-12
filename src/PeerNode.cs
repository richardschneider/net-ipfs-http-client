using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ipfs.Api
{
    /// <summary>
    ///   An IPFS node.
    /// </summary>
    public class PeerNode
    {
        static MultiAddress[] noAddress = new MultiAddress[0];

        /// <summary>
        ///   Unique identifier (multihash)
        /// </summary>
        /// <value>
        ///   This is the <see cref="MultiHash"/> of the peer's <see cref="PublicKey"/>.
        /// </value>
        public string Id { get; set; }

        /// <summary>
        ///   The public key of the node.
        /// </summary>
        /// <value>
        ///   The base 64 encoding of the node's public key.
        /// </value>
        public string PublicKey { get; set; }

        /// <summary>
        ///   The multiple addresses of the node.
        /// </summary>
        /// <value>
        ///   Where the peer can be found.
        /// </value>
        public IEnumerable<MultiAddress> Addresses { get; set; } = noAddress;

        /// <summary>
        ///   The name and version of the IPFS software.
        /// </summary>
        /// <value>
        ///   For example "go-ipfs/0.4.7/".
        /// </value>
        /// <remarks>
        ///   There is no specification that describes the agent version string.
        /// </remarks>
        public string AgentVersion { get; set; }

        /// <summary>
        ///  The name and version of the supported IPFS protocol.
        /// </summary>
        /// <value>
        ///   For example "go-ipfs/0.4.7/".
        /// </value>
        /// <remarks>
        ///   There is no specification that describes the protocol version string.
        /// </remarks>
        public string ProtocolVersion { get; set; }

        /// <summary>
        ///   Determines if the information on the peer is valid.
        /// </summary>
        /// <returns>
        ///   <b>true</b> if all validation rules pass; otherwise <b>false</b>.
        /// </returns>
        /// <remarks>
        ///    Verifies that
        ///    <list type="bullet">
        ///      <item><description>The <see cref="Id"/> is a hash of the <see cref="PublicKey"/></description></item>
        ///      <item><description>All <see cref="Addresses"/> point to the node</description></item>
        ///    </list>
        /// </remarks>
        public bool IsValid()
        {
            var mh = new MultiHash(Id);
            if (!mh.Matches(Convert.FromBase64String(PublicKey)))
                return false;
            if (!Addresses.All(a => 
                a.Protocols.Last().Name == "ipfs" &&
                a.Protocols.Last().Value == Id))
                return false;
            return true;
        }
    }
}
