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
using Ipfs.CoreApi;

namespace Ipfs.Api
{

    class DhtApi : IDhtApi
    {
        static ILog log = LogManager.GetLogger<DhtApi>();

        IpfsClient ipfs;

        internal DhtApi(IpfsClient ipfs)
        {
            this.ipfs = ipfs;
        }

        public Task<Peer> FindPeerAsync(MultiHash id, CancellationToken cancel = default(CancellationToken))
        {
            return ipfs.IdAsync(id, cancel);
        }

        public async Task<IEnumerable<Peer>> FindProvidersAsync(Cid id, CancellationToken cancel = default(CancellationToken))
        {
            int limit = 20; // TODO: should be an argument
            var stream = await ipfs.PostDownloadAsync("dht/findprovs", cancel, id, $"num-providers={limit}");
            return ProviderFromStream(stream, limit);
        }

        IEnumerable<Peer> ProviderFromStream(Stream stream, int limit = int.MaxValue)
        { 
            using (var sr = new StreamReader(stream))
            {
                var n = 0;
                while (!sr.EndOfStream && n < limit)
                {
                    var json = sr.ReadLine();
                    if (log.IsDebugEnabled)
                        log.DebugFormat("Provider {0}", json);

                    var r = JObject.Parse(json);
                    var id = (string)r["ID"];
                    if (id != String.Empty)
                    {
                        ++n;
                        yield return new Peer { Id = new MultiHash(id) };
                    }
                    else
                    {
                        var responses = (JArray)r["Responses"];
                        if (responses != null)
                        {
                            foreach (var response in responses)
                            {
                                var rid = (string)response["ID"];
                                if (rid != String.Empty)
                                {
                                    ++n;
                                    yield return new Peer { Id = new MultiHash(rid) };
                                }
                            }
                        }
                    }
                }
            }
         }
    }

}
