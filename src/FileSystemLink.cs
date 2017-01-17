
namespace Ipfs.Api
{
    /// <summary>
    ///     A link to another file system node in IPFS.
    /// </summary>
    public class FileSystemLink : IMerkleLink
    {
        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public string Hash { get; set; }

        /// <inheritdoc />
        public long Size { get; set; }

        /// <summary>
        ///   Determines if the link is a directory (folder).
        /// </summary>
        /// <value>
        ///   <b>true</b> if the link is a directory; Otherwise <b>false</b>,
        ///   the link is some type of a file.
        /// </value>
        public bool IsDirectory { get; set; }
    }
}
