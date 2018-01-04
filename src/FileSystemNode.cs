using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ipfs.Api
{
    /// <inheritdoc />
    public class FileSystemNode : IMerkleNode<FileSystemLink>
    {
        IpfsClient ipfsClient;
        IEnumerable<FileSystemLink> links;
        long? size;
        bool? isDirectory;

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
                return IpfsClient.FileSystem.ReadFileAsync(Hash.ToBase58()).Result;
            }
        }
        /// <inheritdoc />
        public MultiHash Hash { get; set; }

        /// <inheritdoc />
        public IEnumerable<FileSystemLink> Links {
            get
            {
                if (links == null) GetInfo();
                return links;
            }
            set
            {
                links = value;
            }
        }

        /// <summary>
        ///   Size of the file contents.
        /// </summary>
        /// <value>
        ///   This is the size of the file not the raw encoded contents
        ///   of the block.
        /// </value>
        public long Size
        {
            get
            {
                if (!size.HasValue) GetInfo();
                return size.Value;
            }
            set
            {
                size = value;
            }
        }

        /// <summary>
        ///   Determines if the link is a directory (folder).
        /// </summary>
        /// <value>
        ///   <b>true</b> if the link is a directory; Otherwise <b>false</b>,
        ///   the link is some type of a file.
        /// </value>
        public bool IsDirectory
        {
            get
            {
                if (!isDirectory.HasValue) GetInfo();
                return isDirectory.Value;
            }
            set
            {
                isDirectory = value;
            }
        }

        /// <summary>
        ///   The file name of the IPFS node.
        /// </summary>
        public string Name { get; set; }

        /// <inheritdoc />
        public FileSystemLink ToLink(string name = null)
        {
            return new FileSystemLink
            {
                Name = name ?? Name,
                Hash = Hash,
                Size = Size,
                IsDirectory = IsDirectory
            };
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

        void GetInfo()
        {
            var node = IpfsClient.FileSystem.ListFileAsync(Hash.ToBase58()).Result;
            this.IsDirectory = node.IsDirectory;
            this.Links = node.Links;
            this.Size = node.Size;
        }

    }
}
