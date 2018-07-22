namespace Ipfs.DSL
open System.Threading
open FSharpPlus

[<AutoOpen>]
module Cancellation =
    type Cto = CancellationToken option
    type CancellationContext = private | Cancelable of Cto * CancellationTokenSource option

    let mToken (Cancelable(ct,_)) = ct
    let mTokenSource (Cancelable(_,src)) = src

    let cancelNow (Cancelable(_,src)) =
        match src with
        | Some(cts) -> cts.Cancel()
        | None -> ()

    let dontUse :Cto = None

    let newContext = monad {
        let cts = new CancellationTokenSource()
        return! (Cancelable(Some(cts.Token), Some(cts)))
    }

    let emptyContext = Cancelable(None, None)