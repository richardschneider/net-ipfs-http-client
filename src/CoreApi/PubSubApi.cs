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

namespace Ipfs.Api
{

    /// <summary>
    ///   Allows you to publish messages to a given topic, and also to
    ///   subscribe to new messages on a given topic.
    /// </summary>
    /// <remarks>
    ///   This API is accessed via the <see cref="IpfsClient.PubSub"/> property.
    ///   <para>
    ///   This is an experimental feature. It is not intended in its current state
    ///   to be used in a production environment.
    ///   </para>
    ///   <para>
    ///   To use, the daemon must be run with '--enable-pubsub-experiment'.
    ///   </para>
    /// </remarks>
    /// <seealso href="https://github.com/ipfs/interface-ipfs-core/blob/master/SPEC/PUBSUB.md">PUBSUB API</seealso>
    public class PubSubApi
    {
        static ILog log = LogManager.GetLogger<PubSubApi>();

        IpfsClient ipfs;

        internal PubSubApi(IpfsClient ipfs)
        {
            this.ipfs = ipfs;
        }

        /// <summary>
        ///   Get the subscribed topics.
        /// </summary>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
        /// </param>
        /// <returns>
        ///   A sequence of <see cref="string"/> for each topic.
        /// </returns>
        public async Task<IEnumerable<string>> SubscribedTopicsAsync(CancellationToken cancel = default(CancellationToken))
        {
            var json = await ipfs.DoCommandAsync("pubsub/ls", cancel);
            var result = JObject.Parse(json);
            var strings = result["Strings"] as JArray;
            if (strings == null) return new string[0];
            return strings.Select(s => (string)s);
        }

        /// <summary>
        ///   Get the peers that are pubsubing with us.
        /// </summary>
        /// <param name="topic">
        ///   When specified, only peers pubsubing on the topic are returned.
        /// </param>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
        /// </param>
        /// <returns>
        ///   A sequence of <see cref="string"/> for each peer ID.
        /// </returns>
        public async Task<IEnumerable<string>> PeersAsync(string topic = null, CancellationToken cancel = default(CancellationToken))
        {
            var json = await ipfs.DoCommandAsync("pubsub/peers", cancel, topic);
            var result = JObject.Parse(json);
            var strings = result["Strings"] as JArray;
            if (strings == null) return new string[0];
            return strings.Select(s => (string)s);
        }

        /// <summary>
        ///   Publish a message to a given topic.
        /// </summary>
        /// <param name="topic">
        ///   The topic name.
        /// </param>
        /// <param name="message">
        ///   The message to publish.
        /// </param>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
        /// </param>
        public async Task Publish(string topic, string message, CancellationToken cancel = default(CancellationToken))
        {
            var _ = await ipfs.DoCommandAsync("pubsub/pub", cancel, topic, "arg=" + message);
            return;
        }

        /// <summary>
        ///   Subscribe to messages on a given topic.
        /// </summary>
        /// <param name="topic">
        ///   The topic name.
        /// </param>
        /// <param name="handler">
        ///   The action to perform when a <see cref="PublishedMessage"/> is received.
        /// </param>
        /// <param name="cancellationToken">
        ///   Is used to stop the topic listener.  When cancelled, the <see cref="OperationCanceledException"/>
        ///   is <b>NOT</b> raised.
        /// </param>
        /// <returns>
        ///   After the topic listener is register with the IPFS server.
        /// </returns>
        /// <remarks>
        ///   The <paramref name="handler"/> is invoked on the topic listener thread.
        /// </remarks>
        public async Task Subscribe(string topic, Action<PublishedMessage> handler, CancellationToken cancellationToken)
        {
            var messageStream = await ipfs.PostDownloadAsync("pubsub/sub", cancellationToken, topic);
            var sr = new StreamReader(messageStream);
            var response = sr.ReadLine();
            if (log.IsDebugEnabled)
                log.Debug("RSP " + response);

            // First line is always an empty JSON object,
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
                    if (json != "{}" && !ct.IsCancellationRequested)
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
