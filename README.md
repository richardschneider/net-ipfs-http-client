# net-ipfs-api

[![build status](https://ci.appveyor.com/api/projects/status/github/richardschneider/net-ipfs-api?branch=master&svg=true)](https://ci.appveyor.com/project/richardschneider/net-ipfs-api) 
[![Coverage Status](https://coveralls.io/repos/richardschneider/net-ipfs-api/badge.svg?branch=master&service=github)](https://coveralls.io/github/richardschneider/net-ipfs-api?branch=master)
[![Version](https://img.shields.io/nuget/v/Ipfs.Api.svg)](https://www.nuget.org/packages/Ipfs.Api)
[![docs](https://cdn.rawgit.com/richardschneider/net-ipfs-core/master/doc/images/docs-latest-green.svg)](https://richardschneider.github.io/net-ipfs-api)


A .Net client library for the IPFS HTTP API, implemented in C#. 
More information, including the Class Reference, is on the [Project](https://richardschneider.github.io/net-ipfs-api/) web site.

![](https://ipfs.io/ipfs/QmQJ68PFMDdAsgCZvA1UVzzn18asVcf7HVvCDgpjiSCAse)

## Features

- Targets .NET Framework 4.5, .NET Standard 1.4 and .NET Standard 2.0
- [Asynchronous I/O](https://richardschneider.github.io/net-ipfs-api/articles/async.html) to an IPFS server
- Supports [cancellation](https://richardschneider.github.io/net-ipfs-api/articles/cancellation.html) of all requests to the IPFS Server
- C# style access to the [ipfs core interface](https://github.com/ipfs/interface-ipfs-core#api)
  - [Block API](https://richardschneider.github.io/net-ipfs-api/api/Ipfs.Api.BlockApi.html)
  - [Config API](https://richardschneider.github.io/net-ipfs-api/api/Ipfs.Api.ConfigApi.html)
  - [Dag API](https://richardschneider.github.io/net-ipfs-api/api/Ipfs.Api.DagApi.html)
  - [Dht API](https://richardschneider.github.io/net-ipfs-api/api/Ipfs.Api.DhtApi.html)
  - [FileSystem API](https://richardschneider.github.io/net-ipfs-api/api/Ipfs.Api.FileSystemApi.html)
  - [Object API](https://richardschneider.github.io/net-ipfs-api/api/Ipfs.Api.ObjectApi.html)
  - [Pin API](https://richardschneider.github.io/net-ipfs-api/api/Ipfs.Api.PinApi.html)
  - [PubSub API](https://richardschneider.github.io/net-ipfs-api/api/Ipfs.Api.PubSubApi.html)
  - [Swarm API](https://richardschneider.github.io/net-ipfs-api/api/Ipfs.Api.SwarmApi.html)

## Getting started

Published releases of IPFS API are available on [NuGet](https://www.nuget.org/packages/ipfs.api/).  To install, run the following command in the [Package Manager Console](https://docs.nuget.org/docs/start-here/using-the-package-manager-console).

    PM> Install-Package Ipfs.Api
    
For the latest build or older non-released builds see [Continuous Integration](https://github.com/richardschneider/net-ipfs-core/wiki/Continuous-Integration).

## IpfsClient

Every IPFS Api is a property of the [IpfsClient](https://richardschneider.github.io/net-ipfs-api/api/Ipfs.Api.IpfsClient.html).  The following example reads a text file

```
var ipfs = new IpfsClient();

const string filename = "QmXarR6rgkQ2fDSHjSY5nM2kuCXKYGViky5nohtwgF65Ec/about";
string text = await ipfs.FileSystem.ReadAllTextAsync(filename);
```

# License
Copyright © 2015-2017 Richard Schneider (makaretu@gmail.com)

The IPFS API library is licensed under the [MIT](http://www.opensource.org/licenses/mit-license.php "Read more about the MIT license form") license. Refer to the [LICENSE](https://github.com/richardschneider/net-ipfs-api/blob/master/LICENSE) file for more information.
