using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace Ipfs.Api
{
    /// <summary>
    ///   A list of trusted peers.
    /// </summary>
    /// <remarks>
    ///   This is the list of peers that are initially trusted by IPFS. Its equivalent to the
    ///   <see cref="https://ipfs.io/ipfs/QmTkzDwWqPbnAh5YiV5VwcTLnGdwSNsNTn2aDxdXBFca7D/example#/ipfs/QmThrNbvLj7afQZhxH72m5Nn1qiVn3eMKWFYV49Zp2mv9B/bootstrap/readme.md"/>ipfs bootstrap</see> command.
    /// </remarks>
    /// <returns>
    ///   A series of <see cref="MultiAddress"/>.  Each address ends with an IPNS node id, for
    ///   example "/ip4/104.131.131.82/tcp/4001/ipfs/QmaCpDMGvV2BGHeYERUEnRQAwe3N8SzbUtfsmvsqQLuvuJ".
    /// </returns>
    public class TruestedPeerCollection : ICollection<MultiAddress>
    {
        class BootstrapListResponse
        {
            public MultiAddress[] Peers { get; set; }
        }

        IpfsClient ipfs;
        MultiAddress[] peers;

        internal TruestedPeerCollection(IpfsClient ipfs)
        {
            this.ipfs = ipfs;
        }

        /// <inheritdoc />
        public void Add(MultiAddress address)
        {
            if (address == null)
                throw new ArgumentNullException();

            ipfs.DoCommand("bootstrap/add", address.ToString());
            peers = null;
        }

        /// <inheritdoc />
        public void Clear()
        {
            ipfs.DoCommand("bootstrap/rm", null, "all=true");
            peers = null;
        }

        /// <inheritdoc />
        public bool Contains(MultiAddress item)
        {
            Fetch();
            return peers.Contains(item);
        }

        /// <inheritdoc />
        public void CopyTo(MultiAddress[] array, int index)
        {
            Fetch();
            peers.CopyTo(array, index);
        }

        /// <inheritdoc />
        public int Count
        {
            get
            {
                if (peers == null)
                    Fetch();
                return peers.Count();
            }
        }

        /// <inheritdoc />
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <inheritdoc />
        public bool Remove(MultiAddress address)
        {
            if (address == null)
                throw new ArgumentNullException();

            ipfs.DoCommand("bootstrap/rm", address.ToString());
            peers = null;
            return true;
        }

        /// <inheritdoc />
        public IEnumerator<MultiAddress> GetEnumerator()
        {
            Fetch();
            return ((IEnumerable<MultiAddress>) peers).GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            Fetch();
            return peers.GetEnumerator();
        }

        void Fetch()
        {
            peers = ipfs.DoCommand<BootstrapListResponse>("bootstrap/list").Peers;
        }
    }
}
