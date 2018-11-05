# Accessing IPFS

IPFS is a distributed peer to peer system.  There is no central server!  Typically, each machine (peer) runs 
a [daemon](daemon.md) that communicates with other peers.

The [IpfsClient](xref:Ipfs.Api.IpfsClient) provides a simple way for your program to access the daemon 
via the [IPFS HTTP API](https://docs.ipfs.io/reference/api/http/) protocol. The client 
should be used as a shared object in your program, much like [HttpClient](xref:System.Net.Http.HttpClient).  It is 
thread safe (re-entrant) and conserves sockets and TCP connections when only one instance is used.

```csharp
public class Program
{
  static readonly IpfsClient ipfs = new IpfsClient();
  public async Task Main(string[] args) 
  {
	// Get the Peer info of the daemon
	var peer = await ipfs.IdAsync();
  }
}
```

## Locating the daemon

By default the client looks for a deamon at `http://localhost:5001`.  This can be overriden by either 
setting the environment variable [IpfsHttpUrl](envvars.md) or initialising the client with an URL.

```csharp
// js-ipfs likes this address
static readonly IpfsClient ipfs = new IpfsClient("http://127.0.0.1:5002");
```


