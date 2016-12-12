using Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

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
    /// <remarks>
    ///   <b>IpfsClient</b> is thread safe, only one instance is required
    ///   by the application.
    /// </remarks>
    public partial class IpfsClient
    {
        static ILog log = LogManager.GetCurrentClassLogger();
        static object safe = new object();
        static HttpClient api = null;

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

        /// <summary>
        ///   Get the IPFS API.
        /// </summary>
        /// <returns>
        ///   A <see cref="HttpClient"/>.
        /// </returns>
        /// <remarks>
        ///   Only one client is needed.  Its thread safe.
        /// </remarks>
        HttpClient Api()
        {
            if (api == null)
            {
                lock (safe)
                {
                    if (api == null)
                    {
                        api = new HttpClient();
                        api.DefaultRequestHeaders.Add("User-Agent", UserAgent);
                    }
                }
            }
            return api;
        }

        /// <summary>
        ///  Perform an <see href="https://ipfs.io/docs/api/">IPFS API command</see> returning a string.
        /// </summary>
        /// <param name="command">
        ///   The <see href="https://ipfs.io/docs/api/">IPFS API command</see>, such as
        ///   <see href="https://ipfs.io/docs/api/#apiv0filels">"file/ls"</see>.
        /// </param>
        /// <param name="arg">
        ///   The optional argument to the command.
        /// </param>
        /// <param name="options">
        ///   The optional flags to the command.
        /// </param>
        /// <returns>
        ///   A string representation of the command's result.
        /// </returns>
        public async Task<string> DoCommandAsync(string command, string arg = null, params string[] options)
        {
            try
            {
                var url = BuildCommand(command, arg, options);
                if (log.IsDebugEnabled)
                    log.Debug("GET " + url.ToString());
                var s = await Api().GetStringAsync(url);
                if (log.IsDebugEnabled)
                    log.Debug("RSP " + s);
                return s;
            }
            catch (Exception e)
            {
                throw new IpfsException(e);
            }
        }

        /// <summary>
        ///   Perform an <see href="https://ipfs.io/docs/api/">IPFS API command</see> returning 
        ///   a specific <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T">
        ///   The <see cref="Type"/> of object to return.
        /// </typeparam>
        /// <param name="command">
        ///   The <see href="https://ipfs.io/docs/api/">IPFS API command</see>, such as
        ///   <see href="https://ipfs.io/docs/api/#apiv0filels">"file/ls"</see>.
        /// </param>
        /// <param name="arg">
        ///   The optional argument to the command.
        /// </param>
        /// <param name="options">
        ///   The optional flags to the command.
        /// </param>
        /// <returns>
        ///   A <typeparamref name="T"/>.
        /// </returns>
        /// <remarks>
        ///   The command's response is converted to <typeparamref name="T"/> using
        ///   <c>JsonConvert</c>.
        /// </remarks>
        public async Task<T> DoCommandAsync<T>(string command, string arg = null, params string[] options)
        {
            var json = await DoCommandAsync(command, arg, options);
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        ///  Perform an <see href="https://ipfs.io/docs/api/">IPFS API command</see> returning a
        ///  <see cref="Stream"/>.
        /// </summary>
        /// <param name="command">
        ///   The <see href="https://ipfs.io/docs/api/">IPFS API command</see>, such as
        ///   <see href="https://ipfs.io/docs/api/#apiv0filels">"file/ls"</see>.
        /// </param>
        /// <param name="arg">
        ///   The optional argument to the command.
        /// </param>
        /// <param name="options">
        ///   The optional flags to the command.
        /// </param>
        /// <returns>
        ///   A <see cref="Stream"/> containing the command's result.
        /// </returns>
        public Task<Stream> DownloadAsync(string command, string arg = null, params string[] options)
        {
            var url = BuildCommand(command, arg, options);
            if (log.IsDebugEnabled)
                log.Debug("GET " + url.ToString());
            return Api().GetStreamAsync(url);
        }

        /// <summary>
        ///  Perform an <see href="https://ipfs.io/docs/api/">IPFS API command</see> returning a string.
        /// </summary>
        /// <param name="command">
        ///   The <see href="https://ipfs.io/docs/api/">IPFS API command</see>, such as
        ///   <see href="https://ipfs.io/docs/api/#apiv0filels">"file/ls"</see>.
        /// </param>
        /// <param name="arg">
        ///   The optional argument to the command.
        /// </param>
        /// <param name="options">
        ///   The optional flags to the command.
        /// </param>
        /// <returns>
        ///   A string representation of the command's result.
        /// </returns>
        public string DoCommand(string command, string arg = null, params string[] options)
        {
            return DoCommandAsync(command, arg, options).Result;
        }

        /// <summary>
        ///   Perform an <see href="https://ipfs.io/docs/api/">IPFS API command</see> returning 
        ///   a specific <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T">
        ///   The <see cref="Type"/> of object to return.
        /// </typeparam>
        /// <param name="command">
        ///   The <see href="https://ipfs.io/docs/api/">IPFS API command</see>, such as
        ///   <see href="https://ipfs.io/docs/api/#apiv0filels">"file/ls"</see>.
        /// </param>
        /// <param name="arg">
        ///   The optional argument to the command.
        /// </param>
        /// <param name="options">
        ///   The optional flags to the command.
        /// </param>
        /// <returns>
        ///   A <typeparamref name="T"/>.
        /// </returns>
        /// <remarks>
        ///   The command's response is converted to <typeparamref name="T"/> using
        ///   <c>JsonConvert</c>.
        /// </remarks>
        public T DoCommand<T>(string command, string arg = null, params string[] options)
        {
            return DoCommandAsync<T>(command, arg, options).Result;
        }

        /// <summary>
        ///  Perform an <see href="https://ipfs.io/docs/api/">IPFS API command</see> returning a
        ///  <see cref="Stream"/>.
        /// </summary>
        /// <param name="command">
        ///   The <see href="https://ipfs.io/docs/api/">IPFS API command</see>, such as
        ///   <see href="https://ipfs.io/docs/api/#apiv0filels">"file/ls"</see>.
        /// </param>
        /// <param name="arg">
        ///   The optional argument to the command.
        /// </param>
        /// <param name="options">
        ///   The optional flags to the command.
        /// </param>
        /// <returns>
        ///   A <see cref="Stream"/> containing the command's result.
        /// </returns>
        public Stream Download(string command, string arg = null, params string[] options)
        {
            return DownloadAsync(command, arg, options).Result;
        }
    
    }
}
