using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Ipfs.Api
{
    /// <summary>
    ///   The IPFS <see href="https://github.com/ipfs/specs/tree/master/merkledag">MerkleDag</see> is the datastructure at the heart of IPFS. It is an acyclic directed graph whose edges are hashes.
    /// </summary>
    /// <remarks>
    ///   Initially an <b>MerkleNode</b> is just constructed with its MultiHash.  Its other properties are lazily loaded.
    /// </remarks>
    public class MerkleNode : IMerkleNode<IMerkleLink>, IEquatable<MerkleNode>
    {
        bool hasObjectStats;
        bool hasBlockStats;
        long blockSize;
        long cumulativeSize;
        long dataSize;
        long linksSize;
        long linksCount;
        string name;
        IEnumerable<IMerkleLink> links;
        IpfsClient ipfsClient;

        /// <summary>
        ///   Creates a new instance of the <see cref="MerkleNode"/> with the specified
        ///   <see cref="Hash">multihash</see> and optional <see cref="Name"/>.
        /// </summary>
        /// <param name="hash">
        ///   The Base58 hash of the node or "/ipfs/hash".
        /// </param>
        /// <param name="name"></param>
        public MerkleNode(string hash, string name = null)
        {
            if (string.IsNullOrWhiteSpace(hash))
                throw new ArgumentNullException("hash");

            if (hash.StartsWith("/ipfs/"))
                hash = hash.Substring(6);

            Hash = hash;
            Name = name;
        }

        /// <summary>
        ///   Creates a new instance of the <see cref="MerkleNode"/> from the
        ///   <see cref="IMerkleLink"/>.
        /// </summary>
        /// <param name="link">The link to a node.</param>
        public MerkleNode(IMerkleLink link)
        {
            Hash = link.Hash;
            Name = link.Name;
            blockSize = link.Size;
            hasBlockStats = true;
        }

        internal IpfsClient IpfsClient
        {
            get
            {
                if (ipfsClient == null)
                {
                    lock (this)
                    {
                        ipfsClient = new IpfsClient();
                    }
                }
                return ipfsClient;
            }
            set
            {
                ipfsClient = value;
            }
        }

        /// <inheritdoc />
        public string Hash { get; private set; }

        /// <summary>
        ///   The name for the node.  If unknown it is "" (not null).
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value == null ? string.Empty : value; }
        }

        /// <summary>
        ///   Size of the raw, encoded node.
        /// </summary>
        public long BlockSize
        {
            get
            {
                GetBlockStats();
                return blockSize;
            }
        }

        /// <summary>
        ///   Size of the Links segment.
        /// </summary>
        public long LinksSize
        {
            get
            {
                GetObjectStats();
                return linksSize;
            }
        }

        /// <summary>
        ///  TODO
        /// </summary>
        public long LinksCount
        {
            get
            {
                GetObjectStats();
                return linksCount;
            }
        }

        /// <inheritdoc />
        public IEnumerable<IMerkleLink> Links
        {
            get
            {
                if (links == null)
                {
                    links = IpfsClient.Object.LinksAsync(Hash).Result;
                }

                return links;
            }
        }

        /// <summary>
        ///   Size of the Data segment.
        /// </summary>
        public long DataSize
        {
            get
            {
                GetObjectStats();
                return dataSize;
            }
        }

        /// <summary>
        ///   Cumulative size of object and its references.
        /// </summary>
        public long CumulativeSize
        {
            get
            {
                GetObjectStats();
                return cumulativeSize;
            }
        }

        /// <inheritdoc />
        public byte[] DataBytes
        {
            get
            {
                using (var stream = DataStream)
                using (var data = new MemoryStream())
                {
                    stream.CopyTo(data);
                    return data.ToArray();
                }
            }
        }

        /// <inheritdoc />
        public Stream DataStream
        {
            get
            {
                return IpfsClient.Download("block/get", Hash);
            }
        }

        /// <inheritdoc />
        public IMerkleLink ToLink(string name = null)
        {
            return new DagLink(name != null ? name : Name, Hash, BlockSize);
        }

        /// <summary>
        ///   Get object statistics about the node, <c>ipfs object stat <i>hash</i></c>
        /// </summary>
        /// <remarks>
        ///   The object stats include the block stats.
        /// </remarks>
        void GetObjectStats()
        {
            if (hasObjectStats)
                return;

            var stats = IpfsClient.Object.StatAsync(Hash).Result;
            blockSize = stats.BlockSize;
            cumulativeSize = stats.CumulativeSize;
            dataSize = stats.DataSize;
            linksSize = stats.LinksSize;
            linksCount = stats.NumLinks;
            if (linksCount == 0)
                links = new DagLink[0];

            hasObjectStats = true;
            hasBlockStats = true;
        }

        /// <summary>
        ///   Get block statistics about the node, <c>ipfs block stat <i>key</i></c>
        /// </summary>
        /// <remarks>
        ///   The object stats include the block stats.
        /// </remarks>
        void GetBlockStats()
        {
            if (hasBlockStats)
                return;

            var stats = IpfsClient.Block.StatAsync(Hash).Result;
            blockSize = stats.Size;

            hasBlockStats = true;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Hash.GetHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var that = obj as MerkleNode;
            return that != null && this.Hash == that.Hash;
        }

        /// <inheritdoc />
        public bool Equals(MerkleNode that)
        {
            return that != null && this.Hash == that.Hash;
        }

        /// <summary>
        ///  TODO
        /// </summary>
        public static bool operator ==(MerkleNode a, MerkleNode b)
        {
            if (object.ReferenceEquals(a, b)) return true;
            if (object.ReferenceEquals(a, null)) return false;
            if (object.ReferenceEquals(b, null)) return false;

            return a.Equals(b);
        }

        /// <summary>
        ///  TODO
        /// </summary>
        public static bool operator !=(MerkleNode a, MerkleNode b)
        {
            if (object.ReferenceEquals(a, b)) return false;
            if (object.ReferenceEquals(a, null)) return true;
            if (object.ReferenceEquals(b, null)) return true;

            return !a.Equals(b);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "/ipfs/" + Hash;
        }

        /// <summary>
        ///  TODO
        /// </summary>
        static public implicit operator MerkleNode(string hash)
        {
            return new MerkleNode(hash);
        }

    }
}
