using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ipfs.Api
{

    /// <summary>
    ///   Manages pinned objects (local stored and permanent).
    /// </summary>
    /// <remarks>
    ///   This API is accessed via the <see cref="IpfsClient.Pin"/> property.
    ///   <para>
    ///   Pinned objects are locally stored and never garbage collected.
    ///   </para>
    /// </remarks>
    /// <seealso href="https://github.com/ipfs/interface-ipfs-core/tree/master/API/pin">Pin API</seealso>
    public class PinApi
    {
        IpfsClient ipfs;

        internal PinApi(IpfsClient ipfs)
        {
            this.ipfs = ipfs;
        }

        /// <summary>
        ///   Adds an IPFS object to the pinset and also stores it to the IPFS repo. pinset is the set of hashes currently pinned (not gc'able).
        /// </summary>
        /// <param name="hash">
        ///   The <see cref="string"/> representation of a base58 encoded <see cref="Ipfs.MultiHash"/>.
        /// </param>
        /// <param name="recursive">
        ///   <b>true</b> to recursively pin links of object; otherwise, <b>false</b> to only pin
        ///   the specified object.  Default is <b>true</b>.
        /// </param>
        public async Task<PinnedObject[]> AddAsync(string hash, bool recursive = true)
        {
            var opts = "recursive=" + recursive.ToString().ToLowerInvariant();
            var json = await ipfs.DoCommandAsync("pin/add", hash, opts);
            return ((JArray)JObject.Parse(json)["Pins"])
                .Select(p => new PinnedObject { Id = (string)p })
                .ToArray();
        }

        /// <summary>
        ///   List all the objects pinned to local storage.
        /// </summary>
        /// <param name="mode">
        ///   The <see cref="PinMode">type</see> of pinned objects to return.
        ///   Defaults to <see cref="PinMode.All"/>.
        /// </param>
        public async Task<PinnedObject[]> ListAsync(PinMode mode = PinMode.All)
        {
            var filter = "type=" + mode.ToString().ToLowerInvariant();
            var json = await ipfs.DoCommandAsync("pin/ls", null, filter);
            var keys = (JObject)(JObject.Parse(json)["Keys"]);
            return keys
                .Properties()
                .Select(p => new PinnedObject
                {
                    Id = p.Name,
                    Mode = (PinMode)Enum.Parse(typeof(PinMode), (string)keys[p.Name]["Type"], true)
                })
                .ToArray();
        }

        /// <summary>
        ///   Unpin an object.
        /// </summary>
        /// <param name="hash">
        ///   The <see cref="string"/> representation of a base58 encoded <see cref="Ipfs.MultiHash"/>.
        /// </param>
        /// <param name="recursive">
        ///   <b>true</b> to recursively unpin links of object; otherwise, <b>false</b> to only unpin
        ///   the specified object.  Default is <b>true</b>.
        /// </param>
        public async Task<PinnedObject[]> RemoveAsync(string hash, bool recursive = true)
        {
            var opts = "recursive=" + recursive.ToString().ToLowerInvariant();
            var json = await ipfs.DoCommandAsync("pin/rm", hash, opts);
            return ((JArray)JObject.Parse(json)["Pins"])
                .Select(p => new PinnedObject { Id = (string)p })
                .ToArray();
        }
    }

}
