using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ipfs.Api
{
    /// <summary>
    ///   An IPFS object that is permanently stored on the local host.
    /// </summary>
    public class PinnedObject
    {
        /// <summary>
        ///   The <see cref="Cid"/> of the object.
        /// </summary>
        public Cid Id { get; set; }

        /// <summary>
        ///   The method used to pin the object.
        /// </summary>
        public PinMode Mode { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Id + " " + Mode.ToString();
        }

    }
}
