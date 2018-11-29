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
using System.Globalization;

namespace Ipfs.Http
{

    class DagApi : IDagApi
    {
        IpfsClient ipfs;

        internal DagApi(IpfsClient ipfs)
        {
            this.ipfs = ipfs;
        }


        public async Task<Cid> PutAsync(
            JObject data,
            string contentType = "dag-cbor",
            string multiHash = MultiHash.DefaultAlgorithmName,
            string encoding = MultiBase.DefaultAlgorithmName,
            bool pin = true,
            CancellationToken cancel = default(CancellationToken))
        {
            using (var ms = new MemoryStream())
            {
                using (var sw = new StreamWriter(ms, new UTF8Encoding(false), 4096, true) { AutoFlush = true })
                using (var jw = new JsonTextWriter(sw))
                {
                    var serializer = new JsonSerializer
                    {
                        Culture = CultureInfo.InvariantCulture
                    };
                    serializer.Serialize(jw, data);
                }
                ms.Position = 0;
                return await PutAsync(ms, contentType, multiHash, encoding, pin, cancel);
            }
        }

        public async Task<Cid> PutAsync(
            object data, 
            string contentType = "dag-cbor",
            string multiHash = MultiHash.DefaultAlgorithmName,
            string encoding = MultiBase.DefaultAlgorithmName,
            bool pin = true, 
            CancellationToken cancel = default(CancellationToken))
        {
            using (var ms = new MemoryStream(
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)),
                false))
            {
                return await PutAsync(ms, contentType, multiHash, encoding, pin, cancel);
            }
        }

        public async Task<Cid> PutAsync(
            Stream data,
            string contentType = "dag-cbor",
            string multiHash = MultiHash.DefaultAlgorithmName,
            string encoding = MultiBase.DefaultAlgorithmName,
            bool pin = true,
            CancellationToken cancel = default(CancellationToken))
        {
            var json = await ipfs.UploadAsync("dag/put", cancel,
                data, null,
                $"format={contentType}",
                $"pin={pin.ToString().ToLowerInvariant()}",
                $"hash={multiHash}",
                $"cid-base={encoding}");
            var result = JObject.Parse(json);
            return (Cid)(string)result["Cid"]["/"];
        }

        public async Task<JObject> GetAsync(
            Cid id, 
            CancellationToken cancel = default(CancellationToken))
        {
            var json = await ipfs.DoCommandAsync("dag/get", cancel, id);
            return JObject.Parse(json);
        }


        public async Task<JToken> GetAsync(
            string path, 
            CancellationToken cancel = default(CancellationToken))
        {
            var json = await ipfs.DoCommandAsync("dag/get", cancel, path);
            return JToken.Parse(json);
        }

        public async Task<T> GetAsync<T>(Cid id, CancellationToken cancel = default(CancellationToken))
        {
            var json = await ipfs.DoCommandAsync("dag/get", cancel, id);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
