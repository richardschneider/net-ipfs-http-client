using Common.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ipfs.CoreApi;

namespace Ipfs.Http
{

    class FileSystemApi : IFileSystemApi
    {
        static ILog log = LogManager.GetLogger<FileSystemApi>();

        IpfsClient ipfs;
        Lazy<DagNode> emptyFolder;

        internal FileSystemApi(IpfsClient ipfs)
        {
            this.ipfs = ipfs;
            this.emptyFolder = new Lazy<DagNode>(() => ipfs.Object.NewDirectoryAsync().Result);
        }

        public async Task<IFileSystemNode> AddFileAsync(string path, AddFileOptions options = null, CancellationToken cancel = default(CancellationToken))
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var node = await AddAsync(stream, Path.GetFileName(path), options, cancel);
                return node;
            }
        }

        public Task<IFileSystemNode> AddTextAsync(string text, AddFileOptions options = null, CancellationToken cancel = default(CancellationToken))
        {
            return AddAsync(new MemoryStream(Encoding.UTF8.GetBytes(text), false), "", options, cancel);
        }

        public async Task<IFileSystemNode> AddAsync(Stream stream, string name = "", AddFileOptions options = null, CancellationToken cancel = default(CancellationToken))
        {
            if (options == null)
                options = new AddFileOptions();
            var opts = new List<string>();
            if (!options.Pin)
                opts.Add("pin=false");
            if (options.Wrap)
                opts.Add("wrap-with-directory=true");
            if (options.RawLeaves)
                opts.Add("raw-leaves=true");
            if (options.OnlyHash)
                opts.Add("only-hash=true");
            if (options.Trickle)
                opts.Add("trickle=true");
            if (options.Progress != null)
                opts.Add("progress=true");
            if (options.Hash != MultiHash.DefaultAlgorithmName)
                opts.Add($"hash=${options.Hash}");
            if (options.Encoding != MultiBase.DefaultAlgorithmName)
                opts.Add($"cid-base=${options.Encoding}");
            if (!string.IsNullOrWhiteSpace(options.ProtectionKey))
                opts.Add($"protect={options.ProtectionKey}");
            opts.Add($"chunker=size-{options.ChunkSize}");

            var json = await ipfs.UploadAsync("add", cancel, stream, name, opts.ToArray());

            // The result is a stream of LDJSON objects.
            // See https://github.com/ipfs/go-ipfs/issues/4852
            FileSystemNode fsn = null;
            using (var sr = new StringReader(json))
            using (var jr = new JsonTextReader(sr) { SupportMultipleContent = true })
            {
                while (jr.Read())
                {
                    var r = await JObject.LoadAsync(jr, cancel);

                    // If a progress report.
                    if (r.ContainsKey("Bytes"))
                    {
                        options.Progress?.Report(new TransferProgress
                        {
                            Name = (string)r["Name"],
                            Bytes = (ulong)r["Bytes"]
                        });
                    }

                    // Else must be an added file.
                    else
                    {
                        fsn = new FileSystemNode
                        {
                            Id = (string)r["Hash"],
                            Size = long.Parse((string)r["Size"]),
                            IsDirectory = false,
                            Name = name,
                            IpfsClient = ipfs
                        };
                        if (log.IsDebugEnabled)
                            log.Debug("added " + fsn.Id + " " + fsn.Name);
                    }
                }
            }

            fsn.IsDirectory = options.Wrap;
            return fsn;
        }

        public async Task<IFileSystemNode> AddDirectoryAsync(string path, bool recursive = true, AddFileOptions options = null, CancellationToken cancel = default(CancellationToken))
        {
            if (options == null)
                options = new AddFileOptions();
            options.Wrap = false;

            // Add the files and sub-directories.
            path = Path.GetFullPath(path);
            var files = Directory
                .EnumerateFiles(path)
                .Select(p => AddFileAsync(p, options, cancel));
            if (recursive)
            {
                var folders = Directory
                    .EnumerateDirectories(path)
                    .Select(dir => AddDirectoryAsync(dir, recursive, options, cancel));
                files = files.Union(folders);
            }

            // go-ipfs v0.4.14 sometimes fails when sending lots of 'add file'
            // requests.  It's happy with adding one file at a time.
#if true
            var links = new List<IFileSystemLink>();
            foreach (var file in files)
            {
                var node = await file;
                links.Add(node.ToLink());
            }
#else
            var nodes = await Task.WhenAll(files);
            var links = nodes.Select(node => node.ToLink());
#endif
            // Create the directory with links to the created files and sub-directories
            var folder = emptyFolder.Value.AddLinks(links);
            var directory = await ipfs.Object.PutAsync(folder, cancel);

            if (log.IsDebugEnabled)
                log.Debug("added " + directory.Id + " " + Path.GetFileName(path));
            return new FileSystemNode
            {
                Id = directory.Id,
                Name = Path.GetFileName(path),
                Links = links,
                IsDirectory = true,
                Size = directory.Size,
                IpfsClient = ipfs
            };

        }

        /// <summary>
        ///   Reads the content of an existing IPFS file as text.
        /// </summary>
        /// <param name="path">
        ///   A path to an existing file, such as "QmXarR6rgkQ2fDSHjSY5nM2kuCXKYGViky5nohtwgF65Ec/about"
        ///   or "QmZTR5bcpQD7cFgTorqxZDYaew1Wqgfbd2ud9QqGPAkK2V"
        /// </param>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
        /// </param>
        /// <returns>
        ///   The contents of the <paramref name="path"/> as a <see cref="string"/>.
        /// </returns>
        public async Task<String> ReadAllTextAsync(string path, CancellationToken cancel = default(CancellationToken))
        {
            using (var data = await ReadFileAsync(path, cancel))
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
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
        /// </param>
        /// <returns>
        ///   A <see cref="Stream"/> to the file contents.
        /// </returns>
        public Task<Stream> ReadFileAsync(string path, CancellationToken cancel = default(CancellationToken))
        {
            return ipfs.DownloadAsync("cat", cancel, path);
        }

        public Task<Stream> ReadFileAsync(string path, long offset, long length = 0, CancellationToken cancel = default(CancellationToken))
        {
            // https://github.com/ipfs/go-ipfs/issues/5380
            if (offset > int.MaxValue)
                throw new NotSupportedException("Only int offsets are currently supported.");
            if (length > int.MaxValue)
                throw new NotSupportedException("Only int lengths are currently supported.");

            if (length == 0)
                length = int.MaxValue; // go-ipfs only accepts int lengths
            return ipfs.DownloadAsync("cat", cancel, path, 
                $"offset={offset}",
                $"length={length}");
        }

        /// <summary>
        ///   Get information about the file or directory.
        /// </summary>
        /// <param name="path">
        ///   A path to an existing file or directory, such as "QmXarR6rgkQ2fDSHjSY5nM2kuCXKYGViky5nohtwgF65Ec/about"
        ///   or "QmZTR5bcpQD7cFgTorqxZDYaew1Wqgfbd2ud9QqGPAkK2V"
        /// </param>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
        /// </param>
        /// <returns></returns>
        public async Task<IFileSystemNode> ListFileAsync(string path, CancellationToken cancel = default(CancellationToken))
        {
            var json = await ipfs.DoCommandAsync("file/ls", cancel, path);
            var r = JObject.Parse(json);
            var hash = (string)r["Arguments"][path];
            var o = (JObject)r["Objects"][hash];
            var node = new FileSystemNode()
            {
                Id = (string)o["Hash"],
                Size = (long)o["Size"],
                IsDirectory = (string)o["Type"] == "Directory",
                Links = new FileSystemLink[0],
                IpfsClient = ipfs
            };
            var links = o["Links"] as JArray;
            if (links != null)
            {
                node.Links = links
                    .Select(l => new FileSystemLink()
                    {
                        Name = (string)l["Name"],
                        Id = (string)l["Hash"],
                        Size = (long)l["Size"],
                        IsDirectory = (string)l["Type"] == "Directory",
                    })
                    .ToArray();
            }

            return node;
        }

        public Task<Stream> GetAsync(string path, bool compress = false, CancellationToken cancel = default(CancellationToken))
        {
            return ipfs.DownloadAsync("get", cancel, path, $"compress={compress}");
        }
    }
}
