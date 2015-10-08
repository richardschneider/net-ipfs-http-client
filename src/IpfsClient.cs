using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ipfs.Api
{
    /// <summary>
    ///   A client that allows access to the InterPlanetary File System (IPFS).
    /// </summary>
    public partial class IpfsClient
    {
        /// <summary>
        ///   Creates a new instance of the <see cref="IpfsClient"/> class and sets the
        ///   default values;
        /// </summary>
        public IpfsClient()
        {
            ApiAddress = new Uri("http://localhost:5001");
        }

        /// <summary>
        ///   The URL to the IPFS API server.  The default is "http://localhost:5001";
        /// </summary>
        public Uri ApiAddress { get; set; }

        protected Uri BuildCommand(string command)
        {
            return new Uri(ApiAddress, "/api/v0/" + command);
        }
    }
}
