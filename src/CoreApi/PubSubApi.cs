﻿using Common.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        /// <returns>
        ///   A sequence of <see cref="string"/> for each topic.
        /// </returns>
        public async Task<IEnumerable<string>> SubscribedTopicsAsync()
        {
            var json = await ipfs.DoCommandAsync("pubsub/ls");
            var result = JObject.Parse(json);
            var strings = result["Strings"] as JArray;
            if (strings == null) return new string[0];
            return strings.Select(s => (string)s);
        }

        /// <summary>
        ///   Get the peers that are pubsubing with us.
        /// </summary>
        /// <returns>
        ///   A sequence of <see cref="string"/> for each peer ID.
        /// </returns>
        public async Task<IEnumerable<string>> PeersAsync()
        {
            var json = await ipfs.DoCommandAsync("pubsub/peers");
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
        public async Task Publish(string topic, string message)
        {
            var _ = await ipfs.PostCommandAsync("pubsub/pub", topic, "arg=" + message);
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
        /// <returns>
        ///   After the topic listener is register with the IPFS server.
        /// </returns>
        /// <remarks>
        ///   The <paramref name="handler"/> is invoked on a different thread.
        /// </remarks>
        public async Task Subscribe(string topic, Action<PublishedMessage> handler)
        {
            var messageStream = await ipfs.PostDownloadAsync("pubsub/sub", topic);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(() => ProcessMessages(topic, handler, messageStream));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            return;
        }

        void ProcessMessages(string topic, Action<PublishedMessage> handler, Stream stream)
        {
            log.DebugFormat("Start listening for '{0}' messages", topic);
            using (var sr = new StreamReader(stream))
            {
                while (!sr.EndOfStream)
                {
                    var json = sr.ReadLine();
                    if (log.IsDebugEnabled)
                        log.DebugFormat("PubSub message {0}", json);
                    if (json != "{}")
                    {
                        handler(new PublishedMessage(json));
                    }
                }
            }
            log.DebugFormat("Stop listening for '{0}' messages", topic);
        }

    }

}