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

namespace Ipfs.Api
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

        public async Task PublishAsync(string topic, string message, CancellationToken cancel = default(CancellationToken))
        {
            var _ = await ipfs.DoCommandAsync("pubsub/pub", cancel, topic, "arg=" + message);
            return;
        }

        public async Task SubscribeAsync(string topic, Action<IPublishedMessage> handler, CancellationToken cancellationToken)
        {
            var messageStream = await ipfs.PostDownloadAsync("pubsub/sub", cancellationToken, topic);
            var sr = new StreamReader(messageStream);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(() => ProcessMessages(topic, handler, sr, cancellationToken));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            return;
        }

        void ProcessMessages(string topic, Action<PublishedMessage> handler, StreamReader sr, CancellationToken ct)
        {
            log.DebugFormat("Start listening for '{0}' messages", topic);
                     
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
