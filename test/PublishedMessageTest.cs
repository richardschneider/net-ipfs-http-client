using Ipfs.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.IO;

namespace Ipfs.Api
{
    
    [TestClass]
    public partial class PublishedMessageTest
    {
        const string json = @"{
 ""from"":""EiDzOYdzT4BE42JXwxVM8Q19w6tx30Bp2N3T7tOH/a2nCw=="",
 ""data"":""aGVsbG8gd29ybGQ="",
 ""seqno"":""FPBVj+oTUug="",
 ""topicIDs"":[""net-ipfs-api-test""]
}";

        [TestMethod]
        public void FromJson()
        {
            var msg = new PublishedMessage(json);
            Assert.AreEqual("Qmei6fBYij8gjbetgHLXmoR54iRc9hioPR7dtmBTNG3oWa", msg.Sender);
            Assert.AreEqual("14f0558fea1352e8", msg.SequenceNumber.ToHexString());
            Assert.AreEqual("68656c6c6f20776f726c64", msg.DataBytes.ToHexString());
            Assert.AreEqual("hello world", msg.DataString);
            CollectionAssert.Contains(msg.Topics.ToArray(), "net-ipfs-api-test");

            var data = msg.DataBytes;
            var streamData = new MemoryStream();
            msg.DataStream.CopyTo(streamData);
            CollectionAssert.AreEqual(data, streamData.ToArray());
        }

    }
}
