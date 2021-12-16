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
using Multiformats.Base;

namespace Ipfs.Http
{

    class PubSubApi : IPubSubApi
    {
        static ILog log = LogManager.GetLogger<PubSubApi>();

        IpfsClient ipfs;

        internal PubSubApi(IpfsClient ipfs)
        {
            this.ipfs = ipfs;
        }

        public async Task<IEnumerable<string>> SubscribedTopicsAsync(CancellationToken cancel = default(CancellationToken))
        {
            var json = await ipfs.DoCommandAsync("pubsub/ls", cancel);
            var result = JObject.Parse(json);
            var strings = result["Strings"] as JArray;
            if (strings == null) return new string[0];
            return strings.Select(s => (string)s);
        }

        public async Task<IEnumerable<Peer>> PeersAsync(string topic = null, CancellationToken cancel = default(CancellationToken))
        {
            var json = await ipfs.DoCommandAsync("pubsub/peers", cancel, topic);
            var result = JObject.Parse(json);
            var strings = result["Strings"] as JArray;
            if (strings == null) return new Peer[0];
            return strings.Select(s => new Peer { Id = (string)s } );
        }

        public Task PublishAsync(string topic, byte[] message, CancellationToken cancel = default(CancellationToken))
        {
            var url = new StringBuilder();
            url.Append("/api/v0/pubsub/pub");
            url.Append("?arg=u");
            url.Append(Multibase.Encode(MultibaseEncoding.Base64Url, Encoding.UTF8.GetBytes(topic)));

            return ipfs.DoCommandAsync(new Uri(ipfs.ApiUri, url.ToString()), message, cancel);
        }

        public Task PublishAsync(string topic, Stream message, CancellationToken cancel = default(CancellationToken))
        {
            var url = new StringBuilder();
            url.Append("/api/v0/pubsub/pub");
            url.Append("?arg=u");
            url.Append(Multibase.Encode(MultibaseEncoding.Base64Url, Encoding.UTF8.GetBytes(topic)));

            return ipfs.DoCommandAsync(new Uri(ipfs.ApiUri, url.ToString()), message, cancel);
        }

        public Task PublishAsync(string topic, string message, CancellationToken cancel = default(CancellationToken))
        {
            var url = new StringBuilder();
            url.Append("/api/v0/pubsub/pub");
            url.Append("?arg=u");
            url.Append(Multibase.Encode(MultibaseEncoding.Base64Url, Encoding.UTF8.GetBytes(topic)));

            return ipfs.DoCommandAsync(new Uri(ipfs.ApiUri, url.ToString()), message, cancel);
        }

        public async Task SubscribeAsync(string topic, Action<IPublishedMessage> handler, CancellationToken cancellationToken)
        {
            var messageStream = await ipfs.PostDownloadAsync("pubsub/sub", cancellationToken, $"u{Multibase.Encode(MultibaseEncoding.Base64Url, Encoding.UTF8.GetBytes(topic))}");
            var sr = new StreamReader(messageStream);

            _ = Task.Run(() => ProcessMessages(topic, handler, sr, cancellationToken));

            return;
        }

        void ProcessMessages(string topic, Action<PublishedMessage> handler, StreamReader sr, CancellationToken ct)
        {
            log.DebugFormat($"Start listening for '{topic}' messages");
                     
            // .Net needs a ReadLine(CancellationToken)
            // As a work-around, we register a function to close the stream
            ct.Register(() => sr.Dispose());
            try
            {
                while (!sr.EndOfStream && !ct.IsCancellationRequested)
                {
                    var json = sr.ReadLine();
                    if (json == null)
                        break;

                    if (log.IsDebugEnabled)
                        log.DebugFormat("PubSub message {0}", json);

                    // go-ipfs 0.4.13 and earlier always send empty JSON
                    // as the first response.
                    if (json == "{}")
                        continue;

                    if (!ct.IsCancellationRequested)
                    {
                        handler(new PublishedMessage(json));
                    }
                }
            }
            catch (Exception e)
            {
                // Do not report errors when cancelled.
                if (!ct.IsCancellationRequested)
                    log.Error(e);
            }
            finally
            {
                sr.Dispose();
            }

            log.DebugFormat("Stop listening for '{0}' messages", topic);
        }

    }

}
