# net-ipfs-http-client

[![build status](https://ci.appveyor.com/api/projects/status/github/richardschneider/net-ipfs-api?branch=master&svg=true)](https://ci.appveyor.com/project/richardschneider/net-ipfs-api) 
[![Coverage Status](https://coveralls.io/repos/github/richardschneider/net-ipfs-http-client/badge.svg?branch=master)](https://coveralls.io/github/richardschneider/net-ipfs-http-client?branch=master)
[![Version](https://img.shields.io/nuget/v/Ipfs.Http.svg)](https://www.nuget.org/packages/Ipfs.Http)
[![docs](https://img.shields.io/badge/docs-latest-green.svg)](https://richardschneider.github.io/net-ipfs-http-client/articles/client.html)


A .Net client library for managing IPFS using the HTTP API protocol. 
More information, including the Class Reference, is on the [Project](https://richardschneider.github.io/net-ipfs-http-client/) web site.

![](https://ipfs.io/ipfs/QmQJ68PFMDdAsgCZvA1UVzzn18asVcf7HVvCDgpjiSCAse)

## Features

- Targets .NET Framework 4.5, .NET Standard 1.4 and .NET Standard 2.0
- [Asynchronous I/O](https://richardschneider.github.io/net-ipfs-http-client/articles/async.html) to an IPFS server
- Supports [cancellation](https://richardschneider.github.io/net-ipfs-http-client/articles/cancellation.html) of all requests to the IPFS Server
- Requests that all responses are compressed
- Comprehensive [documentation](https://richardschneider.github.io/net-ipfs-http-client/articles/client.html)
- C# style access to the [ipfs core interface](https://richardschneider.github.io/net-ipfs-core/api/Ipfs.CoreApi.html)
  - [Bitswap API](https://richardschneider.github.io/net-ipfs-core/api/Ipfs.CoreApi.IBitswapApi.html)
  - [Block API](https://richardschneider.github.io/net-ipfs-core/api/Ipfs.CoreApi.IBlockApi.html)
  - [Config API](https://richardschneider.github.io/net-ipfs-core/api/Ipfs.CoreApi.IConfigApi.html)
  - [Dag API](https://richardschneider.github.io/net-ipfs-core/api/Ipfs.CoreApi.IDagApi.html)
  - [Dht API](https://richardschneider.github.io/net-ipfs-core/api/Ipfs.CoreApi.IDhtApi.html)
  - [Misc API](https://richardschneider.github.io/net-ipfs-core/api/Ipfs.CoreApi.IGenericApi.html)
  - [FileSystem API](https://richardschneider.github.io/net-ipfs-core/api/Ipfs.CoreApi.IFileSystemApi.html)
  - [Key API](https://richardschneider.github.io/net-ipfs-core/api/Ipfs.CoreApi.IKeyApi.html)
  - [Name API](https://richardschneider.github.io/net-ipfs-core/api/Ipfs.CoreApi.INameApi.html)
  - [Object API](https://richardschneider.github.io/net-ipfs-core/api/Ipfs.CoreApi.IObjectApi.html)
  - [Pin API](https://richardschneider.github.io/net-ipfs-core/api/Ipfs.CoreApi.IPinApi.html)
  - [PubSub API](https://richardschneider.github.io/net-ipfs-core/api/Ipfs.CoreApi.IPubSubApi.html)
  - [Swarm API](https://richardschneider.github.io/net-ipfs-core/api/Ipfs.CoreApi.ISwarmApi.html)

## Getting started

Published releases of IPFS API are available on [NuGet](https://www.nuget.org/packages/ipfs.http.client/).  To install, run the following command in the [Package Manager Console](https://docs.nuget.org/docs/start-here/using-the-package-manager-console).

    PM> Install-Package Ipfs.Http.Client
    
## IpfsClient

Every feature of IPFS is a property of the [IpfsClient](https://richardschneider.github.io/net-ipfs-http-client/api/Ipfs.Http.IpfsClient.html).  The following example 
uses `FileSystem` to read a text file

```csharp
using Ipfs.Http;

var ipfs = new IpfsClient();

const string filename = "QmXarR6rgkQ2fDSHjSY5nM2kuCXKYGViky5nohtwgF65Ec/about";
string text = await ipfs.FileSystem.ReadAllTextAsync(filename);
```

# License
Copyright © 2015-2018 Richard Schneider (makaretu@gmail.com)

The IPFS API library is licensed under the [MIT](http://www.opensource.org/licenses/mit-license.php "Read more about the MIT license form") license. Refer to the [LICENSE](https://github.com/richardschneider/net-ipfs-http-client/blob/master/LICENSE) file for more information.
