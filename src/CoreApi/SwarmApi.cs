using Common.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ipfs.Api
{

    /// <summary>
    ///   Manages the swarm of peers.
    /// </summary>
    /// <remarks>
    ///   The swarm is a sequence of peer nodes.
    ///   <para>
    ///   This API is accessed via the <see cref="IpfsClient.Swarm"/> property.
    ///   </para>
    /// </remarks>
    /// <seealso href="https://github.com/ipfs/interface-ipfs-core/tree/master/API/swarm">Swarm API</seealso>
    public class SwarmApi
    {
        IpfsClient ipfs;

        internal SwarmApi(IpfsClient ipfs)
        {
            this.ipfs = ipfs;
        }

        /// <summary>
        ///   Get the peers in the current swarm.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<PeerNode>> AddressesAsync()
        {
            var json = await ipfs.DoCommandAsync("swarm/addrs");
            return ((JObject)JObject.Parse(json)["Addrs"])
                .Properties()
                .Select(p => new PeerNode {
                    Id = p.Name,
                    Addresses = ((JArray)p.Value)
                        .Select(v => new MultiAddress((string)v))
                });
        }

        /// <summary>
        ///   Get the peers that are connected to this node.
        /// </summary>
        /// <returns>
        ///   A sequence of <see cref="ConnectedPeer">Connected Peers</see>.
        /// </returns>
        public async Task<IEnumerable<ConnectedPeer>> PeersAsync()
        {
            var json = await ipfs.DoCommandAsync("swarm/peers", null, "verbose=true");
            var result = JObject.Parse(json);

            // Older servers return an array of strings
            var strings = (JArray)result["Strings"];
            if (strings != null)
            {
                return strings
                   .Select(s =>
                   {
                       var parts = ((string)s).Split(' ');
                       var address = new MultiAddress(parts[0]);
                       return new ConnectedPeer
                       {
                           Id = address.Protocols.First(p => p.Name == "ipfs").Value,
                           ConnectedAddress = parts[0],
                           Latency = ParseLatency(parts[1])
                       };
                   });
            }

            // Current servers return JSON
            var peers = (JArray)result["Peers"];
            if (peers != null)
            {
                return peers.Select(p => new ConnectedPeer
                {
                    Id = (string)p["Peer"],
                    ConnectedAddress = new MultiAddress((string)p["Addr"]),
                    Latency = ParseLatency((string)p["Latency"])
                });
            }
            
            // Hmmm. Another change we can handle
            throw new FormatException("Unknown response from 'swarm/peers");
        }

        TimeSpan ParseLatency(string latency)
        {
            if (latency.EndsWith("ms"))
            {
                var ms = Double.Parse(latency.Substring(0, latency.Length - 2));
                return TimeSpan.FromMilliseconds(ms);
            }
            if (latency.EndsWith("s"))
            {
                var sec = Double.Parse(latency.Substring(0, latency.Length - 1));
                return TimeSpan.FromSeconds(sec);
            }

            throw new FormatException(String.Format("Invalid latency unit '{0}'.", latency));
        }

        /// <summary>
        ///   Connect to a peer.
        /// </summary>
        /// <param name="address">
        ///   An ipfs <see cref="MultiAddress"/>, such as
        ///  <c>/ip4/104.131.131.82/tcp/4001/ipfs/QmaCpDMGvV2BGHeYERUEnRQAwe3N8SzbUtfsmvsqQLuvuJ</c>.
        /// </param>
        public async Task ConnectAsync(MultiAddress address)
        {
            await ipfs.DoCommandAsync("swarm/connect", address.ToString());
        }

        /// <summary>
        ///   Disconnect from a peer.
        /// </summary>
        /// <param name="address">
        ///   An ipfs <see cref="MultiAddress"/>, such as
        ///  <c>/ip4/104.131.131.82/tcp/4001/ipfs/QmaCpDMGvV2BGHeYERUEnRQAwe3N8SzbUtfsmvsqQLuvuJ</c>.
        /// </param>
        public async Task DisconnectAsync(MultiAddress address)
        {
            await ipfs.DoCommandAsync("swarm/disconnect", address.ToString());
        }
    }

}
