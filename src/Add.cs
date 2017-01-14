using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace Ipfs.Api
{
    public partial class IpfsClient
    {
        /// <summary>
        ///   Add a file to the interplanetary file system.
        /// </summary>
        /// <param name="path"></param>
        public Task<MerkleNode> AddFileAsync(string path)
        {
            return AddAsync(
                new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read),
                Path.GetFileName(path));
        }

        /// <summary>
        ///   Add a directory and its files to the interplanetary file system.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="recursive"></param>
        public async Task<MerkleNode> AddDirectoryAsync(string path, bool recursive = true)
        {
            path = Path.GetFullPath(path);
            var folder = await Object.NewDirectoryAsync();
            var files = Directory
                .EnumerateFiles(path)
                .Select(AddFileAsync);
            if (recursive)
            {
                var folders = Directory
                    .EnumerateDirectories(path)
                    .Select(dir => AddDirectoryAsync(dir, recursive));
                files = files.Union(folders);
            }
            var nodes = await Task.WhenAll(files);
            var links = nodes.Select(node => node.ToLink());

            // TODO: Use DagNode.AddLinks
            folder = new DagNode(folder.Data, links);

            var directory = await Object.PutAsync(folder);
            return new MerkleNode(directory.Hash, Path.GetFileName(path));
        }

        /// <summary>
        ///   Add some text to the interplanetary file system.
        /// </summary>
        /// <param name="text"></param>
        public Task<MerkleNode> AddTextAsync(string text)
        {
            return AddAsync(new MemoryStream(Encoding.UTF8.GetBytes(text)));
        }

        /// <summary>
        ///   Add a <see cref="Stream"/> to interplanetary file system.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="name"></param>
        public async Task<MerkleNode> AddAsync(Stream stream, string name = "")
        {
            var json = await UploadAsync("add", stream);
            var r = JObject.Parse(json);
            return new MerkleNode((string)r["Hash"])
            {
                Name = name,
                IpfsClient = this
            };
        }
    }
}
