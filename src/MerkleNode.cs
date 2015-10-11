using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ipfs.Api
{
    /// <summary>
    ///   The IPFS MerkleDag is the datastructure at the heart of IPFS. It is an acyclic directed graph whose edges are hashes.
    /// </summary>
    public class MerkleNode
    {
        public MerkleNode(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
                throw new ArgumentNullException("hash");

            MultiHash = hash;
        }

        /// <summary>
        ///   The multihash (Base58 hash) for the node.
        /// </summary>
        public string MultiHash { get; private set; }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return MultiHash.GetHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var that = obj as MerkleNode;
            return (that == null)
                ? false
                : this.MultiHash == that.MultiHash;
        }

        /// <inheritdoc />
        public bool Equals(MerkleNode that)
        {
            return this.MultiHash == that.MultiHash;
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
            return MultiHash;
        }

        static public implicit operator MerkleNode(string hash)
        {
            return new MerkleNode(hash);
        }

    }
}
