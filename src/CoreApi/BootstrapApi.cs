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

    class BootstrapApi : IBootstrapApi
    {
        IpfsClient ipfs;

        internal BootstrapApi(IpfsClient ipfs)
        {
            this.ipfs = ipfs;
        }

        public async Task<MultiAddress> AddAsync(MultiAddress address, CancellationToken cancel = default(CancellationToken))
        {
            var json = await ipfs.DoCommandAsync("bootstrap/add", cancel, address.ToString());
            var addrs = (JArray)(JObject.Parse(json)["Peers"]);
            var a = addrs.FirstOrDefault();
            if (a == null)
                return null;
            return new MultiAddress((string)a);
        }

        public async Task<IEnumerable<MultiAddress>> AddDefaultsAsync(CancellationToken cancel = default(CancellationToken))
        {
            var json = await ipfs.DoCommandAsync("bootstrap/add/default", cancel);
            var addrs = (JArray)(JObject.Parse(json)["Peers"]);
            return addrs
                .Select(a => MultiAddress.TryCreate((string)a))
                .Where(ma => ma != null);
        }

        public async Task<IEnumerable<MultiAddress>> ListAsync(CancellationToken cancel = default(CancellationToken))
        {
            var json = await ipfs.DoCommandAsync("bootstrap/list", cancel);
            var addrs = (JArray)(JObject.Parse(json)["Peers"]);
            return addrs
                .Select(a => MultiAddress.TryCreate((string)a))
                .Where(ma => ma != null);
        }

        public Task RemoveAllAsync(CancellationToken cancel = default(CancellationToken))
        {
            return ipfs.DoCommandAsync("bootstrap/rm/all", cancel);
        }

        public async Task<MultiAddress> RemoveAsync(MultiAddress address, CancellationToken cancel = default(CancellationToken))
        {
            var json = await ipfs.DoCommandAsync("bootstrap/rm", cancel, address.ToString());
            var addrs = (JArray)(JObject.Parse(json)["Peers"]);
            var a = addrs.FirstOrDefault();
            if (a == null)
                return null;
            return new MultiAddress((string)a);
        }
    }

}
