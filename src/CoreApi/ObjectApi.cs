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

namespace Ipfs.Http
{

    class ObjectApi : IObjectApi
    {
        static ILog log = LogManager.GetLogger<ObjectApi>();

        IpfsClient ipfs;

        internal ObjectApi(IpfsClient ipfs)
        {
            this.ipfs = ipfs;
        }

        public Task<DagNode> NewDirectoryAsync(CancellationToken cancel = default(CancellationToken))
        {
            return NewAsync("unixfs-dir", cancel);
        }

        public async Task<DagNode> NewAsync(string template = null, CancellationToken cancel = default(CancellationToken))
        {
            var json = await ipfs.DoCommandAsync("object/new", cancel, template);
            var hash = (string) (JObject.Parse(json)["Hash"]);
            return await GetAsync(hash);
        }

        public async Task<DagNode> GetAsync(Cid id, CancellationToken cancel = default(CancellationToken))
        {
            var json = await ipfs.DoCommandAsync("object/get", cancel, id);
            return GetDagFromJson(json);
        }

        public Task<DagNode> PutAsync(byte[] data, IEnumerable<IMerkleLink> links = null, CancellationToken cancel = default(CancellationToken))
        {
            return PutAsync(new DagNode(data, links), cancel);
        }

        public async Task<DagNode> PutAsync(DagNode node, CancellationToken cancel = default(CancellationToken))
        {
            var json = await ipfs.UploadAsync("object/put", cancel, node.ToArray(), "inputenc=protobuf");
            return node;
        }

        public Task<Stream> DataAsync(Cid id, CancellationToken cancel = default(CancellationToken))
        {
            return ipfs.DownloadAsync("object/data", cancel, id);
        }

        public async Task<IEnumerable<IMerkleLink>> LinksAsync(Cid id, CancellationToken cancel = default(CancellationToken))
        {
            var json = await ipfs.DoCommandAsync("object/links", cancel, id);
            return GetDagFromJson(json).Links;
        }

        // TOOD: patch sub API

        DagNode GetDagFromJson(string json)
        {
            var result = JObject.Parse(json);
            byte[] data = null;
            var stringData = (string)result["Data"];
            if (stringData != null)
                data = Encoding.UTF8.GetBytes(stringData);
            var links = ((JArray)result["Links"])
                .Select(link => new DagLink(
                    (string)link["Name"],
                    (string)link["Hash"],
                    (long)link["Size"]));
            return new DagNode(data, links);
        }

        public async Task<ObjectStat> StatAsync(Cid id, CancellationToken cancel = default(CancellationToken))
        {
            var json = await ipfs.DoCommandAsync("object/stat", cancel, id);
            var r = JObject.Parse(json);

            return new ObjectStat
            {
                LinkCount = (int)r["NumLinks"],
                LinkSize = (long)r["LinksSize"],
                BlockSize = (long)r["BlockSize"],
                DataSize = (long)r["DataSize"],
                CumulativeSize = (long)r["CumulativeSize"]
            };
        }
    }
}
