using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ipfs.Api
{
    /// <summary>
    ///    The method used to pin an IPFS object.
    /// </summary>
    [Flags]
    public enum PinMode
    {
        /// <summary>
        ///   Pin the specific object, and indirectly pin all its decendants
        /// </summary>
        Recursive = 1,

        /// <summary>
        ///    Pin the specific object.
        /// </summary>
        Direct = 2,

        /// <summary>
        ///    Pinned indirectly by an ancestor (like a refcount)
        /// </summary>
        Indirect = 4,

        /// <summary>
        ///   All 
        /// </summary>
        All = 7
    }
}
