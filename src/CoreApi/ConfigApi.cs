using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ipfs.Api
{

    /// <summary>
    ///   Manages the IPFS Configuration.
    /// </summary>
    /// <remarks>
    ///   This API is accessed via the <see cref="IpfsClient.Config"/> property.
    ///   <para>
    ///   Configuration values are JSON.  <see href="http://www.newtonsoft.com/json">Json.NET</see>
    ///   is used to represent JSON.
    ///   </para>
    /// </remarks>
    /// <seealso href="https://github.com/ipfs/interface-ipfs-core/tree/master/API/config">Config API</seealso>
    public class ConfigApi
    {
        IpfsClient ipfs;

        internal ConfigApi(IpfsClient ipfs)
        {
            this.ipfs = ipfs;
        }

        /// <summary>
        ///   Gets the entire configuration.
        /// </summary>
        /// <returns>
        ///   A <see cref="JObject"/> containing the configuration.
        /// </returns>
        public async Task<JObject> GetAsync(CancellationToken cancel = default(CancellationToken))
        {
            var json = await ipfs.DoCommandAsync("config/show", cancel);
            return JObject.Parse(json);
        }

        /// <summary>
        ///   Gets the value of a configuration key.
        /// </summary>
        /// <param name="key">
        ///   The key name, such as "Addresses.API".
        /// </param>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
        /// </param>
        /// <returns>
        ///   The value of the <paramref name="key"/> as <see cref="JToken"/>.
        /// </returns>
        /// <exception cref="Exception">
        ///   When the <paramref name="key"/> does not exist.
        /// </exception>
        /// <remarks>
        ///   Keys are case sensistive.
        /// </remarks>
        public async Task<JToken> GetAsync(string key, CancellationToken cancel = default(CancellationToken))
        {
            var json = await ipfs.DoCommandAsync("config", cancel, key);
            var r = JObject.Parse(json);
            return r["Value"];
        }

        /// <summary>
        ///   Adds or replaces a configuration value.
        /// </summary>
        /// <param name="key">
        ///   The key name, such as "Addresses.API".
        /// </param>
        /// <param name="value">
        ///   The new <see cref="string"/> value of the <paramref name="key"/>.
        /// </param>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
        /// </param>
        public async Task SetAsync(string key, string value, CancellationToken cancel = default(CancellationToken))
        {
            var _ = await ipfs.DoCommandAsync("config", cancel, key, "arg=" + value);
            return;
        }

        /// <summary>
        ///   Adds or replaces a configuration value.
        /// </summary>
        /// <param name="key">
        ///   The key name, such as "Addresses.API".
        /// </param>
        /// <param name="value">
        ///   The new <see cref="JToken">JSON</see> value of the <paramref name="key"/>.
        /// </param>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
        /// </param>
        public async Task SetAsync(string key, JToken value, CancellationToken cancel = default(CancellationToken))
        {
            var _ = await ipfs.DoCommandAsync("config", cancel,
                key,
                "arg=" + value.ToString(Formatting.None),
                "json=true");
            return;
        }

        /// <summary>
        ///   Replaces the entire configuration.
        /// </summary>
        /// <param name="config"></param>
        /// <remarks>
        ///   Not Yet Implemented.
        /// </remarks>
        public Task ReplaceAsync(JObject config)
        {
            throw new NotImplementedException();
        }
    }

}
