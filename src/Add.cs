using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Ipfs.Api
{
    public partial class IpfsClient
    {
        /// <summary>
        ///   Add a file to the interplanetary file system.
        /// </summary>
        /// <param name="path"></param>
        public MerkleNode AddFile(string path)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///   Add a directory and its files to the interplanetary file system.
        /// </summary>
        /// <param name="path"></param>
        public MerkleNode AddDirectory(string path, bool recursive = true)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///   Add some text to the interplanetary file system.
        /// </summary>
        /// <param name="text"></param>
        public MerkleNode AddText(string text)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///   Add a <see cref="Stream"/> to interplanetary file system.
        /// </summary>
        /// <param name="s"></param>
        public MerkleNode Add(Stream s)
        {
            throw new NotImplementedException();
        }
    }
}
