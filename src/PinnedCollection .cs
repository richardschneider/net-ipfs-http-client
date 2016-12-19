using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace Ipfs.Api
{
    /// <summary>
    ///   A list of pinned object.
    /// </summary>
    /// <remarks>
    ///   This is the list of objects that are permanently stored on the local host.
    ///   <see cref="https://ipfs.io/ipfs/QmTkzDwWqPbnAh5YiV5VwcTLnGdwSNsNTn2aDxdXBFca7D/example#/ipfs/QmThrNbvLj7afQZhxH72m5Nn1qiVn3eMKWFYV49Zp2mv9B/pin/readme.md"/>ipfs pin</see> command.
    /// </remarks>
    /// <returns>
    ///   A series of <see cref="PinnedObject"/>.
    ///   </returns>
    public class PinnedCollection : ICollection<PinnedObject>
    {
        IpfsClient ipfs;
        PinnedObject[] pins;

        internal PinnedCollection(IpfsClient ipfs)
        {
            this.ipfs = ipfs;
        }

        PinnedObject[] Pins
        {
            get
            {
                if (pins == null)
                {
                    Refresh();
                }
                return pins;
            }
        }

        /// <inheritdoc />
        public void Add(PinnedObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException();

            throw new NotImplementedException();
        }

        /// <summary>
        ///    Remove all the trusted peers.
        /// </summary>
        /// <remarks>
        ///    Equivalent to <c>ipfs bootstrap rm --all</c>.
        /// </remarks>
        public void Clear()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool Contains(PinnedObject item)
        {
            return Pins.Contains(item);
        }

        /// <inheritdoc />
        public void CopyTo(PinnedObject[] array, int index)
        {
            Pins.CopyTo(array, index);
        }

        /// <inheritdoc />
        public int Count
        {
            get
            {
                return Pins.Length;
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
        public bool Remove(PinnedObject peer)
        {
            if (peer == null)
                throw new ArgumentNullException();

            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IEnumerator<PinnedObject> GetEnumerator()
        {
            return ((IEnumerable<PinnedObject>) Pins).GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Pins.GetEnumerator();
        }

        /// <summary>
        ///   Ask IPFS for the pinned the objects.
        /// </summary>
        public void Refresh()
        {
            var json = ipfs.DoCommand("pin/ls");
            var keys = (JObject) (JObject.Parse(json)["Keys"]);
            pins = keys
                .Properties()
                .Select(p => new PinnedObject
                {
                    Id = p.Name,
                    Mode = (PinMode)Enum.Parse(typeof(PinMode), (string) keys[p.Name]["Type"], true)
                })
                .ToArray();
         }
    }
}
