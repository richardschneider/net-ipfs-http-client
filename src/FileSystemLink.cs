
namespace Ipfs.Http
{
    /// <summary>
    ///     A link to another file system node in IPFS.
    /// </summary>
    public class FileSystemLink : IFileSystemLink
    {
        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public Cid Id { get; set; }

        /// <inheritdoc />
        public long Size { get; set; }

        /// <inheritdoc />
        public bool IsDirectory { get; set; }
    }
}
