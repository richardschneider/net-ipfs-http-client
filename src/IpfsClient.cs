using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Reflection;
using System.Web;
using Newtonsoft.Json;

namespace Ipfs.Api
{
    /// <summary>
    ///   A client that allows access to the InterPlanetary File System (IPFS).
    /// </summary>
    /// <remarks>
    ///   The API is based on the <see href="https://ipfs.io/docs/commands/">IPFS commands</see>.
    /// </remarks>
    /// <seealso href="https://ipfs.io/docs/api/">IPFS API</seealso>
    /// <seealso href="https://ipfs.io/docs/commands/">IPFS commands</seealso>
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
            TrustedPeers = new TruestedPeerCollection(this);
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

        /// <summary>
        ///   The list of peers that are initially trusted by IPFS.
        /// </summary>
        /// <remarks>
        ///   This is equilivent to <c>ipfs bootstrap list</c>.
        /// </remarks>
        public TruestedPeerCollection TrustedPeers { get; private set; }

        Uri BuildCommand(string command, string arg = null)
        {
            var url = "/api/v0/" + command;
            if (arg != null)
                url += "?arg=" + HttpUtility.UrlEncode(arg);
            return new Uri(ApiAddress, url);
        }

        WebClient Api()
        {
            var api = new WebClient
            {
                Encoding = Encoding.UTF8
            };
            api.Headers["User-Agent"] = UserAgent;
            return api;
        }

        protected internal string DoCommand(string command, string arg = null)
        {
            return Api().DownloadString(BuildCommand(command, arg));
        }

        protected internal T DoCommand<T>(string command, string arg = null)
        {
            var json = Api().DownloadString(BuildCommand(command, arg));
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
