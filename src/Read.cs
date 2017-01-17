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
        ///   Reads the content of an existing IPFS file as text.
        /// </summary>
        /// <param name="path">
        ///   A path to an existing file, such as "QmXarR6rgkQ2fDSHjSY5nM2kuCXKYGViky5nohtwgF65Ec/about"
        ///   or "QmZTR5bcpQD7cFgTorqxZDYaew1Wqgfbd2ud9QqGPAkK2V"
        /// </param>
        /// <returns></returns>
        public async Task<String> ReadAllTextAsync(string path)
        {
            using (var data = await ReadFileAsync(path))
            using (var text = new StreamReader(data))
            {
                return await text.ReadToEndAsync();
            }
        }

        /// <summary>
        ///   Opens an existing IPFS file for reading.
        /// </summary>
        /// <param name="path">
        ///   A path to an existing file, such as "QmXarR6rgkQ2fDSHjSY5nM2kuCXKYGViky5nohtwgF65Ec/about"
        ///   or "QmZTR5bcpQD7cFgTorqxZDYaew1Wqgfbd2ud9QqGPAkK2V"
        /// </param>
        /// <returns>
        ///   A <see cref="Stream"/> to the file contents.
        /// </returns>
        public Task<Stream> ReadFileAsync(string path)
        {
            return DownloadAsync("cat", path);
        }

        /// <summary>
        ///   Get information about the file or directory.
        /// </summary>
        /// <param name="path">
        ///   A path to an existing file or directory, such as "QmXarR6rgkQ2fDSHjSY5nM2kuCXKYGViky5nohtwgF65Ec/about"
        ///   or "QmZTR5bcpQD7cFgTorqxZDYaew1Wqgfbd2ud9QqGPAkK2V"
        /// </param>
        /// <returns></returns>
        public async Task<FileSystemNode> ListFileAsync(string path)
        {
            var json = await DoCommandAsync("file/ls", path);
            var r = JObject.Parse(json);
            var hash = (string)r["Arguments"][path];
            var o = (JObject)r["Objects"][hash];
            var node = new FileSystemNode()
            {
                Hash = (string)o["Hash"],
                Size = (long)o["Size"],
                IsDirectory = (string)o["Type"] == "Directory",
                Links = new FileSystemLink[0]
            };
            var links = o["Links"] as JArray;
            if (links != null)
            {
                node.Links = links
                    .Select(l => new FileSystemLink()
                    {
                        Name = (string)l["Name"],
                        Hash = (string)l["Hash"],
                        Size = (long)l["Size"],
                        IsDirectory = (string)l["Type"] == "Directory",
                    })
                    .ToArray();
            }

            return node;
        }
    }
}
