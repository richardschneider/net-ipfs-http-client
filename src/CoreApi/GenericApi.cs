using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using Ipfs.CoreApi;
using Newtonsoft.Json.Linq;

namespace Ipfs.Http
{
    public partial class IpfsClient : IGenericApi
    {
        const double TicksPerNanosecond = (double)TimeSpan.TicksPerMillisecond * 0.000001;

        /// <inheritdoc />
        public Task<Peer> IdAsync(MultiHash peer = null, CancellationToken cancel = default(CancellationToken))
        {
            return DoCommandAsync<Peer>("id", cancel, peer?.ToString());
        }

        /// <inheritdoc />
        public async Task<IEnumerable<PingResult>> PingAsync(MultiHash peer, int count = 10, CancellationToken cancel = default(CancellationToken))
        {
            var stream = await PostDownloadAsync("ping", cancel,
                peer.ToString(),
                $"count={count.ToString(CultureInfo.InvariantCulture)}");
            return PingResultFromStream(stream);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<PingResult>> PingAsync(MultiAddress address, int count = 10, CancellationToken cancel = default(CancellationToken))
        {
            var stream = await PostDownloadAsync("ping", cancel,
                address.ToString(),
                $"count={count.ToString(CultureInfo.InvariantCulture)}");
            return PingResultFromStream(stream);
        }

        IEnumerable<PingResult> PingResultFromStream(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                while (!sr.EndOfStream)
                {
                    var json = sr.ReadLine();
                    if (log.IsDebugEnabled)
                        log.DebugFormat("RSP {0}", json);

                    var r = JObject.Parse(json);
                    yield return new PingResult
                    {
                        Success = (bool)r["Success"],
                        Text = (string)r["Text"],
                        Time = TimeSpan.FromTicks((long)((long)r["Time"] * TicksPerNanosecond))
                    };
                }
            }
        }

        /// <inheritdoc />
        public async Task<string> ResolveAsync(string name, bool recursive = true, CancellationToken cancel = default(CancellationToken))
        {
            var json = await DoCommandAsync("resolve", cancel,
                name,
                $"recursive={recursive.ToString().ToLowerInvariant()}");
            var path = (string)(JObject.Parse(json)["Path"]);
            return path;
        }

        /// <inheritdoc />
        public async Task ShutdownAsync()
        {
            await DoCommandAsync("shutdown", default(CancellationToken));
        }

        /// <inheritdoc />
        public Task<Dictionary<string, string>> VersionAsync(CancellationToken cancel = default(CancellationToken))
        {
            return DoCommandAsync<Dictionary<string, string>>("version", cancel);
        }

    }
}
