using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

// TODO: GetRawData()
// TODO: GetJsonData()

namespace Ipfs.Api
{
    /// <summary>
    ///   The IPFS MerkleDag is the datastructure at the heart of IPFS. It is an acyclic directed graph whose edges are hashes.
    /// </summary>
    /// <remarks>
    ///   Initially an <b>MerkleNode</b> is just constructed with its MultiHash.  Its other properties are lazily loaded.
    /// </remarks>
    public class MerkleNode
    {
        bool hasObjectStats;
        bool hasBlockStats;
        long blockSize;
        long cumulativeSize;
        long dataSize;
        long linksSize;
        long linksCount;
        string name;
        MerkleNode[] links;

        /// <summary>
        ///   Creates a new instance of the <see cref="MerkleNode"/> with the specified
        ///   <see cref="Hash">multihash</see> and optional <see cref="Name"/>.
        /// </summary>
        /// <param name="hash">
        ///   The Base58 hash of the node or "/ipfs/hash".
        /// </param>
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
        ///   The multihash (Base58 hash) for the node.
        /// </summary>
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
        ///   Size of the raw, encoded code.
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

        public long LinksCount
        {
            get
            {
                GetObjectStats();
                return linksCount;
            }
        }

        public IEnumerable<MerkleNode> Links
        {
            get
            {
                if (links == null)
                {
                    if (linksCount == 0)
                    {
                        links = new MerkleNode[0];
                    }
                    else
                    {
                        var result = new IpfsClient().DoCommand<dynamic>("object/links", Hash);
                        links = ((JArray)result.Links)
                            .Select(l => new MerkleNode((string)l["Hash"], (string)l["Name"])).ToArray();
                    }
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

            var stats = new IpfsClient().DoCommand<dynamic>("object/stat", Hash);
            if (stats.Hash != Hash)
                throw new IpfsException("Did not receive object/stat for the request merkle node.");
            blockSize = stats.BlockSize;
            cumulativeSize = stats.CumulativeSize;
            dataSize = stats.DataSize;
            linksSize = stats.LinksSize;
            linksCount = stats.NumLinks;

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

            var stats = new IpfsClient().DoCommand<dynamic>("block/stat", Hash);
            if (stats.Key != Hash)
                throw new IpfsException("Did not receive block/stat for the request merkle node.");
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
            return (that == null)
                ? false
                : this.Hash == that.Hash;
        }

        /// <inheritdoc />
        public bool Equals(MerkleNode that)
        {
            return this.Hash == that.Hash;
        }

        public static bool operator ==(MerkleNode a, MerkleNode b)
        {
            if (object.ReferenceEquals(a, b)) return true;
            if (object.ReferenceEquals(a, null)) return false;
            if (object.ReferenceEquals(b, null)) return false;

            return a.Equals(b);
        }

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

        static public implicit operator MerkleNode(string hash)
        {
            return new MerkleNode(hash);
        }

    }
}
