using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ipfs.Api
{

    /// <summary>
    ///   Allows you to publish messages to a given topic, and also to
    ///   subscribe to new messages on a given topic.
    /// </summary>
    /// <remarks>
    ///   This API is accessed via the <see cref="IpfsClient.PubSub"/> property.
    ///   <para>
    ///   This is an experimental feature. It is not intended in its current state
    ///   to be used in a production environment.
    ///   </para>
    ///   <para>
    ///   To use, the daemon must be run with '--enable-pubsub-experiment'.
    ///   </para>
    /// </remarks>
    /// <seealso href="https://github.com/ipfs/interface-ipfs-core/blob/master/SPEC/PUBSUB.md">PUBSUB API</seealso>
    public class PubSubApi
    {
        IpfsClient ipfs;

        internal PubSubApi(IpfsClient ipfs)
        {
            this.ipfs = ipfs;
        }

        /// <summary>
        ///   Get the subscribed topics.
        /// </summary>
        /// <returns>
        ///   A sequence of <see cref="string"/> for each topic.
        /// </returns>
        public async Task<IEnumerable<string>> SubscribedTopicsAsync()
        {
            var json = await ipfs.DoCommandAsync("pubsub/ls");
            var result = JObject.Parse(json);
            var strings = (JArray)result["Strings"];
            return strings.Select(s => (string)s);
        }

        /// <summary>
        ///   Get the peers that are pubsubing with us.
        /// </summary>
        /// <returns>
        ///   A sequence of <see cref="string"/> for each peer ID.
        /// </returns>
        public async Task<IEnumerable<string>> PeersAsync()
        {
            var json = await ipfs.DoCommandAsync("pubsub/peers");
            var result = JObject.Parse(json);
            var strings = (JArray)result["Strings"];
            return strings.Select(s => (string)s);
        }

    }

}
