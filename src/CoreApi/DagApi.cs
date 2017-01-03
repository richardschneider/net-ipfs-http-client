using Common.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ipfs.Api
{

    /// <summary>
    ///   Manages the Directed Acrylic Graph.
    /// </summary>
    /// <remarks>
    ///   <note>
    ///   The Dag API seems to be a work in progress.  There are no tests nor implemtations
    ///   of it.  All methods throw <see cref="NotImplementedException"/>.
    ///   </note>
    ///   <para>
    ///   This API is accessed via the <see cref="IpfsClient.DagApi"/> property.
    ///   </para>
    /// </remarks>
    /// <seealso href="https://github.com/ipfs/interface-ipfs-core/tree/master/API/dag">DAG API</seealso>
    public class DagApi
    {
        static ILog log = LogManager.GetLogger<DagApi>();

        IpfsClient ipfs;

        internal DagApi(IpfsClient ipfs)
        {
            this.ipfs = ipfs;
        }

        public Task PutAsync(DagNode node, string multicodec, string hashAlgorithm = MultiHash.DefaultAlgorithmName)
        {
            throw new NotImplementedException();
        }

        public Task<DagNode> GetAsync(string cid)
        {
            throw new NotImplementedException();
        }

    }
}
