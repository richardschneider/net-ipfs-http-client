# Accessing IPFS

IPFS is a distributed peer to peer system.  There is no central server!  Typically, each machine (peer) runs 
a [daemon](daemon.md) that communicates with other peers.

The [IpfsClient](xref:Ipfs.Http.IpfsClient) provides a simple way for your program to access the daemon 
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

## Core API

The [Core API](xref:Ipfs.CoreApi.ICoreApi) is a set of interfaces to IPFS features and is implemented by the client.  The 
[FileSystem](filesystem.md) and [PubSub]() features are most often used.

```csharp
const string filename = "QmXarR6rgkQ2fDSHjSY5nM2kuCXKYGViky5nohtwgF65Ec/about";
string text = await ipfs.FileSystem.ReadAllTextAsync(filename);
```

### Features

| Feature | Purpose |
| ------- | ------- |
| [Bitswap](xref:Ipfs.CoreApi.IBitswapApi) | Data trading module for IPFS; requests blocks from and sends blocks to other peers |
| [Block](xref:Ipfs.CoreApi.IBlockApi) | Manages the blocks |
| [Bootstrap](xref:Ipfs.CoreApi.IBootstrapApi) | Trusted peers |
| [Config](xref:Ipfs.CoreApi.IConfigApi) | Manages the configuration of the local peer |
| [Dag](xref:Ipfs.CoreApi.IDagApi) | Manages the IPLD (linked data) Directed Acrylic Graph |
| [Dht](xref:Ipfs.CoreApi.IDhtApi) | Manages the Distributed Hash Table |
| [Dns](xref:Ipfs.CoreApi.IDhtApi) | DNS mapping to IPFS |
| [Misc](xref:Ipfs.CoreApi.IGenericApi) | Some miscellaneous methods |
| [FileSystem](xref:Ipfs.CoreApi.IFileSystemApi) | Manages the files/directories in IPFS |
| [Key](xref:Ipfs.CoreApi.IKeyApi) | Manages the cryptographic keys |
| [Name](xref:Ipfs.CoreApi.INameApi) | Manages the Interplanetary Name Space (IPNS) |
| [Object](xref:Ipfs.CoreApi.IObjectApi) | Manages the IPFS Directed Acrylic Graph |
| [Pin](xref:Ipfs.CoreApi.IPinApi) | Manage objects that are locally stored and permanent |
| [PubSub](xref:Ipfs.CoreApi.IPubSubApi) | Publish and subscribe to topic messages |
| [Swarm](xref:Ipfs.CoreApi.ISwarmApi) | Manages the swarm of peers |

