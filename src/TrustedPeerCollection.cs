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
    ///   <c>ipfs bootstrap list</c> command.
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

        #region ICollection<MultiAddress> Members

        /// <inheritdoc />
        public void Add(MultiAddress item)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Clear()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool Contains(MultiAddress item)
        {
            if (peers == null)
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
        public bool Remove(MultiAddress item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable Members

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

        #endregion

        void Fetch()
        {
            var result = ipfs.Api()
                .DownloadString(ipfs.BuildCommand("bootstrap/list"));
            peers = JsonConvert
                .DeserializeObject<BootstrapListResponse>(result)
                .Peers;
        }
    }
}
