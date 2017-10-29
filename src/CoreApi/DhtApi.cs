using Common.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ipfs.Api
{

    /// <summary>
    ///   Manages the Distributed Hash Table.
    /// </summary>
    /// <remarks>
    ///   The DHT is a place to store, not the value, but pointers to peers who have 
    ///   the actual value.
    ///   <para>
    ///   This API is accessed via the <see cref="IpfsClient.Dht"/> property.
    ///   </para>
    /// </remarks>
    /// <seealso href="https://github.com/ipfs/interface-ipfs-core/tree/master/API/dht">Dht API</seealso>
    public class DhtApi
    {
        static ILog log = LogManager.GetLogger<DhtApi>();

        IpfsClient ipfs;

        internal DhtApi(IpfsClient ipfs)
        {
            this.ipfs = ipfs;
        }

        /// <summary>
        ///   Information about an IPFS peer.
        /// </summary>
        /// <param name="id">
        ///   The <see cref="string"/> ID of the IPFS peer.  
        /// </param>
        public Task<PeerNode> FindPeerAsync(string id, CancellationToken cancel = default(CancellationToken))
        {
            return ipfs.IdAsync(id, cancel);
        }

        /// <summary>
        ///   Find the providers for content that is addressed by a hash.
        /// </summary>
        /// <param name="hash">
        ///   The <see cref="string"/> representation of a base58 encoded <see cref="Ipfs.MultiHash"/>.
        /// </param>
        /// <returns>
        ///   A sequence of IPFS peer IDs.
        /// </returns>
        public async Task<IEnumerable<string>> FindProvidersAsync(string hash, CancellationToken cancel = default(CancellationToken))
        {
            var serializer = new JsonSerializer();
            var stream = await ipfs.PostDownloadAsync("dht/findprovs", cancel, hash);
            return ProviderFromStream(stream);
        }

        IEnumerable<string> ProviderFromStream(Stream stream)
        { 
            using (var sr = new StreamReader(stream))
            {
                while (!sr.EndOfStream)
                {
                    var json = sr.ReadLine();
                    if (log.IsDebugEnabled)
                        log.DebugFormat("Provider {0}", json);

                    var r = JObject.Parse(json);
                    var id = (string)r["ID"];
                    if (id != String.Empty)
                        yield return id;
                    else
                    {
                        var responses = (JArray)r["Responses"];
                        if (responses != null)
                        {
                            foreach (var response in responses)
                            {
                                var rid = (string)response["ID"];
                                yield return rid;
                            }
                        }
                    }
                }
            }
         }
    }

}
