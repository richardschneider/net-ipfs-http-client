using Newtonsoft.Json;
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
        public IEnumerable<MultiAddress> BootstrapPeers()
        {
            var result = Api().DownloadString(BuildCommand("bootstrap/list"));
            // TODO: return JsonConvert.DeserializeObject<MultiAddress[]>(result);
            throw new NotImplementedException();
        }

    }
}
