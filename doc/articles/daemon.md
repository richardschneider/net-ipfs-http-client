# IPFS Daemon

The IPFS daemon is a service that runs on a machine and allows access to other peers on the network. This 
is what the [IPFS client](client.md) manages.

## Installing

The authoritive documentmenation is at [https://docs.ipfs.io/introduction/install/](https://docs.ipfs.io/introduction/install/) which 
describes the `go-ipfs` implementation. 
There is also [js-ipfs](https://docs.ipfs.io/reference/js/overview/) for NodeJS fans.

### Windows

For Windows using [chocolatey](https://chocolatey.org/)

```
> choco install go-ipfs
> ipfs init
> ipfs daemon
```

## Locating the daemon

By default the client looks for a deamon at `http://localhost:5001`.  This can be overriden by either 
setting the environment variable [IpfsHttpUrl](envvars.md) or initialising the client with an URL.

```csharp
// js-ipfs likes this address
static readonly IpfsClient ipfs = new IpfsClient("http://127.0.0.1:5002");
```


