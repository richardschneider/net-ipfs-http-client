using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ipfs.CoreApi;
using System.IO;

namespace Ipfs.Api
{

    class NameApi : INameApi
    {
        IpfsClient ipfs;

        internal NameApi(IpfsClient ipfs)
        {
            this.ipfs = ipfs;
        }

        public async Task<NamedContent> PublishAsync(string path, bool resolve = true, string key = "self", TimeSpan? lifetime = null, CancellationToken cancel = default(CancellationToken))
        {
            var json = await ipfs.DoCommandAsync("name/publish", cancel,
                path,
                "lifetime=24h",
                $"resolve={resolve.ToString().ToLowerInvariant()}",
                $"key={key}");
            // TODO: lifetime
            var info = JObject.Parse(json);
            return new NamedContent
            {
                NamePath = (string)info["Name"],
                ContentPath = (string)info["Value"]
            };
        }

        public Task<NamedContent> PublishAsync(Cid id, string key = "self", TimeSpan? lifetime = null, CancellationToken cancel = default(CancellationToken))
        {
            return PublishAsync("/ipfs/" + id.Encode(), false, key, lifetime, cancel);
        }

        public async Task<string> ResolveAsync(string name, bool recursive = false, bool nocache = false, CancellationToken cancel = default(CancellationToken))
        {
            var json = await ipfs.DoCommandAsync("name/resolve", cancel,
                name,
                $"recursive={recursive.ToString().ToLowerInvariant()}",
                $"nocache={nocache.ToString().ToLowerInvariant()}");
            var path = (string)(JObject.Parse(json)["Path"]);
            return path;
        }
    }
}
