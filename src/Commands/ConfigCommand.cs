using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class ConfigCommand
    {
        IpfsClient ipfs;

        internal ConfigCommand(IpfsClient ipfs)
        {
            this.ipfs = ipfs;
        }

        /// <summary>
        ///   Gets the entire configuration.
        /// </summary>
        /// <returns>
        ///   A <see cref="JObject"/> containing the configuration.
        /// </returns>
        public async Task<JObject> GetAsync()
        {
            var json = await ipfs.DoCommandAsync("config/show");
            return JObject.Parse(json);
        }

        /// <summary>
        ///   Gets the value of a configuration key.
        /// </summary>
        /// <param name="key">
        ///   The key name, such as "Addresses.API".
        /// </param>
        /// <returns>
        ///   The value of the <paramref name="key"/> as <see cref="JToken"/>.
        /// </returns>
        /// <exception cref="IpfsException">
        ///   When the <paramref name="key"/> does not exist.
        /// </exception>
        /// <remarks>
        ///   Keys are case sensistive.
        /// </remarks>
        public async Task<JToken> GetAsync(string key)
        {
            var json = await ipfs.DoCommandAsync("config", key);
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
        public async Task SetAsync(string key, string value)
        {
            var _ = await ipfs.PostCommandAsync("config", key, "arg=" + value);
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
        public async Task SetAsync(string key, JToken value)
        {
            var _ = await ipfs.PostCommandAsync("config", key,
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
