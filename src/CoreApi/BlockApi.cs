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

    class BlockApi : IBlockApi
    {
        IpfsClient ipfs;

        internal BlockApi(IpfsClient ipfs)
        {
            this.ipfs = ipfs;
        }

        public async Task<IDataBlock> GetAsync(Cid id, CancellationToken cancel = default(CancellationToken)) // TODO CID support
        {
            var data = await ipfs.DownloadBytesAsync("block/get", cancel, id);
            return new Block
            {
                DataBytes = data,
                Id = id
            };
        }

        public async Task<Cid> PutAsync(
            byte[] data,
            string contentType = Cid.DefaultContentType,
            string multiHash = MultiHash.DefaultAlgorithmName,
            bool pin = false,
            CancellationToken cancel = default(CancellationToken))
        {
            var options = new List<string>();
            if (multiHash != MultiHash.DefaultAlgorithmName || contentType != Cid.DefaultContentType)
            {
                options.Add($"mhtype={multiHash}");
                options.Add($"format={contentType}");
            }
            var json = await ipfs.UploadAsync("block/put", cancel, data, options.ToArray());
            var info = JObject.Parse(json);
            Cid cid = (string)info["Key"];

            if (pin)
            {
                await ipfs.Pin.AddAsync(cid, recursive: false, cancel: cancel);
            }

            return cid;
        }

        public async Task<Cid> PutAsync(
            Stream data,
            string contentType = Cid.DefaultContentType,
            string multiHash = MultiHash.DefaultAlgorithmName,
            bool pin = false,
            CancellationToken cancel = default(CancellationToken))
        {
            var options = new List<string>();
            if (multiHash != MultiHash.DefaultAlgorithmName || contentType != Cid.DefaultContentType)
            {
                options.Add($"mhtype={multiHash}");
                options.Add($"format={contentType}");
            }
            var json = await ipfs.UploadAsync("block/put", cancel, data, options.ToArray());
            var info = JObject.Parse(json);
            Cid cid = (string)info["Key"];

            if (pin)
            {
                await ipfs.Pin.AddAsync(cid, recursive: false, cancel: cancel);
            }

            return cid;
        }

        public async Task<IDataBlock> StatAsync(Cid id, CancellationToken cancel = default(CancellationToken))
        {
            var json = await ipfs.DoCommandAsync("block/stat", cancel, id);
            var info = JObject.Parse(json);
            return new Block
            {
                Size = (long)info["Size"],
                Id = (string)info["Key"]
            };
        }

        public async Task<Cid> RemoveAsync(Cid id, bool ignoreNonexistent = false, CancellationToken cancel = default(CancellationToken)) // TODO CID support
        {
            var json = await ipfs.DoCommandAsync("block/rm", cancel, id, "force=" + ignoreNonexistent.ToString().ToLowerInvariant());
            if (json.Length == 0)
                return null;
            var result = JObject.Parse(json);
            var error = (string)result["Error"];
            if (error != null)
                throw new HttpRequestException(error);
            return (Cid)(string)result["Hash"];
        }

    }

}
