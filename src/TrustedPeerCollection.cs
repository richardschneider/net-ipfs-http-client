using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Threading;

namespace Ipfs.Http
{
    /// <summary>
    ///   A list of trusted peers.
    /// </summary>
    /// <remarks>
    ///   This is the list of peers that are initially trusted by IPFS. Its equivalent to the
    ///   <see href="https://ipfs.io/ipfs/QmTkzDwWqPbnAh5YiV5VwcTLnGdwSNsNTn2aDxdXBFca7D/example#/ipfs/QmThrNbvLj7afQZhxH72m5Nn1qiVn3eMKWFYV49Zp2mv9B/bootstrap/readme.md">ipfs bootstrap</see> command.
    /// </remarks>
    /// <returns>
    ///   A series of <see cref="MultiAddress"/>.  Each address ends with an IPNS node id, for
    ///   example "/ip4/104.131.131.82/tcp/4001/ipfs/QmaCpDMGvV2BGHeYERUEnRQAwe3N8SzbUtfsmvsqQLuvuJ".
    /// </returns>
    public class TrustedPeerCollection : ICollection<MultiAddress>
    {
        class BootstrapListResponse
        {
            public MultiAddress[] Peers { get; set; }
        }

        IpfsClient ipfs;
        MultiAddress[] peers;

        internal TrustedPeerCollection(IpfsClient ipfs)
        {
            this.ipfs = ipfs;
        }

        /// <inheritdoc />
        public void Add(MultiAddress peer)
        {
            if (peer == null)
                throw new ArgumentNullException();

            ipfs.DoCommandAsync("bootstrap/add", default(CancellationToken), peer.ToString()).Wait();
            peers = null;
        }

        /// <summary>
        ///    Add the default bootstrap nodes to the trusted peers.
        /// </summary>
        /// <remarks>
        ///    Equivalent to <c>ipfs bootstrap add --default</c>.
        /// </remarks>
        public void AddDefaultNodes()
        {
            ipfs.DoCommandAsync("bootstrap/add", default(CancellationToken), null, "default=true").Wait();
            peers = null;
        }

        /// <summary>
        ///    Remove all the trusted peers.
        /// </summary>
        /// <remarks>
        ///    Equivalent to <c>ipfs bootstrap rm --all</c>.
        /// </remarks>
        public void Clear()
        {
            ipfs.DoCommandAsync("bootstrap/rm", default(CancellationToken), null, "all=true").Wait();
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

        /// <summary>
        ///    Remove the trusted peer.
        /// </summary>
        /// <remarks>
        ///    Equivalent to <c>ipfs bootstrap rm <i>peer</i></c>.
        /// </remarks>
        public bool Remove(MultiAddress peer)
        {
            if (peer == null)
                throw new ArgumentNullException();

            ipfs.DoCommandAsync("bootstrap/rm", default(CancellationToken), peer.ToString()).Wait();
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
            peers = ipfs.DoCommandAsync<BootstrapListResponse>("bootstrap/list", default(CancellationToken)).Result.Peers;
        }
    }
}
