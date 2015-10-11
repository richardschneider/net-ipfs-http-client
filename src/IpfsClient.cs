using Common.Logging;
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
        static ILog log = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///   The default URL to the IPFS API server.  The default is "http://localhost:5001".
        /// </summary>
        public static Uri DefaultApiUri = new Uri("http://localhost:5001");

        /// <summary>
        ///   Creates a new instance of the <see cref="IpfsClient"/> class and sets the
        ///   default values.
        /// </summary>
        public IpfsClient()
        {
            ApiUri = DefaultApiUri;
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            UserAgent = string.Format("net-ipfs/{0}.{1}", version.Major, version.Minor);
            TrustedPeers = new TruestedPeerCollection(this);
        }

        /// <summary>
        ///   Creates a new instance of the <see cref="IpfsClient"/> class and specifies
        ///   the <see cref="ApiUri">API host's URL</see>.
        ///   default values
        /// </summary>
        /// <param name="host">
        ///   The URL of the API host.  For example "http://localhost:5001" or "http://ipv4.fiddler:5001".
        /// </param>
        public IpfsClient(string host)
            : this()
        {
            ApiUri = new Uri(host);
        }
        
        /// <summary>
        ///   The URL to the IPFS API server.  The default is "http://localhost:5001".
        /// </summary>
        public Uri ApiUri { get; set; }

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

        Uri BuildCommand(string command, string arg = null, params string[] options)
        {
            var url = "/api/v0/" + command;
            var q = new StringBuilder();
            if (arg != null)
            {
                q.Append("&arg=");
                q.Append(HttpUtility.UrlEncode(arg));
            }

            foreach (var option in options)
            {
                q.Append('&');
                var i = option.IndexOf('=');
                if (i < 0)
                {
                    q.Append(option);
                }
                else
                {
                    q.Append(option.Substring(0, i));
                    q.Append('=');
                    q.Append(HttpUtility.UrlEncode(option.Substring(i + 1)));
                }
            }

            if (q.Length > 0)
            {
                q[0] = '?';
                q.Insert(0, url);
                url = q.ToString();
            }

            return new Uri(ApiUri, url);
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

        protected internal string DoCommand(string command, string arg = null, params string[] options)
        {
            try
            {
                var url = BuildCommand(command, arg, options);
                if (log.IsDebugEnabled)
                    log.Debug("GET " + url.ToString());
                var s = Api().DownloadString(url);
                if (log.IsDebugEnabled)
                    log.Debug("RSP " + s);
                return s;
            }
            catch (Exception e)
            {
                throw new IpfsException(e);
            }
        }

        protected internal T DoCommand<T>(string command, string arg = null, params string[] options)
        {
            var json = DoCommand(command, arg, options);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
