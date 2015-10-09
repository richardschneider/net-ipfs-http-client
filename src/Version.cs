using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace Ipfs.Api
{
    public partial class IpfsClient
    {
        /// <summary>
        ///   Get the version of the API server.
        /// </summary>
        /// <returns>
        ///   A string representing the version of the API server.  For example "0.3.8-dev".
        /// </returns>
        public string Version()
        {
            return DoCommand("version");
        }
    }
}
