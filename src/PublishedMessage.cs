using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace Ipfs.Api
{
    /// <summary>
    ///   A published message.
    /// </summary>
    /// <remarks>
    ///   The <see cref="PubSubApi"/> is used to publish and subsribe to a message.
    /// </remarks>
    public class PublishedMessage
    {
        /// <summary>
        ///   Creates a new instance of <see cref="PublishedMessage"/> from the
        ///   specified JSON string.
        /// </summary>
        /// <param name="json">
        ///   The JSON representation of a published message.
        /// </param>
        public PublishedMessage(string json)
        {
            var o = JObject.Parse(json);
            this.Sender = Convert.FromBase64String((string)o["from"]).ToBase58();
            this.SequenceNumber = Convert.FromBase64String((string)o["seqno"]);
            this.DataBytes = Convert.FromBase64String((string)o["data"]);
            this.Topics = ((JArray)o["topicIDs"]).Select(t => (string)t);
        }

        /// <summary>
        ///   The sender of the message.
        /// </summary>
        /// <remarks>
        ///   This is the peer ID of the node that sent the message.
        /// </remarks>
        public string Sender { get; private set; }

        /// <summary>
        ///   The topics of the message.
        /// </summary>
        public IEnumerable<string> Topics { get; private set; }

        /// <summary>
        ///   The sequence number of the message.
        /// </summary>
        public byte[] SequenceNumber { get; private set; }

        /// <summary>
        ///   Contents as a byte array.
        /// </summary>
        /// <value>
        ///   The contents as a sequence of bytes.
        /// </value>
        public byte[] DataBytes { get; private set; }

        /// <summary>
        ///   Contents as a stream of bytes.
        /// </summary>
        /// <value>
        ///   The contents as a stream of bytes.
        /// </value>
        public Stream DataStream
        {
            get
            {
                return new MemoryStream(DataBytes, false);
            }
        }

        /// <summary>
        ///   Contents as a string.
        /// </summary>
        /// <value>
        ///   The contents interpreted as a UTF-8 string.
        /// </value>
        public string DataString
        {
            get
            {
                return Encoding.UTF8.GetString(DataBytes);
            }
        }
    }
}
