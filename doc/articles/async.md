# Asynchronous I/O

All requests to the IPFS server are [asynchronous](https://docs.microsoft.com/en-us/dotnet/csharp/async),
which does not block current thread.

This means that callers should **normally** use the `async/await` paradigm

```cs
async Task<string> AddText()
{
	var result = await ipfs.FileSystem.AddTextAsync("I am pinned");
	return result.Hash;
}
```

If a synchronous operation is required, then this can work

```cs
string AddText()
{
	var result = ipfs.FileSystem.AddTextAsync("I am pinned").Result;
	return result.Hash;
}
```