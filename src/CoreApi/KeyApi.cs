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
    class KeyApi  : IKeyApi
    {
        /// <summary>
        ///   Information about a local key.
        /// </summary>
        public class KeyInfo : IKey
        {
            /// <inheritdoc />
            public MultiHash Id { get; set; }

            /// <inheritdoc />
            public string Name { get; set; }

            /// <inheritdoc />
            public override string ToString()
            {
                return Name;
            }

        }
        IpfsClient ipfs;

        internal KeyApi(IpfsClient ipfs)
        {
            this.ipfs = ipfs;
        }

        public async Task<IKey> CreateAsync(string name, string keyType, int size, CancellationToken cancel = default(CancellationToken))
        {
            return await ipfs.DoCommandAsync<KeyInfo>("key/gen", cancel, 
                name, 
                $"type={keyType}", 
                $"size={size}");
        }

        public async Task<IEnumerable<IKey>> ListAsync(CancellationToken cancel = default(CancellationToken))
        {
            var json = await ipfs.DoCommandAsync("key/list", cancel, null, "l=true");
            var keys = (JArray)(JObject.Parse(json)["Keys"]);
            return keys
                .Select(k => new KeyInfo
                {
                    Id = (string)k["Id"],
                    Name = (string)k["Name"]
                });
        }

        public async Task<IEnumerable<IKey>> RemoveAsync(string name, CancellationToken cancel = default(CancellationToken))
        {
            var json = await ipfs.DoCommandAsync("key/rm", cancel, name);
            var keys = (JArray)(JObject.Parse(json)["Keys"]);
            return keys
                .Select(k => new KeyInfo
                {
                    Id = (string)k["Id"],
                    Name = (string)k["Name"]
                });
        }
    }
}
