using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ipfs.Api
{

    /// <summary>
    ///   Information about a raw IPFS Block.
    /// </summary>
    /// <seealso cref="BlockApi.StatAsync"/>
    public class BlockInfo
    {
        /// <summary>
        ///   The <see cref="MultiHash"/> ID of the block.
        /// </summary>
        /// <value>
        ///   The unique ID of the block.
        /// </value>
        public MultiHash Key { get; set; }

        /// <summary>
        ///   The serialised size (in bytes) of the block.
        /// </summary>
        public long Size { get; set; }
    }

    /// <summary>
    ///   Manages the raw <see cref="Block">IPFS blocks</see>.
    /// </summary>
    /// <remarks>
    ///   An IPFS Block is a byte sequence that represents an IPFS Object 
    ///   (i.e. serialized byte buffers). It is useful to talk about them as "blocks" in Bitswap 
    ///   and other things that do not care about what is being stored. 
    ///   <para>
    ///   It is also possible to store arbitrary stuff using ipfs block put/get as the API 
    ///   does not check for proper IPFS Object formatting.
    ///   </para>
    ///   <note>
    ///   This may be very good or bad, we haven't decided yet 😄
    ///   </note>
    ///   <para>
    ///   This API is accessed via the <see cref="IpfsClient.Block"/> property.
    ///   </para>
    /// </remarks>
    /// <seealso href="https://github.com/ipfs/interface-ipfs-core/tree/master/API/block">Block API</seealso>
    public class BlockApi
    {
        IpfsClient ipfs;

        internal BlockApi(IpfsClient ipfs)
        {
            this.ipfs = ipfs;
        }

        /// <summary>
        ///   Gets a raw <see cref="Block">IPFS block</see>.
        /// </summary>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
        /// </param>
        /// <param name="hash">
        ///   The <see cref="MultiHash"/> of the block.
        /// </param>
        public async Task<Block> GetAsync(MultiHash hash, CancellationToken cancel = default(CancellationToken)) // TODO CID support
        {
            var data = await ipfs.DownloadBytesAsync("block/get", cancel, hash.ToString());
            return new Block
            {
                DataBytes = data,
                Hash = hash
            };
        }

        /// <summary>
        ///   Stores a byte array as a raw <see cref="Block">IPFS block</see>.
        /// </summary>
        /// <param name="data">
        ///   The byte array to send to the IPFS network.
        /// </param>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
        /// </param>
        public async Task<Block> PutAsync(byte[] data, CancellationToken cancel = default(CancellationToken))
        {
            var json = await ipfs.UploadAsync("block/put", cancel, data);
            var info = JsonConvert.DeserializeObject<BlockInfo>(json);
            return new Block
            {
                DataBytes = data,
                Hash = info.Key
            };
        }

        /// <summary>
        ///   Stores a raw <see cref="Block">IPFS block</see>.
        /// </summary>
        /// <param name="block">
        ///   The <seealso cref="Block"/> to send to the IPFS network.
        /// </param>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
        /// </param>
        public Task<Block> PutAsync(Block block, CancellationToken cancel = default(CancellationToken))
        {
            return PutAsync(block.DataBytes, cancel);
        }

        /// <summary>
        ///   Information on a raw <see cref="Block">IPFS block</see>.
        /// </summary>
        /// <param name="hash">
        ///   The <see cref="MultiHash"/> id of the block.
        /// </param>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
        /// </param>
        public Task<BlockInfo> StatAsync(MultiHash hash, CancellationToken cancel = default(CancellationToken))
        {
            return ipfs.DoCommandAsync<BlockInfo>("block/stat", cancel, hash.ToBase58());
        }

        /// <summary>
        ///   Remove a raw <see cref="Block">IPFS block</see>.
        /// </summary>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
        /// </param>
        /// <param name="hash">
        ///   The <see cref="MultiHash"/> id of the block.
        /// </param>
        /// <param name="ignoreNonexistent">
        ///   If <b>true</b> do not raise exception when <paramref name="hash"/> does not
        ///   exist.  Default value is <b>false</b>.
        /// </param>
        /// <returns>
        ///   The awaited Task will return the deleted <paramref name="hash"/> or
        ///   <see cref="string.Empty"/> if the hash does not exist and <paramref name="ignoreNonexistent"/>
        ///   is <b>true</b>.
        /// </returns>
        public async Task<string> RemoveAsync(MultiHash hash, bool ignoreNonexistent = false, CancellationToken cancel = default(CancellationToken)) // TODO CID support
        {
            var json = await ipfs.DoCommandAsync("block/rm", cancel, hash.ToBase58(), "force=" + ignoreNonexistent.ToString().ToLowerInvariant());
            if (json.Length == 0)
                return "";
            var result = JObject.Parse(json);
            var error = (string)result["Error"];
            if (error != null)
                throw new HttpRequestException(error);
            return (string)result["Hash"];
        }
    }

}
