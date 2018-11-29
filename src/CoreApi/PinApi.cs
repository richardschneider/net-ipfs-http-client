using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ipfs.CoreApi;

namespace Ipfs.Http
{

    class PinApi : IPinApi
    {
        IpfsClient ipfs;

        internal PinApi(IpfsClient ipfs)
        {
            this.ipfs = ipfs;
        }

        public async Task<IEnumerable<Cid>> AddAsync(string path, bool recursive = true, CancellationToken cancel = default(CancellationToken))
        {
            var opts = "recursive=" + recursive.ToString().ToLowerInvariant();
            var json = await ipfs.DoCommandAsync("pin/add", cancel, path, opts);
            return ((JArray)JObject.Parse(json)["Pins"])
                .Select(p => (Cid)(string)p);
        }

        public async Task<IEnumerable<Cid>> ListAsync(CancellationToken cancel = default(CancellationToken))
        {
            var json = await ipfs.DoCommandAsync("pin/ls", cancel);
            var keys = (JObject)(JObject.Parse(json)["Keys"]);
            return keys
                .Properties()
                .Select(p => (Cid)p.Name);
        }

        public async Task<IEnumerable<Cid>> RemoveAsync(Cid id, bool recursive = true, CancellationToken cancel = default(CancellationToken))
        {
            var opts = "recursive=" + recursive.ToString().ToLowerInvariant();
            var json = await ipfs.DoCommandAsync("pin/rm", cancel, id, opts);
            return ((JArray)JObject.Parse(json)["Pins"])
                .Select(p => (Cid)(string)p);
        }

    }

}
