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

namespace Ipfs.Api
{

    /// <summary>
    ///   Manages asymmetric keys.
    /// </summary>
    /// <remarks>
    ///   <note>
    ///   The Key API is work in progress! There be dragons here.
    ///   </note>
    ///   <para>
    ///   This API is accessed via the <see cref="IpfsClient.Key"/> property.
    ///   </para>
    /// </remarks>
    /// <seealso href="https://github.com/ipfs/specs/tree/master/keystore">Key API</seealso>
    public class KeyApi
    {
        static ILog log = LogManager.GetLogger<KeyApi>();

        /// <summary>
        ///   Information about a local key.
        /// </summary>
        public class KeyInfo
        {
            /// <summary>
            ///   Unique identifier (multihash)
            /// </summary>
            /// <value>
            ///   This is the <see cref="MultiHash"/> of the key's public key.
            /// </value>
            public string Id { get; set; }

            /// <summary>
            ///   The locally assigned name to the key.
            /// </summary>
            /// <value>
            ///   The name is only unique within the local peer node. The
            ///   <see cref="Id"/> is universally unique.
            /// </value>
            public string Name { get; set; }

        }
        IpfsClient ipfs;

        internal KeyApi(IpfsClient ipfs)
        {
            this.ipfs = ipfs;
        }

        /// <summary>
        ///   Creates a new key.
        /// </summary>
        /// <param name="name">
        ///   The local name of the key.
        /// </param>
        /// <param name="keyType">
        ///   The type of key to create; "rsa" or "ed25519".
        /// </param>
        /// <param name="size">
        ///   The size, in bits, of the key.
        /// </param>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
        /// </param>
        /// <returns>
        ///   The information on the newly created key.
        /// </returns>
        public async Task<KeyInfo> CreateAsync(string name, string keyType, int size, CancellationToken cancel = default(CancellationToken))
        {
            return await ipfs.DoCommandAsync<KeyInfo>("key/gen", cancel, name, 
                $"type={keyType}", $"size={size}");
        }

        /// <summary>
        ///   List all the keys.
        /// </summary>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
        /// </param>
        /// <returns>
        ///   A sequence of IPFS keys.
        /// </returns>
        public async Task<IEnumerable<KeyInfo>> ListAsync(CancellationToken cancel = default(CancellationToken))
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

        /// <summary>
        ///   Delete the specified key.
        /// </summary>
        /// <param name="name">
        ///   The local name of the key.
        /// </param>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
        /// </param>
        /// <returns>
        ///   A sequence of IPFS keys that were deleted.
        /// </returns>
        public async Task<IEnumerable<KeyInfo>> RemoveAsync(string name, CancellationToken cancel = default(CancellationToken))
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
