using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Reflection;
using System.Web;

namespace Ipfs.Api
{
    /// <summary>
    ///   A client that allows access to the InterPlanetary File System (IPFS).
    /// </summary>
    /// <seealso href="https://ipfs.io/docs/api/">IPFS API</seealso>
    public partial class IpfsClient
    {
        /// <summary>
        ///   Creates a new instance of the <see cref="IpfsClient"/> class and sets the
        ///   default values;
        /// </summary>
        public IpfsClient()
        {
            ApiAddress = new Uri("http://localhost:5001");
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            UserAgent = string.Format("net-ipfs/{0}.{1}", version.Major, version.Minor);
        }

        /// <summary>
        ///   The URL to the IPFS API server.  The default is "http://localhost:5001";
        /// </summary>
        public Uri ApiAddress { get; set; }

        /// <summary>
        ///   The value of HTTP User-Agent header sent to the API server. 
        /// </summary>
        /// <value>
        ///   The default value is "net-ipfs/M.N", where M is the major and N is minor version
        ///   numbers of the assembly.
        /// </value>
        public string UserAgent { get; set; }

        protected Uri BuildCommand(string command, string arg = null)
        {
            var url = "/api/v0/" + command;
            if (arg != null)
                url += "?arg=" + HttpUtility.UrlEncode(arg);
            return new Uri(ApiAddress, url);
        }

        protected WebClient Api()
        {
            var api = new WebClient
            {
                Encoding = Encoding.UTF8
            };
            api.Headers["User-Agent"] = UserAgent;
            return api;
        }
    }
}
