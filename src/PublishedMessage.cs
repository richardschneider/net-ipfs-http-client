using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Runtime.Serialization;

namespace Ipfs.Http
{
    /// <summary>
    ///   A published message.
    /// </summary>
    /// <remarks>
    ///   The <see cref="PubSubApi"/> is used to publish and subsribe to a message.
    /// </remarks>
    [DataContract]
    public class PublishedMessage : IPublishedMessage
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
            var topics = (JArray) (o["topicIDs"]);
            this.Topics = topics.Select(t => (string)t);
        }

        /// <inheritdoc />
        [DataMember]
        public Peer Sender { get; private set; }

        /// <inheritdoc />
        [DataMember]
        public IEnumerable<string> Topics { get; private set; }

        /// <inheritdoc />
        [DataMember]
        public byte[] SequenceNumber { get; private set; }

        /// <inheritdoc />
        [DataMember]
        public byte[] DataBytes { get; private set; }

        /// <inheritdoc />
        public Stream DataStream
        {
            get
            {
                return new MemoryStream(DataBytes, false);
            }
        }

        /// <inheritdoc />
        [DataMember]
        public long Size
        {
            get { return DataBytes.Length;  }
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

        /// <summary>>
        ///   NOT SUPPORTED.
        /// </summary>
        /// <exception cref="NotSupportedException">
        ///   A published message does not have a content id.
        /// </exception>
        public Cid Id => throw new NotSupportedException();
    }
}
