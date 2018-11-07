# Asynchronous I/O

All requests to the IPFS server are [asynchronous](https://docs.microsoft.com/en-us/dotnet/csharp/async),
which does not block current thread.

This means that callers should **normally** use the `async/await` paradigm

```cs
var result = await ipfs.FileSystem.AddTextAsync("I am pinned");
```

## Synchronous

If a synchronous operation is required, then this can work

```cs
var result = ipfs.FileSystem.AddTextAsync("I am pinned").Result;
```

## Cancelling a request

All requests to the IPFS server can be cancelled by supplying 
an optional [CancellationToken](xref:System.Threading.CancellationToken).  When 
the token is cancelled, 
a [TaskCanceledException](xref:System.Threading.Tasks.TaskCanceledException) 
will be `thrown`.

Here's a contrived example ([unit test](https://github.com/richardschneider/net-ipfs-api/blob/cancellation/test/CoreApi/CancellationTest.cs)) 
that forces the getting of info on the local IPFS server to be cancelled

```csharp
var cts = new CancellationTokenSource(500);
try
{
	await Task.Delay(1000);
	var peer = await ipfs.IdAsync(cts.Token);
	Assert.Fail("Did not throw TaskCanceledException");
}
catch (TaskCanceledException)
{
	return;
}
```

See also [Task Cancellation](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-cancellation)
