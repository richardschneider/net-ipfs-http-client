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