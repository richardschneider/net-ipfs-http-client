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

    class DagApi : IDagApi
    {
        IpfsClient ipfs;

        internal DagApi(IpfsClient ipfs)
        {
            this.ipfs = ipfs;
        }


        public Task<Cid> PutAsync(ILinkedNode data, string contentType, string multiHash = MultiHash.DefaultAlgorithmName, CancellationToken cancel = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        Task<ILinkedNode> IDagApi.GetAsync(string path, CancellationToken cancel)
        {
            throw new NotImplementedException();
        }
    }
}
