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

        public Task<Cid> PublishAsync(string path, bool resolve = true, string key = "self", TimeSpan? lifetime = null, CancellationToken cancel = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<Cid> ResolveAsync(string name, bool recursive = false, bool nocache = false, CancellationToken cancel = default(CancellationToken))
        {
            throw new NotImplementedException();
        }
    }
}
