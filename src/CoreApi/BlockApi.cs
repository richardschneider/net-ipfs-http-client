using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        ///   The unique ID of the block.
        /// </summary>
        /// <value>
        ///   Typically, the string representation of a <see cref="MultiHash"/>.
        /// </value>
        public string Key { get; set; }

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
        /// <param name="hash">
        ///   The <see cref="string"/> representation of a base58 encoded <see cref="Ipfs.MultiHash"/>.
        /// </param>
        public async Task<Block> GetAsync(string hash) // TODO CID support
        {
            var data = await ipfs.DownloadBytesAsync("block/get", hash);
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
        public async Task<Block> PutAsync(byte[] data)
        {
            var json = await ipfs.UploadAsync("block/put", data);
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
        public Task<Block> PutAsync(Block block)
        {
            return PutAsync(block.DataBytes);
        }

        /// <summary>
        ///   Information on a raw <see cref="Block">IPFS block</see>.
        /// </summary>
        /// <param name="hash">
        ///   The <see cref="string"/> representation of a base58 encoded <see cref="Ipfs.MultiHash"/>.
        /// </param>
        public Task<BlockInfo> StatAsync(string hash)
        {
            return ipfs.DoCommandAsync<BlockInfo>("block/stat", hash);
        }
    }

}
