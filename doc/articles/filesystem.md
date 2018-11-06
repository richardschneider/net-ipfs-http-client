# IPFS file system

The official name is [UnixFS](https://docs.ipfs.io/guides/concepts/unixfs/).  It allows files and directories of any size 
to be added and retrieved from IPFS via the [FileSystem](xref:Ipfs.CoreApi.IFileSystemApi) 
and [Object](xref:Ipfs.CoreApi.IObjectApi) API.  

## Files

A file has a unique [content id (CID)](xref:Ipfs.Cid) which is the cryptographic hash of the content; see
[CID concept](https://docs.ipfs.io/guides/concepts/cid/) for background information.  The file's content is not just the file's 
data but is encapsulated with a [protocol buffer](https://en.wikipedia.org/wiki/Protocol_Buffers) encoding of the 
[PBNode](https://github.com/ipfs/go-ipfs/blob/0cb22ccf359e05fb5b55a9bf2f9c515bf7d4dba7/merkledag/pb/merkledag.proto#L31-L39) 
and [UnixFS Data](https://github.com/ipfs/go-ipfs/blob/0cb22ccf359e05fb5b55a9bf2f9c515bf7d4dba7/unixfs/pb/unixfs.proto#L3-L20).

Where
- `PBNode.Data` contains unixfs message Data
- unixfs `Data.Data` contans file's data

When the file's data exceeds the [chunking size](xref:Ipfs.CoreApi.AddFileOptions.ChunkSize), multiple [blocks](xref:Ipfs.CoreApi.IBlockApi) 
are generated.  The returned CID points to a block that has `PBNode.Links` and no `PBNode.Data`.

### Adding a file

[AddAsync](xref:Ipfs.CoreApi.IFileSystemApi.AddAsync*) is used to add a stream of data to IPFS.  It returns a 
[FileSystemNode](xref:Ipfs.IFileSystemNode) which 
describes the added the data.  Of particular import is its [CID](xref:Ipfs.IDataBlock.Id).  The helper methods 
[AddTextAsync](xref:Ipfs.CoreApi.IFileSystemApi.AddTextAsync*) and [AddFileAsync](xref:Ipfs.CoreApi.IFileSystemApi.AddFileAsync*) are also available.

All the Add methods accept [options](xref:Ipfs.CoreApi.AddFileOptions) to control how the data is added to IPFS.

```csharp
var fsn = await ipfs.FileSystem.AddTextAsync("hello world");
Console.WriteLine((string)fsn.Id)

// Qmf412jQZiuVUtdgnB36FXFX7xg5V6KEbSJ4dpQuhkLyfD
```

### Reading a file

[ReaFileAsync](xref:Ipfs.CoreApi.IFileSystemApi.ReadFileAsync*) is used to read a stream of data from IPFS.  It returns a 
[Stream](xref:System.IO.Stream) containing just the file's data NOT the protocol buffer encoded data.

```csharp
string path = "Qmf412jQZiuVUtdgnB36FXFX7xg5V6KEbSJ4dpQuhkLyfD";
using (var stream = await ipfs.FileSystem.ReadFileAsyc(path))
{
	// Do something with the data
}
```

