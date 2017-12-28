using Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;

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
        static ILog log = LogManager.GetLogger(typeof(IpfsClient));
        static object safe = new object();
        static HttpClient api = null;

        /// <summary>
        ///   The default URL to the IPFS HTTP API server.
        /// </summary>
        /// <value>
        ///   The default is "http://localhost:5001".
        /// </value>
        /// <remarks>
        ///   The environment variable "IpfsHttpApi" overrides this value.
        /// </remarks>
        public static Uri DefaultApiUri = new Uri(
            Environment.GetEnvironmentVariable("IpfsHttpApi") 
            ?? "http://localhost:5001");

        /// <summary>
        ///   Creates a new instance of the <see cref="IpfsClient"/> class and sets the
        ///   default values.
        /// </summary>
        /// <remarks>
        ///   All methods of IpfsClient are thread safe.  Typically, only one instance is required for
        ///   an application.
        /// </remarks>
        public IpfsClient()
        {
            ApiUri = DefaultApiUri;
            var version = typeof(IpfsClient).GetTypeInfo().Assembly.GetName().Version;
            UserAgent = string.Format("net-ipfs/{0}.{1}", version.Major, version.Minor);
            TrustedPeers = new TrustedPeerCollection(this);
            PinnedObjects = new PinnedCollection(this);
            Block = new BlockApi(this);
            Config = new ConfigApi(this);
            Pin = new PinApi(this);
            Dht = new DhtApi(this);
            Swarm = new SwarmApi(this);
            Dag = new DagApi(this);
            Object = new ObjectApi(this);
            FileSystem = new FileSystemApi(this);
            PubSub = new PubSubApi(this);
            Key = new KeyApi(this);
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
        public TrustedPeerCollection TrustedPeers { get; private set; }

        /// <summary>
        ///   The list of objects that are permanently stored on the local host.
        /// </summary>
        /// <remarks>
        ///   This is equilivent to <c>ipfs pin ls</c>.
        /// </remarks>
        public PinnedCollection PinnedObjects { get; private set; }

        /// <summary>
        ///   Provides access to the <see cref="BlockApi">Block API</see>.
        /// </summary>
        public BlockApi Block { get; private set; }

        /// <summary>
        ///   Provides access to the <see cref="ConfigApi">Config API</see>.
        /// </summary>
        public ConfigApi Config { get; private set; }

        /// <summary>
        ///   Provides access to the <see cref="PinApi">Pin API</see>.
        /// </summary>
        public PinApi Pin { get; private set; }

        /// <summary>
        ///   Provides access to the <see cref="DagApi">DAG API</see>.
        /// </summary>
        public DagApi Dag { get; private set; }

        /// <summary>
        ///   Provides access to the <see cref="DhtApi">Distributed Hash Table API</see>.
        /// </summary>
        public DhtApi Dht { get; private set; }

        /// <summary>
        ///   Provides access to the <see cref="SwarmApi">Swarm API</see>.
        /// </summary>
        public SwarmApi Swarm { get; private set; }

        /// <summary>
        ///   Provides access to the <see cref="ObjectApi">Object API</see>.
        /// </summary>
        public ObjectApi Object { get; private set; }

        /// <summary>
        ///   Provides access to the <see cref="FileSystemApi">File System API</see>.
        /// </summary>
        public FileSystemApi FileSystem { get; private set; }

        /// <summary>
        ///   Provides access to the <see cref="PubSubApi">PubSub API</see>.
        /// </summary>
        public PubSubApi PubSub { get; private set; }

        /// <summary>
        ///   Provides access to the <see cref="KeyApi">Key API</see>.
        /// </summary>
        public KeyApi Key { get; private set; }

        Uri BuildCommand(string command, string arg = null, params string[] options)
        {
            var url = "/api/v0/" + command;
            var q = new StringBuilder();
            if (arg != null)
            {
                q.Append("&arg=");
                q.Append(WebUtility.UrlEncode(arg));
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
                    q.Append(WebUtility.UrlEncode(option.Substring(i + 1)));
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
                        var handler = new HttpClientHandler();
                        if (handler.SupportsAutomaticDecompression)
                        {
                            handler.AutomaticDecompression = DecompressionMethods.GZip
                                | DecompressionMethods.Deflate;
                        }
                        api = new HttpClient(handler);
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
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
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
        /// <exception cref="HttpRequestException">
        ///   When the IPFS server indicates an error.
        /// </exception>
        public async Task<string> DoCommandAsync(string command, CancellationToken cancel, string arg = null, params string[] options)
        {
            var url = BuildCommand(command, arg, options);
            if (log.IsDebugEnabled)
                log.Debug("GET " + url.ToString());
            using (var response = await Api().GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancel))
            {
                await ThrowOnErrorAsync(response);
                var body = await response.Content.ReadAsStringAsync();
                if (log.IsDebugEnabled)
                    log.Debug("RSP " + body);
                return body;
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
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
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
        /// <exception cref="HttpRequestException">
        ///   When the IPFS server indicates an error.
        /// </exception>
        public async Task<T> DoCommandAsync<T>(string command, CancellationToken cancel, string arg = null, params string[] options)
        {
            var json = await DoCommandAsync(command, cancel, arg, options);
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        ///  Post an <see href="https://ipfs.io/docs/api/">IPFS API command</see> returning a string.
        /// </summary>
        /// <param name="command">
        ///   The <see href="https://ipfs.io/docs/api/">IPFS API command</see>, such as
        ///   <see href="https://ipfs.io/docs/api/#apiv0filels">"file/ls"</see>.
        /// </param>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
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
        /// <exception cref="HttpRequestException">
        ///   When the IPFS server indicates an error.
        /// </exception>
        public async Task<string> PostCommandAsync(string command, CancellationToken cancel, string arg = null, params string[] options)
        {
            var url = BuildCommand(command, arg, options);
            if (log.IsDebugEnabled)
                log.Debug("POST " + url.ToString());
            var request = new HttpRequestMessage(HttpMethod.Post, url);

            using (var response = await Api().SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancel))
            {
                await ThrowOnErrorAsync(response);
                var body = await response.Content.ReadAsStringAsync();
                if (log.IsDebugEnabled)
                    log.Debug("RSP " + body);
                return body;
            }
        }

        /// <summary>
        ///   Post an <see href="https://ipfs.io/docs/api/">IPFS API command</see> returning 
        ///   a specific <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T">
        ///   The <see cref="Type"/> of object to return.
        /// </typeparam>
        /// <param name="command">
        ///   The <see href="https://ipfs.io/docs/api/">IPFS API command</see>, such as
        ///   <see href="https://ipfs.io/docs/api/#apiv0filels">"file/ls"</see>.
        /// </param>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
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
        /// <exception cref="HttpRequestException">
        ///   When the IPFS server indicates an error.
        /// </exception>
        public async Task<T> PostCommandAsync<T>(string command, CancellationToken cancel, string arg = null, params string[] options)
        {
            var json = await PostCommandAsync(command, cancel, arg, options);
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        ///  Post an <see href="https://ipfs.io/docs/api/">IPFS API command</see> returning a stream.
        /// </summary>
        /// <param name="command">
        ///   The <see href="https://ipfs.io/docs/api/">IPFS API command</see>, such as
        ///   <see href="https://ipfs.io/docs/api/#apiv0filels">"file/ls"</see>.
        /// </param>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
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
        /// <exception cref="HttpRequestException">
        ///   When the IPFS server indicates an error.
        /// </exception>
        public async Task<Stream> PostDownloadAsync(string command, CancellationToken cancel, string arg = null, params string[] options)
        {
            var url = BuildCommand(command, arg, options);
            if (log.IsDebugEnabled)
                log.Debug("POST " + url.ToString());
            var request = new HttpRequestMessage(HttpMethod.Post, url);

            var response = await Api().SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancel);
            await ThrowOnErrorAsync(response);
            return await response.Content.ReadAsStreamAsync();
        }

        /// <summary>
        ///  Perform an <see href="https://ipfs.io/docs/api/">IPFS API command</see> returning a
        ///  <see cref="Stream"/>.
        /// </summary>
        /// <param name="command">
        ///   The <see href="https://ipfs.io/docs/api/">IPFS API command</see>, such as
        ///   <see href="https://ipfs.io/docs/api/#apiv0filels">"file/ls"</see>.
        /// </param>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
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
        /// <exception cref="HttpRequestException">
        ///   When the IPFS server indicates an error.
        /// </exception>
        public async Task<Stream> DownloadAsync(string command, CancellationToken cancel, string arg = null, params string[] options)
        {
            var url = BuildCommand(command, arg, options);
            if (log.IsDebugEnabled)
                log.Debug("GET " + url.ToString());
            var response = await Api().GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancel);
            await ThrowOnErrorAsync(response);
            return await response.Content.ReadAsStreamAsync();
        }

        /// <summary>
        ///  Perform an <see href="https://ipfs.io/docs/api/">IPFS API command</see> returning a
        ///  a byte array.
        /// </summary>
        /// <param name="command">
        ///   The <see href="https://ipfs.io/docs/api/">IPFS API command</see>, such as
        ///   <see href="https://ipfs.io/docs/api/#apiv0filels">"file/ls"</see>.
        /// </param>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
        /// </param>
        /// <param name="arg">
        ///   The optional argument to the command.
        /// </param>
        /// <param name="options">
        ///   The optional flags to the command.
        /// </param>
        /// <returns>
        ///   A byte array containing the command's result.
        /// </returns>
        /// <exception cref="HttpRequestException">
        ///   When the IPFS server indicates an error.
        /// </exception>
        public async Task<byte[]> DownloadBytesAsync(string command, CancellationToken cancel, string arg = null, params string[] options)
        {
            var url = BuildCommand(command, arg, options);
            if (log.IsDebugEnabled)
                log.Debug("GET " + url.ToString());
            var response = await Api().GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancel);
            await ThrowOnErrorAsync(response);
            return await response.Content.ReadAsByteArrayAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
        /// </param>
        /// <param name="data"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="HttpRequestException">
        ///   When the IPFS server indicates an error.
        /// </exception>
        public async Task<String> UploadAsync(string command, CancellationToken cancel, Stream data, params string[] options)
        {
            var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(data);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Add(streamContent, "file");

            var url = BuildCommand(command, null, options);
            if (log.IsDebugEnabled)
                log.Debug("POST " + url.ToString());
            using (var response = await Api().PostAsync(url, content, cancel))
            {
                await ThrowOnErrorAsync(response);
                var json = await response.Content.ReadAsStringAsync();
                if (log.IsDebugEnabled)
                    log.Debug("RSP " + json);
                return json;
            }
        }

        /// <summary>
        ///  TODO
        /// </summary>
        public async Task<String> UploadAsync(string command, CancellationToken cancel, byte[] data, params string[] options)
        {
            var content = new MultipartFormDataContent();
            var streamContent = new ByteArrayContent(data);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Add(streamContent, "file");

            var url = BuildCommand(command, null, options);
            if (log.IsDebugEnabled)
                log.Debug("POST " + url.ToString());
            using (var response = await Api().PostAsync(url, content, cancel))
            {
                await ThrowOnErrorAsync(response);
                var json = await response.Content.ReadAsStringAsync();
                if (log.IsDebugEnabled)
                    log.Debug("RSP " + json);
                return json;
            }
        }

        /// <summary>
        ///   Throws an <see cref="HttpRequestException"/> if the response
        ///   does not indicate success.
        /// </summary>
        /// <param name="response"></param>
        /// <returns>
        ///    <b>true</b>
        /// </returns>
        /// <remarks>
        ///   The API server returns an JSON error in the form <c>{ "Message": "...", "Code": ... }</c>.
        /// </remarks>
        async Task<bool> ThrowOnErrorAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
                return true;
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                var error = "Invalid IPFS command: " + response.RequestMessage.RequestUri.ToString();
                if (log.IsDebugEnabled)
                    log.Debug("ERR " + error);
                throw new HttpRequestException(error);
            }

            var body = await response.Content.ReadAsStringAsync();
            if (log.IsDebugEnabled)
                log.Debug("ERR " + body);
            string message = body;
            try
            {
                message = (string)JsonConvert.DeserializeObject<dynamic>(body).Message;
            }
            catch { }
            throw new HttpRequestException(message);
        }

    }
}
