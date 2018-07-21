namespace Ipfs.DSL
open Ipfs
open Ipfs.Api
open Ipfs.CoreApi
open System
open System.IO
open System.Threading
open System.Threading.Tasks
open System.Collections.Generic
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open FSharpPlus
open FSharp.Control
open FSharp.Control.AsyncPrimitives

module Definitions =

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

    module BitswapDSL =

        // 1. [x] the embedded language specification
        // 2. [x] arguments to be passed by, and results returned by interpreter as specified by the embedded language
        // 3. [x] private low-level interop "calls" that are bound to the expressions of the language
        // 4. [x] surface area - public functions that build expressions to support the embedded language
        // 5. [x] interpreter

        // 1. the embedded language specification
        type BitswapDSL =
            private
            | Get of retrieve:((Cid * Cto) -> Async<IDataBlock>)
            | Wants of lookingfor:((MultiHash * Cto) -> AsyncSeq<Cid>)
            | Unwants of stoplookingfor:((Cid * Cto) -> Async<unit>)

        // 2. arguments to be passed by, and results returned by interpreter as specified by the embedded language
        type BitswapDSLArgs =
            private
            | GetArgs of (Cid * Cto)
            | WantsArgs of (MultiHash * Cto)
            | UnwantsArgs of (Cid * Cto)
            static member prepareGet (id) (ct) = GetArgs(id, ct)
            static member prepareWants (mh) (ct) = WantsArgs(mh, ct)
            static member prepareUnwants (id) (ct) = UnwantsArgs(id, ct)

        type BitswapDSLResults =
            private
            | Err
            | GetResult of Async<IDataBlock>
            | WantsResult of AsyncSeq<Cid>
            | UnwantsResult of Async<unit>

        // 3. private low-level interop "calls" that are bound to the expressions of the language
        let inline private getCall (client:IpfsClient) (pair:Cid * Cto) = async {
            let op =
                match snd pair with
                | Some(c) -> client.Bitswap.GetAsync(fst pair, c)
                | None -> client.Bitswap.GetAsync(fst pair)
            return! Async.AwaitTask op
        }

        let inline private wantsCall (client:IpfsClient) (pair: MultiHash * Cto) = asyncSeq {
            let op =
                match snd pair with
                | Some(c) -> client.Bitswap.WantsAsync(fst pair, c)
                | None -> client.Bitswap.WantsAsync(fst pair)

            let! results = Async.AwaitTask op
            for id in results do
                yield id
        }

        let inline private unwantsCall (client:IpfsClient) (pair: Cid * Cto) = async {
            let op = 
                match snd pair with
                | Some(c) -> client.Bitswap.UnwantAsync(fst pair, c)
                | None -> client.Bitswap.UnwantAsync(fst pair)
            return! Async.AwaitTask op
        }

        // 4. surface area - public functions that build expressions to support the embedded language
        let inline get (client) = Get(getCall client)
        let inline wants (client) = Wants(wantsCall client)
        let inline unwants (client) = Unwants(unwantsCall client)

        // 5. interpreter
        let inline interpret (expression:BitswapDSL) (args:BitswapDSLArgs) : Async<BitswapDSLResults> = async {
            match expression, args with
            | Get(f), GetArgs(a) -> return GetResult(f a)
            | Wants(f), WantsArgs(a) -> return WantsResult(f a)
            | Unwants(f), UnwantsArgs(a) -> return UnwantsResult(f a)
            | _,_ -> return Err
        }

    
    type private BlockData = byte[]
    type private ContentType = string
    type private HashAlgorithm = string
    type private Encoding = string
    type private Pin = bool
    type private IgnoreNonexistent = bool

    module BlockDSL =

        // 1. [x] the embedded language specification
        // 2. [x] arguments to be passed by, and results returned by interpreter as specified by the embedded language
        // 3. [x] private low-level interop "calls" that are bound to the expressions of the language
        // 4. [x] surface area - public functions that build expressions to support the embedded language
        // 5. [x] interpreters
        
        // 1. the embedded language specification
        type BlockDSL =
            private
            | Get of retrieve:((Cid * Cto) -> Async<IDataBlock>)
            | PutArray of store:((BlockData * ContentType * HashAlgorithm * Encoding * Pin * Cto) -> Async<Cid>)
            | PutStream of store:((Stream * ContentType * HashAlgorithm * Encoding * Pin * Cto) -> Async<Cid>)
            | Stats of about:((Cid * Cto) -> Async<IDataBlock>)
            | Remove of delete:((Cid * IgnoreNonexistent * Cto) -> Async<Cid>)

        // 2. arguments to be passed by, and results returned by interpreter as specified by the embedded language
        type BlockDSLArgs =
            private
            | GetArgs of (Cid * Cto)
            | PutArrayArgs of (BlockData * ContentType * HashAlgorithm * Encoding * Pin * Cto)
            | PutStreamArgs of (Stream * ContentType * HashAlgorithm * Encoding * Pin * Cto)
            | StatsArgs of (Cid * Cto)
            | RemoveArgs of (Cid * IgnoreNonexistent * Cto)
            static member prepareGet (id) (ct) = GetArgs(id, ct)
            static member preparePutArray (data) (ctype) (hash) (enc) (pin) (ct) = PutArrayArgs(data, ctype, hash, enc, pin, ct)
            static member preparePutStream (data) (ctype) (hash) (enc) (pin) (ct) = PutStreamArgs(data, ctype, hash, enc, pin, ct)
            static member prepareStatsArgs (id) (ct) = StatsArgs(id, ct)
            static member prepareRemoveArgs (id) (ignore) (ct) = RemoveArgs(id, ignore, ct)

        type BlockDSLResults =
            private
            | Err
            | GetResult of Async<IDataBlock>
            | PutArrayResult of Async<Cid>
            | PutStreamResult of Async<Cid>
            | StatsResult of Async<IDataBlock>
            | RemoveResult of Async<Cid>

        // 3. private low-level interop "calls" that are bound to the expressions of the language
        let inline private getCall (client:IpfsClient) (pair: Cid * Cto) = async {
            let op =
                match snd pair with
                | Some(c) -> client.Block.GetAsync(fst pair, c)
                | None -> client.Block.GetAsync(fst pair)
            return! Async.AwaitTask op
        }

        let inline private putArrayCall (client:IpfsClient) (data:byte[]) (ctype:string) (hash:string) (enc:string) (pin:bool) (ct:Cto) = async {
            let c5 (a:byte[]) (b:string) (c:string) (d:string) (e:bool) = client.Block.PutAsync(a, b, c, d, e)
            let c6 (a:byte[]) (b:string) (c:string) (d:string) (e:bool) (f:CancellationToken) = client.Block.PutAsync(a, b, c, d, e, f)
            
            let op =
                match ct with
                | Some(c) -> c6 data ctype hash enc pin c
                | None -> c5 data ctype hash enc pin
            return! Async.AwaitTask op
        }

        let inline private fittedPutArrayCall (client:IpfsClient) (t: BlockData * ContentType * HashAlgorithm * Encoding * Pin * Cto) = async {
            let (data:byte[], ctype:string, hash:string, enc:string, pin:bool, ct:Cto) = t
            return! putArrayCall client data ctype hash enc pin ct
        }

        let inline private putStreamCall (client:IpfsClient) (data:Stream) (ctype:string) (hash:string) (enc:string) (pin:bool) (ct:Cto) = async {
            let c5 (a:Stream) (b:string) (c:string) (d:string) (e:bool) = client.Block.PutAsync(a, b, c, d, e)
            let c6 (a:Stream) (b:string) (c:string) (d:string) (e:bool) (f:CancellationToken) = client.Block.PutAsync(a, b, c, d, e, f)

            let op =
                match ct with
                | Some(c) -> c6 data ctype hash enc pin c
                | None -> c5 data ctype hash enc pin
            return! Async.AwaitTask op
        }

        let inline private fittedPutStreamCall (client:IpfsClient) (t: Stream * ContentType * HashAlgorithm * Encoding * Pin * Cto) = async {
            let (data:Stream, ctype:string, hash:string, enc:string, pin:bool, ct:Cto) = t
            return! putStreamCall client data ctype hash enc pin ct
        }

        let inline private statsCall (client:IpfsClient) (pair: Cid * Cto) = async {
            let op =
                match snd pair with
                | Some(c) -> client.Block.StatAsync(fst pair, c)
                | None -> client.Block.StatAsync(fst pair)
            return! Async.AwaitTask op
        }

        let inline private removeCall (client:IpfsClient) (triple: Cid * IgnoreNonexistent * Cto) = async {
            let (id, ignore, cto) = triple
            let op =
                match cto with
                | Some(c) -> client.Block.RemoveAsync(id, ignore, c)
                | None -> client.Block.RemoveAsync(id, ignore)
            return! Async.AwaitTask op
        }

        // 4. surface area - public functions that build expressions to support the embedded language
        let inline get (client) = Get(getCall client)
        let inline putArray (client) = PutArray(fittedPutArrayCall client)
        let inline putStream (client) = PutStream(fittedPutStreamCall client)
        let inline stats (client) = Stats(statsCall client)
        let inline remove (client) = Remove(removeCall client)

        // 5. interpreters
        let inline interpret (expression:BlockDSL) (args:BlockDSLArgs) : Async<BlockDSLResults> = async {
            match expression, args with
            | Get(f), GetArgs(a) -> return GetResult(f a)
            | PutArray(f), PutArrayArgs(a) -> return PutArrayResult(f a)
            | PutStream(f), PutStreamArgs(a) -> return PutStreamResult(f a)
            | Stats(f), StatsArgs(a) -> return StatsResult(f a)
            | Remove(f), RemoveArgs(a) -> return RemoveResult(f a)
            | _,_ -> return Err
        }

    module BootstrapDSL =
        
        // 1. [x] the embedded language specification
        // 2. [x] arguments to be passed by, and results returned by interpreter as specified by the embedded language
        // 3. [x] private low-level interop "calls" that are bound to the expressions of the language
        // 4. [x] surface area - public functions that build expressions to support the embedded language
        // 5. [x] interpreters

        // 1. the embedded language specification
        type BootstrapDSL =
            private
            | Add of join:((MultiAddress * Cto) -> Async<MultiAddress>)
            | AddDefaults of join:(Cto -> AsyncSeq<MultiAddress>)
            | Ls of about:(Cto -> AsyncSeq<MultiAddress>)
            | RemoveAll of drop:(Cto -> Async<unit>)
            | Remove of drop:((MultiAddress * Cto) -> Async<MultiAddress>)

        // 2. arguments to be passed by, and results returned by interpreter as specified by the embedded language
        type BootstrapDSLArgs =
            private
            | AddArgs of (MultiAddress * Cto)
            | AddDefaultsArgs of Cto
            | LsArgs of Cto
            | RemoveAllArgs of Cto
            | RemoveArgs of (MultiAddress * Cto)
            static member prepareAdd (ma) (ct) = AddArgs(ma, ct)
            static member prepareAddDefaults (ct) = AddDefaultsArgs(ct)
            static member prepareLs (ct) = LsArgs(ct)
            static member prepareRemoveAll (ct) = RemoveAllArgs(ct)
            static member prepareRemove (ma) (ct) = RemoveArgs(ma, ct)

        type BootstrapDSLResults =
            private
            | Err
            | AddResult of Async<MultiAddress>
            | AddDefaultsResult of AsyncSeq<MultiAddress>
            | LsResult of AsyncSeq<MultiAddress>
            | RemoveAllResult of Async<unit>
            | RemoveResult of Async<MultiAddress>

        // 3. private low-level interop "calls" that are bound to the expressions of the language
        let inline private addCall (client:IpfsClient) (pair: MultiAddress * Cto) = async {
            let op = 
                match snd pair with
                | Some(c) -> client.Bootstrap.AddAsync(fst pair, c)
                | None -> client.Bootstrap.AddAsync(fst pair)
            return! Async.AwaitTask op
        }

        let inline private addDefaultsCall (client:IpfsClient) (ct:Cto) = asyncSeq {
            let op =
                match ct with
                | Some(c) -> client.Bootstrap.AddDefaultsAsync(c)
                | None -> client.Bootstrap.AddDefaultsAsync()

            let! results = Async.AwaitTask op
            for id in results do
                yield id
        }

        let inline private listCall (client:IpfsClient) (ct:Cto) = asyncSeq {
            let op =
                match ct with
                | Some(c) -> client.Bootstrap.ListAsync(c)
                | None -> client.Bootstrap.ListAsync()

            let! results = Async.AwaitTask op
            for id in results do
                yield id
        }

        let inline private removeAllCall (client:IpfsClient) (ct:Cto) = async {
            let op =
                match ct with
                | Some(c) -> client.Bootstrap.RemoveAllAsync(c)
                | None -> client.Bootstrap.RemoveAllAsync()
            return! Async.AwaitTask op
        }

        let inline private removeCall (client:IpfsClient) (pair: MultiAddress * Cto) = async {
            let op =
                match snd pair with
                | Some(c) -> client.Bootstrap.RemoveAsync(fst pair, c)
                | None -> client.Bootstrap.RemoveAsync(fst pair)
            return! Async.AwaitTask op
        }

        // 4. surface area - public functions that build expressions to support the embedded language
        let inline add (client) = Add(addCall client)
        let inline addDefaults (client) = AddDefaults(addDefaultsCall client)
        let inline list (client) = Ls(listCall client)
        let inline removeAll (client) = RemoveAll(removeAllCall client)
        let inline remove (client) = Remove(removeCall client)

        // 5. interpreters
        let inline interpret (expression:BootstrapDSL) (args:BootstrapDSLArgs) : Async<BootstrapDSLResults> = async {
            match expression, args with
            | Add(f), AddArgs(a) -> return AddResult(f a)
            | AddDefaults(f), AddDefaultsArgs(a) -> return AddDefaultsResult(f a)
            | Ls(f), LsArgs(a) -> return LsResult(f a)
            | RemoveAll(f), RemoveAllArgs(a) -> return RemoveAllResult(f a)
            | Remove(f), RemoveArgs(a) -> return RemoveResult(f a)
            | _,_ -> return Err
        }

    type private ConfigKey = string

    module ConfigDSL =

        // 1. [x] the embedded language specification
        // 2. [x] arguments to be passed by, and results returned by interpreter as specified by the embedded language
        // 3. [x] private low-level interop "calls" that are bound to the expressions of the language
        // 4. [x] surface area - public functions that build expressions to support the embedded language
        // 5. [x] interpreters

        // 1. the embedded language specification
        type ConfigDSL =
            private
            | GetAll of settings:(Cto -> Async<JObject>)
            | GetKey of setting:((ConfigKey * Cto) -> Async<JToken>)
            | SetKeyString of setting:((ConfigKey * string * Cto) -> Async<unit>)
            | SetKeyToken of setting:((ConfigKey * JToken * Cto) -> Async<unit>)
            | Replace of settings:(JObject -> Async<unit>)

        // 2. arguments to be passed by, and results returned by interpreter as specified by the embedded language
        type ConfigDSLArgs =
            private
            | GetAllArgs of Cto
            | GetKeyArgs of (ConfigKey * Cto)
            | SetKeyStringArgs of (ConfigKey * string * Cto)
            | SetKeyTokenArgs of (ConfigKey * JToken * Cto)
            | ReplaceArgs of JObject
            static member prepareGetAll (ct) = GetAllArgs(ct)
            static member prepareGetKey (ck) (ct) = GetKeyArgs(ck, ct)
            static member prepareSetKeyString (ck) (s) (ct) = SetKeyStringArgs(ck, s, ct)
            static member prepareSetKeyToken (ck) (tk) (ct) = SetKeyTokenArgs(ck, tk, ct)
            static member prepareReplace (settings) = ReplaceArgs(settings)

        type ConfigDSLResults =
            private
            | Err
            | GetAllResult of Async<JObject>
            | GetKeyResult of Async<JToken>
            | SetKeyStringResult of Async<unit>
            | SetKeyTokenResult of Async<unit>
            | ReplaceResult of Async<unit>

        // 3. private low-level interop "calls" that are bound to the expressions of the language
        let inline private getAllCall (client:IpfsClient) (ct:Cto) = async {
            let op =
                match ct with
                | Some(c) -> client.Config.GetAsync(c)
                | None -> client.Config.GetAsync()
            return! Async.AwaitTask op
        }

        let inline private getKeyCall (client:IpfsClient) (pair: ConfigKey * Cto) = async {
            let op =
                match snd pair with
                | Some(c) -> client.Config.GetAsync(fst pair, c)
                | None -> client.Config.GetAsync(fst pair)
            return! Async.AwaitTask op
        }

        let inline private setKeyStringCall (client:IpfsClient) (t: ConfigKey * string * Cto) = async {
            let (key, value, ct) = t
            let op =
                match ct with
                | Some(c) -> client.Config.SetAsync(key, value, c)
                | None -> client.Config.SetAsync(key, value)
            return! Async.AwaitTask op
        }

        let inline private setKeyTokenCall (client:IpfsClient) (t: ConfigKey * JToken * Cto) = async {
            let (key, value, ct) = t
            let op =
                match ct with
                | Some(c) -> client.Config.SetAsync(key, value, c)
                | None -> client.Config.SetAsync(key, value)
            return! Async.AwaitTask op
        }

        let inline private replaceCall (client:IpfsClient) (settings:JObject) = async {
            return! Async.AwaitTask (client.Config.ReplaceAsync(settings))
        }

        // 4. surface area - public functions that build expressions to support the embedded language
        let inline getAll (client) = GetAll(getAllCall client)
        let inline getKey (client) = GetKey(getKeyCall client)
        let inline setKeyString (client) = SetKeyString(setKeyStringCall client)
        let inline setKeyToken (client) = SetKeyToken(setKeyTokenCall client)
        let inline replace (client) = Replace(replaceCall client)

        // 5. interpreters
        let inline interpret (expression:ConfigDSL) (args:ConfigDSLArgs) : Async<ConfigDSLResults> = async {
            match expression, args with
            | GetAll(f), GetAllArgs(a) -> return GetAllResult(f a)
            | GetKey(f), GetKeyArgs(a) -> return GetKeyResult(f a)
            | SetKeyString(f), SetKeyStringArgs(a) -> return SetKeyStringResult(f a)
            | SetKeyToken(f), SetKeyTokenArgs(a) -> return SetKeyTokenResult(f a)
            | Replace(f), ReplaceArgs(a) -> return ReplaceResult(f a)
            | _,_ -> return Err
        }

    module DagDSL =

        // 1. [x] the embedded language specification
        // 2. [x] arguments to be passed by, and results returned by interpreter as specified by the embedded language
        // 3. [x] private low-level interop "calls" that are bound to the expressions of the language
        // 4. [x] surface area - public functions that build expressions to support the embedded language
        // 5. [x] interpreters

        // 1. the embedded language specification
        type DagDSL<'Result> =
            private
            | PutJSON of object:((JObject * ContentType * HashAlgorithm * Encoding * Pin * Cto) -> Async<Cid>)
            | PutNative of object:((Object * ContentType * HashAlgorithm * Encoding * Pin * Cto) -> Async<Cid>)
            | PutStream of object:((Stream * ContentType * HashAlgorithm *Encoding * Pin * Cto) -> Async<Cid>)
            | GetJSONObject of object:((Cid * Cto) -> Async<JObject>)
            | GetJSONToken of token:((string * Cto) -> Async<JToken>)
            | GetNativeT of native:((Cid * Cto) -> Async<'Result>)

        let private flatMapAsync : ('c -> 'd) -> (Cid * Cto -> Async<'c>) -> (Cid * Cto -> Async<'d>) =
            fun g storedFx ->
                let patchable = curry storedFx
                let patched = fun pair ->
                    let applied = patchable (fst pair) (snd pair)
                    map g applied
                patched  

        let rec flatMap : ('a -> 'b) -> DagDSL<'a> -> DagDSL<'b> =
            fun f ->
                function
                | PutJSON(ob) -> PutJSON(ob)
                | PutNative(ob) -> PutNative(ob)
                | PutStream(s) -> PutStream(s)
                | GetJSONObject(o) -> GetJSONObject(o)
                | GetJSONToken(t) -> GetJSONToken(t)
                | GetNativeT(native) -> GetNativeT(flatMapAsync f native)

        type DagDSL<'Result> with
            static member Map (x:DagDSL<'Result>, f) = flatMap f x
        
        // 2. arguments to be passed by, and results returned by interpreter as specified by the embedded language
        type DagDSLArgs =
            private
            | PutJSONArgs of (JObject * ContentType * HashAlgorithm * Encoding * Pin * Cto)
            | PutNativeArgs of (Object * ContentType * HashAlgorithm * Encoding * Pin * Cto)
            | PutStreamArgs of (Stream * ContentType * HashAlgorithm * Encoding * Pin * Cto)
            | GetJSONObjectArgs of (Cid * Cto)
            | GetJSONTokenArgs of (string * Cto)
            | GetNativeTArgs of (Cid * Cto)
            static member preparePutJSON (ob) (ctype) (hash) (enc) (pin) (ct) = PutJSONArgs(ob, ctype, hash, enc, pin, ct)
            static member preparePutNative (ob) (ctype) (hash) (enc) (pin) (ct) = PutNativeArgs(ob, ctype, hash, enc, pin, ct)
            static member preparePutStream (s) (ctype) (hash) (enc) (pin) (ct) = PutStreamArgs(s, ctype, hash, enc, pin, ct)
            static member prepareGetJSONObject (id) (ct) = GetJSONObjectArgs(id, ct)
            static member prepareGetJSONToken (s) (ct) = GetJSONTokenArgs(s, ct)
            static member prepareGetNativeT (id) (ct) = GetNativeTArgs(id, ct)

        type DagDSLResults<'Native> =
            private
            | Err
            | PutJSONResult of Async<Cid>
            | PutNativeResult of Async<Cid>
            | PutStreamResult of Async<Cid>
            | GetJSONObjectResult of Async<JObject>
            | GetJSONTokenResult of Async<JToken>
            | GetNativeTResult of Async<'Native>

        // 3. private low-level interop "calls" that are bound to the expressions of the language
        let inline private putJSONCall (client:IpfsClient) (t: JObject * ContentType * HashAlgorithm * Encoding * Pin * Cto) = async {
            let (data, ctype:string, hash:string, enc:string, pin:bool, ct:Cto) = t
            let op =
                match ct with
                | Some(c) -> client.Dag.PutAsync(data, ctype, hash, enc, pin, c)
                | None -> client.Dag.PutAsync(data, ctype, hash, enc, pin)
            return! Async.AwaitTask op
        }

        let inline private putNativeCall (client:IpfsClient) (t: Object * ContentType * HashAlgorithm * Encoding * Pin * Cto) = async {
            let (data, ctype:string, hash:string, enc:string, pin:bool, ct:Cto) = t
            let op =
                match ct with
                | Some(c) -> client.Dag.PutAsync(data, ctype, hash, enc, pin, c)
                | None -> client.Dag.PutAsync(data, ctype, hash, enc, pin)
            return! Async.AwaitTask op
        }

        let inline private putStreamCall (client:IpfsClient) (t: Stream * ContentType * HashAlgorithm * Encoding * Pin * Cto) = async {
            let (data, ctype:string, hash:string, enc:string, pin:bool, ct:Cto) = t
            let op =
                match ct with
                | Some(c) -> client.Dag.PutAsync(data, ctype, hash, enc, pin, c)
                | None -> client.Dag.PutAsync(data, ctype, hash, enc, pin)
            return! Async.AwaitTask op
        }

        let inline private getJSONObjectCall (client:IpfsClient) (pair: Cid * Cto) = async {
            let op =
                match snd pair with
                | Some(c) -> client.Dag.GetAsync(fst pair, c)
                | None -> client.Dag.GetAsync(fst pair)
            return! Async.AwaitTask op
        }

        let inline private getJSONTokenCall (client:IpfsClient) (pair: string * Cto) = async {
            let op =
                match snd pair with
                | Some(c) -> client.Dag.GetAsync(fst pair,c)
                | None -> client.Dag.GetAsync(fst pair)
            return! Async.AwaitTask op
        }

        let inline private getNativeTCall<'a> (client:IpfsClient) (pair:Cid * Cto) = async {
            let op =
                match snd pair with
                | Some(c) -> client.Dag.GetAsync<'a>(fst pair, c)
                | None -> client.Dag.GetAsync<'a>(fst pair)
            return! Async.AwaitTask op
        }

        // 4. surface area - public functions that build expressions to support the embedded language
        let inline putJSON (client) = PutJSON(putJSONCall client)
        let inline putNative (client) = PutNative(putNativeCall client)
        let inline putStream (client) = PutStream(putStreamCall client)
        let inline getJSONObject (client) = GetJSONObject(getJSONObjectCall client)
        let inline getJSONToken (client) = GetJSONToken(getJSONTokenCall client)
        let inline getNativeT'<'a> (client) = DagDSL<'a>.GetNativeT(getNativeTCall<'a> client)

        // 5. interpreters
        let inline interpret (expression:DagDSL<'a>) (args:DagDSLArgs) : Async<DagDSLResults<'a>> = async {
            match expression, args with
            | PutJSON(f), PutJSONArgs(a) -> return PutJSONResult(f a)
            | PutNative(f), PutNativeArgs(a) -> return PutNativeResult(f a)
            | PutStream(f), PutStreamArgs(a) -> return PutStreamResult(f a)
            | GetJSONObject(f), GetJSONObjectArgs(a) -> return GetJSONObjectResult(f a)
            | GetJSONToken(f), GetJSONTokenArgs(a) -> return GetJSONTokenResult(f a)
            | GetNativeT(f), GetNativeTArgs(a) -> return GetNativeTResult(f a)
            | _,_ -> return Err
        }

    module DhtDSL =
        
        // 1. [x] the embedded language specification
        // 2. [x] arguments to be passed by, and results returned by interpreter as specified by the embedded language
        // 3. [x] private low-level interop "calls" that are bound to the expressions of the language
        // 4. [x] surface area - public functions that build expressions to support the embedded language
        // 5. [x] interpreters

        // 1. the embedded language specification (degenerately generic)
        type DhtDSL =
            private
            | FindPeer of lookup:((MultiHash * Cto) -> Async<Peer>)
            | FindProviders of lookup:((Cid * Cto) -> AsyncSeq<Peer>)

        // 2. arguments to be passed by, and results returned by interpreter as specified by the embedded language
        type DhtDSLArgs =
            private
            | FindPeerArgs of (MultiHash * Cto)
            | FindProvidersArgs of (Cid * Cto)
            static member prepareFindPeer (mh) (ct) = FindPeerArgs(mh, ct)
            static member prepareFindProviders (id) (ct) = FindProvidersArgs(id, ct)

        type DhtDSLResults =
            private
            | Err
            | FindPeerResult of Async<Peer>
            | FindProvidersResult of AsyncSeq<Peer>

        // 3. private low-level interop "calls" that are bound to the expressions of the language
        let inline private findPeerCall (client:IpfsClient) (pair: MultiHash * Cto) = async {
            let op =
                match snd pair with
                | Some(c) -> client.Dht.FindPeerAsync(fst pair, c)
                | None -> client.Dht.FindPeerAsync(fst pair)
            return! Async.AwaitTask op
        }
        
        let inline private findProvidersCall (client:IpfsClient) (pair:Cid * Cto) = asyncSeq {
            let op =
                match snd pair with
                | Some(c) -> client.Dht.FindProvidersAsync(fst pair, c)
                | None -> client.Dht.FindProvidersAsync(fst pair)

            let! results = Async.AwaitTask op
            for id in results do
                yield id
        }

        // 4. surface area - public functions that build expressions to support the embedded language
        let inline findPeer (client) = FindPeer(findPeerCall client)
        let inline findProviders (client) = FindProviders(findProvidersCall client)

        // 5. interpreters
        let inline interpret (expression:DhtDSL) (args:DhtDSLArgs) : Async<DhtDSLResults> = async {
            match expression, args with
            | FindPeer(f), FindPeerArgs(a) -> return FindPeerResult(f a)
            | FindProviders(f), FindProvidersArgs(a) -> return FindProvidersResult(f a)
            | _,_ -> return Err
        }

    type private Recursive = bool

    module DnsDSL =

        // 1. [x] the embedded language specification
        // 2. [x] arguments to be passed by interpreter as specified by the embedded language
        // 3. [x] private low-level interop "calls" that are bound to the expressions of the language
        // 4. [x] surface area - public functions that build expressions to support the embedded language
        // 5. [x] interpreters
        
        // 1. the embedded language specification
        type DnsDSL =
            private
            | Resolve of lookup:((string * Recursive * Cto) -> Async<string>)

        // 2. arguments to be passed by interpreter as specified by the embedded language
        type DnsDSLArgs =
            private
            | ResolveArgs of args:(string * Recursive * Cto)
            static member prepareResolve (name) (r:bool) (ct) = ResolveArgs(name, r, ct)

        // 3. private low-level interop "calls" that are bound to the expressions of the language
        let inline private resolveCall (client:IpfsClient) (t: string * Recursive * Cto) = async {
            let (name, rec', ct) = t
            let op =
                match ct with
                | Some(c) -> client.Dns.ResolveAsync(name, rec', c)
                | None -> client.Dns.ResolveAsync(name, rec')
            return! Async.AwaitTask op
        }

        // 4. surface area - public functions that build expressions to support the embedded language
        let inline resolve (client) = Resolve(resolveCall client)

        // 5. interpreters
        let inline interpret (Resolve(f)) (ResolveArgs(args)) = async {
            return! f args
        }
    
    type private FileOptions = AddFileOptions
    type private FilePath = string
    type private FileName = string
    type private IpfsFilePath = string

    module FileSystemDSL =
        
        // 1. [x] the embedded language specification
        // 2. [x] arguments to be passed by, and results returned by interpreter as specified by the embedded language
        // 3. [x] private low-level interop "calls" that are bound to the expressions of the language
        // 4. [x] surface area - public functions that build expressions to support the embedded language
        // 5. [x] interpreters

        // 1. the embedded language specification
        type FileSystemDSL =
            private
            | AddFile of store:((FilePath * FileOptions * Cto) -> Async<IFileSystemNode>)
            | AddText of store:((string * FileOptions * Cto) -> Async<IFileSystemNode>)
            | AddStream of store:((Stream * FileName * FileOptions * Cto) -> Async<IFileSystemNode>)
            | AddDirectory of store:((FilePath * Recursive * FileOptions * Cto) -> Async<IFileSystemNode>)
            | ReadAllText of retrieve:((IpfsFilePath * Cto) -> Async<string>)
            | ReadFile of retrieve:((IpfsFilePath * Cto) -> Async<Stream>)
            | Ls of info:((IpfsFilePath * Cto) -> Async<IFileSystemNode>)

        // 2. arguments to be passed by, and results returned by interpreter as specified by the embedded language
        type FileSystemDSLArgs =
            private
            | AddFileArgs of (FilePath * FileOptions * Cto)
            | AddTextArgs of (string * FileOptions * Cto)
            | AddStreamArgs of (Stream * FileName * FileOptions * Cto)
            | AddDirectoryArgs of (FilePath * Recursive * FileOptions * Cto)
            | ReadAllTextArgs of (IpfsFilePath * Cto)
            | ReadFileArgs of (IpfsFilePath * Cto)
            | LsArgs of (IpfsFilePath * Cto)
            static member prepareAddFile (fp) (fo) (ct) = AddFileArgs(fp, fo, ct)
            static member prepareAddText (s) (fo) (ct) = AddTextArgs(s, fo, ct)
            static member prepareAddStream (st) (fn) (fo) (ct) = AddStreamArgs(st, fn, fo, ct)
            static member prepareAddDirectory (fp) (r) (fo) (ct) = AddDirectoryArgs(fp, r, fo, ct)
            static member prepareReadAllText (ifp) (ct) = ReadAllTextArgs(ifp, ct)
            static member prepareReadFile (ifp) (ct) = ReadFileArgs(ifp, ct)
            static member prepareLs (ifp) (ct) = LsArgs(ifp, ct)

        type FileSystemDSLResults =
            private
            | Err
            | AddFileResult of Async<IFileSystemNode>
            | AddTextResult of Async<IFileSystemNode>
            | AddStreamResult of Async<IFileSystemNode>
            | AddDirectoryResult of Async<IFileSystemNode>
            | ReadAllTextResult of Async<string>
            | ReadFileResult of Async<Stream>
            | LsResult of Async<IFileSystemNode>

        // 3. private low-level interop "calls" that are bound to the expressions of the language
        let inline private addFileCall (client:IpfsClient) (t: FilePath * FileOptions * Cto) = async {
            let (fp, fo, ct) = t
            let op =
                match ct with
                | Some(c) -> client.FileSystem.AddFileAsync(fp, fo, c)
                | None -> client.FileSystem.AddFileAsync(fp, fo)
            return! Async.AwaitTask op
        }

        let inline private addTextCall (client:IpfsClient) (t: string * FileOptions * Cto) = async {
            let (data, fo, ct) = t
            let op =
                match ct with
                | Some(c) -> client.FileSystem.AddTextAsync(data, fo, c)
                | None -> client.FileSystem.AddTextAsync(data, fo)
            return! Async.AwaitTask op
        }

        let inline private addStreamCall (client:IpfsClient) (t: Stream * FileName * FileOptions * Cto) = async {
            let (data, fn, fo, ct) = t
            let op =
                match ct with
                | Some(c) -> client.FileSystem.AddAsync(data, fn, fo, c)
                | None -> client.FileSystem.AddAsync(data, fn, fo)
            return! Async.AwaitTask op
        }

        let inline private addDirectoryCall (client:IpfsClient) (t: FilePath * Recursive * FileOptions * Cto) = async {
            let (fp, r, fo, ct) = t
            let op =
                match ct with
                | Some(c) -> client.FileSystem.AddDirectoryAsync(fp, r, fo, c)
                | None -> client.FileSystem.AddDirectoryAsync(fp, r, fo)
            return! Async.AwaitTask op
        }

        let inline private readAllTextCall (client:IpfsClient) (pair: IpfsFilePath * Cto) = async {
            let op =
                match snd pair with
                | Some(c) -> client.FileSystem.ReadAllTextAsync(fst pair, c)
                | None -> client.FileSystem.ReadAllTextAsync(fst pair)
            return! Async.AwaitTask op
        }

        let inline private readFileCall (client:IpfsClient) (pair: IpfsFilePath * Cto) = async {
            let op =
                match snd pair with
                | Some(c) -> client.FileSystem.ReadFileAsync(fst pair, c)
                | None -> client.FileSystem.ReadFileAsync(fst pair)
            return! Async.AwaitTask op
        }

        let inline private lsCall (client:IpfsClient) (pair: IpfsFilePath * Cto) = async {
            let op =
                match snd pair with
                | Some(c) -> client.FileSystem.ListFileAsync(fst pair, c)
                | None -> client.FileSystem.ListFileAsync(fst pair)
            return! Async.AwaitTask op
        }

        // 4. surface area - public functions that build expressions to support the embedded language
        let inline addFile (client) = AddFile(addFileCall client)
        let inline addText (client) = AddText(addTextCall client)
        let inline addStream (client) = AddStream(addStreamCall client)
        let inline addDirectory (client) = AddDirectory(addDirectoryCall client)
        let inline readAllText (client) = ReadAllText(readAllTextCall client)
        let inline readFile (client) = ReadFile(readFileCall client)
        let inline ls (client) = Ls(lsCall client)

        // 5. interpreters
        let inline interpret (expression:FileSystemDSL) (args:FileSystemDSLArgs) : Async<FileSystemDSLResults> = async {
            match expression, args with
            | AddFile(f), AddFileArgs(a) -> return AddFileResult(f a)
            | AddText(f), AddTextArgs(a) -> return AddTextResult(f a)
            | AddStream(f), AddStreamArgs(a) -> return AddStreamResult(f a)
            | AddDirectory(f), AddDirectoryArgs(a) -> return AddDirectoryResult(f a)
            | ReadAllText(f), ReadAllTextArgs(a) -> return ReadAllTextResult(f a)
            | ReadFile(f), ReadFileArgs(a) -> return ReadFileResult(f a)
            | Ls(f), LsArgs(a) -> return LsResult(f a)
            | _,_ -> return Err
        }

    module GenericDSL =

        // 1. [x] the embedded language specification
        // 2. [x] arguments to be passed by, and results returned by interpreter as specified by the embedded language
        // 3. [x] private low-level interop "calls" that are bound to the expressions of the language
        // 4. [x] surface area - public functions that build expressions to support the embedded language
        // 5. [x] interpreters

        // 1. the embedded language specification
        type GenericDSL =
            private
            | NodeId of info:((MultiHash * Cto) -> Async<Peer>)
            | Resolve of info:((IpfsFilePath * Recursive * Cto) -> Async<string>)
            | Shutdown of client:(unit -> Async<unit>)
            | Version of info:(Cto -> Async<Dictionary<string,string>>)

        // 2. arguments to be passed by, and results returned by interpreter as specified by the embedded language
        type GenericDSLArgs =
            private
            | NodeIdArgs of (MultiHash * Cto)
            | ResolveArgs of (IpfsFilePath * Recursive * Cto)
            | ShutdownArgs of unit
            | VersionArgs of Cto
            static member prepareNodeId (mh) (ct) = NodeIdArgs(mh, ct)
            static member prepareResolve (ifp) (r) (ct) = ResolveArgs(ifp, r, ct)
            static member prepareShutdown () = ShutdownArgs(())
            static member prepareVersion (ct) = VersionArgs(ct)

        type GenericDSLResults =
            private
            | Err
            | NodeIdResult of Async<Peer>
            | ResolveResult of Async<string>
            | ShutdownResult of Async<unit>
            | VersionResult of Async<Dictionary<string,string>>

        // 3. private low-level interop "calls" that are bound to the expressions of the language
        let inline private nodeIdCall (client:IpfsClient) (pair: MultiHash * Cto) = async {
            let op =
                match snd pair with
                | Some(c) -> client.Generic.IdAsync(fst pair, c)
                | None -> client.Generic.IdAsync(fst pair)
            return! Async.AwaitTask op
        }

        let inline private resolveCall (client:IpfsClient) (t: IpfsFilePath * Recursive * Cto) = async {
            let (ifp, r, ct) = t
            let op =
                match ct with
                | Some(c) -> client.Generic.ResolveAsync(ifp, r, c)
                | None -> client.Generic.ResolveAsync(ifp, r)
            return! Async.AwaitTask op
        }

        let inline private shutdownCall (client:IpfsClient) (t: unit) = async {
            let op = client.Generic.ShutdownAsync()
            return! Async.AwaitTask op
        }

        let inline private versionCall (client:IpfsClient) (ct: Cto) = async {
            let op =
                match ct with
                | Some(c) -> client.Generic.VersionAsync(c)
                | None -> client.Generic.VersionAsync()
            return! Async.AwaitTask op
        }

        // 4. surface area - public functions that build expressions to support the embedded language
        let inline nodeId (client) = NodeId(nodeIdCall client)
        let inline resolve (client) = Resolve(resolveCall client)
        let inline shutdown (client) = Shutdown(shutdownCall client)
        let inline version (client) = Version(versionCall client)

        // 5. interpreters
        let interpret (expression:GenericDSL) (args:GenericDSLArgs) : Async<GenericDSLResults> = async {
            match expression, args with
            | NodeId(f), NodeIdArgs(a) -> return NodeIdResult(f a)
            | Resolve(f), ResolveArgs(a) -> return ResolveResult(f a)
            | Shutdown(f), ShutdownArgs(a) -> return ShutdownResult(f a)
            | Version(f), VersionArgs(a) -> return VersionResult(f a)
            | _,_ -> return Err
        }

    type private KeyName = string
    type private KeyType = string
    type private KeySize = int

    module KeyDSL =

        // 1. [x] the embedded language specification
        // 2. [x] arguments to be passed by, and results returned by interpreter as specified by the embedded language
        // 3. [x] private low-level interop "calls" that are bound to the expressions of the language
        // 4. [x] surface area - public functions that build expressions to support the embedded language
        // 5. [x] interpreters

        // 1. the embedded language specification
        type KeyDSL =
            private
            | Generate of keypair:((KeyName * KeyType * KeySize * Cto) -> Async<IKey> )
            | Ls of keypairs:(Cto -> AsyncSeq<IKey>)
            | Remove of keypair:((KeyName * Cto) -> Async<IKey>)

        // 2. arguments to be passed by, and results returned by interpreter as specified by the embedded language
        type KeyDSLArgs =
            private
            | GenerateArgs of (KeyName * KeyType * KeySize * Cto)
            | LsArgs of Cto
            | RemoveArgs of (KeyName * Cto)
            static member prepareGenerate (kn) (kt) (ks) (ct) = GenerateArgs(kn, kt, ks, ct)
            static member prepareLs (ct) = LsArgs(ct)
            static member prepareRemove (kn) (ct) = RemoveArgs(kn, ct)

        type KeyDSLResults =
            private
            | Err
            | GenerateResult of Async<IKey>
            | LsResult of AsyncSeq<IKey>
            | RemoveResult of Async<IKey>

        // 3. private low-level interop "calls" that are bound to the expressions of the language
        let inline private generateCall (client:IpfsClient) (t: KeyName * KeyType * KeySize * Cto) = async {
            let (kn, kt, ks, ct) = t
            let op =
                match ct with
                | Some(c) -> client.Key.CreateAsync(kn, kt, ks, c)
                | None -> client.Key.CreateAsync(kn, kt, ks)
            return! Async.AwaitTask op
        }

        let inline private lsCall (client:IpfsClient) (ct: Cto) = asyncSeq {
            let op =
                match ct with
                | Some(c) -> client.Key.ListAsync(c)
                | None -> client.Key.ListAsync()
            let! results = Async.AwaitTask op

            for key in results do
                yield key
        }

        let inline private removeCall (client:IpfsClient) (pair: KeyName * Cto) = async {
            let op =
                match snd pair with
                | Some(c) -> client.Key.RemoveAsync(fst pair, c)
                | None -> client.Key.RemoveAsync(fst pair)
            return! Async.AwaitTask op
        }

        // 4. surface area - public functions that build expressions to support the embedded language
        let inline generate (client) = Generate(generateCall client)
        let inline ls (client) = Ls(lsCall client)
        let inline remove (client) = Remove(removeCall client)

        // 5. interpreters
        let inline interpret (expression:KeyDSL) (args:KeyDSLArgs) : Async<KeyDSLResults> = async {
            match expression, args with
            | Generate(f), GenerateArgs(a) -> return GenerateResult(f a)
            | Ls(f), LsArgs(a) -> return LsResult(f a)
            | Remove(f), RemoveArgs(a) -> return RemoveResult(f a)
            | _,_ -> return Err
        }

    type private Resolve = bool
    type private TimeSpan' = TimeSpan option
    type private NoCache = bool
    type private IpfsName = string

    module NameDSL =

        // 1. [x] the embedded language specification
        // 2. [x] arguments to be passed by, and results returned by interpreter as specified by the embedded language
        // 3. [x] private low-level interop "calls" that are bound to the expressions of the language
        // 4. [x] surface area - public functions that build expressions to support the embedded language
        // 5. [x] interpreters

        // 1. the embedded language specification
        type NameDSL =
            private
            | PublishPath of name:((IpfsFilePath * Resolve * KeyName * TimeSpan' * Cto) -> Async<NamedContent>)
            | PublishCid of name:((Cid * KeyName * TimeSpan' * Cto) -> Async<NamedContent>)
            | Resolve of lookup:((IpfsName * Recursive * NoCache * Cto) -> Async<IpfsFilePath>)

        // 2. arguments to be passed by, and results returned by interpreter as specified by the embedded language
        type NameDSLArgs =
            private
            | PublishPathArgs of (IpfsFilePath * Resolve * KeyName * TimeSpan' * Cto)
            | PublishCidArgs of (Cid * KeyName * TimeSpan' * Cto)
            | ResolveArgs of (IpfsName * Recursive * NoCache * Cto)
            static member preparePublishPath (ifp) (r) (kn) (ts) (ct) = PublishPathArgs(ifp, r, kn, ts, ct)
            static member preparePublishCid (id) (kn) (ts) (ct) = PublishCidArgs(id, kn, ts, ct)
            static member prepareResolve (ifn) (r) (nc) (ct) = ResolveArgs(ifn, r, nc, ct)

        type NameDSLResults =
            private
            | Err
            | PublishPathResult of Async<NamedContent>
            | PublishCidResult of Async<NamedContent>
            | ResolveResult of Async<IpfsFilePath>

        // 3. private low-level interop "calls" that are bound to the expressions of the language
        let inline private publishPathCall (client:IpfsClient) (t: IpfsFilePath * Resolve * KeyName * TimeSpan' * Cto) = async {
            let (ifp:string, r:bool, kn:string, ts, ct) = t
            let tspan :Nullable<TimeSpan> = 
                match ts with
                | Some(t) -> System.Nullable(t)
                | None -> System.Nullable()
            let op = 
                match ct with
                | Some(c) -> client.Name.PublishAsync(ifp, r, kn, tspan, c)
                | None -> client.Name.PublishAsync(ifp, r, kn, tspan)
            return! Async.AwaitTask op
        }

        let inline private publishCidCall (client:IpfsClient) (t: Cid * KeyName * TimeSpan' * Cto) = async {
            let (id, kn:string, ts, ct) = t
            let tspan :Nullable<TimeSpan> = 
                match ts with
                | Some(t) -> System.Nullable(t)
                | None -> System.Nullable()
            let op = 
                match ct with
                | Some(c) -> client.Name.PublishAsync(id, kn, tspan, c)
                | None -> client.Name.PublishAsync(id, kn, tspan)
            return! Async.AwaitTask op
        }

        let inline private resolveCall (client:IpfsClient) (t: IpfsName * Recursive * NoCache * Cto) = async {
            let (name:string, r:bool, nc:bool, ct) = t
            let op =
                match ct with
                | Some(c) -> client.Name.ResolveAsync(name, r, nc, c)
                | None -> client.Name.ResolveAsync(name, r, nc)
           return! Async.AwaitTask op
        }

        // 4. surface area - public functions that build expressions to support the embedded language
        let inline publishPath (client) = PublishPath(publishPathCall client)
        let inline publishCid (client) = PublishCid(publishCidCall client)
        let inline resolve (client) = Resolve(resolveCall client)

        // 5. interpreters
        let inline interpret (expression:NameDSL) (args:NameDSLArgs) : Async<NameDSLResults> = async {
            match expression, args with
            | PublishPath(f), PublishPathArgs(a) -> return PublishPathResult(f a)
            | PublishCid(f), PublishCidArgs(a) -> return PublishCidResult(f a)
            | Resolve(f), ResolveArgs(a) -> return ResolveResult(f a)
            | _,_ -> return Err
        }

    type private ObjectTemplate = string
    type private MerkleLinks = IMerkleLink seq

    module ObjectDSL =

        // 1. [x] the embedded language specification
        // 2. [x] arguments to be passed by, and results returned by interpreter as specified by the embedded language
        // 3. [x] private low-level interop "calls" that are bound to the expressions of the language
        // 4. [x] surface area - public functions that build expressions to support the embedded language
        // 5. [x] interpreters

        // 1. the embedded language specification
        type ObjectDSL =
            private
            | NewDirectory of create:(Cto -> Async<DagNode>)
            | NewFromTemplate of create:((ObjectTemplate * Cto) -> Async<DagNode>)
            | GetNode of retrieve:((Cid * Cto) -> Async<DagNode>)
            | PutBytes of construct:((byte[] * MerkleLinks * Cto) -> Async<DagNode>)
            | PutNode of construct:((DagNode * Cto) -> Async<DagNode>)
            | GetData of fetch:((Cid * Cto) -> Async<Stream>)
            | GetLinks of fetch:((Cid * Cto) -> AsyncSeq<IMerkleLink>)

        // 2. arguments to be passed by, and results returned by interpreter as specified by the embedded language
        type ObjectDSLArgs =
            private
            | NewDirectoryArgs of Cto
            | NewFromTemplateArgs of (ObjectTemplate * Cto)
            | GetNodeArgs of (Cid * Cto)
            | PutBytesArgs of (byte[] * MerkleLinks * Cto)
            | PutNodeArgs of (DagNode * Cto)
            | GetDataArgs of (Cid * Cto)
            | GetLinksArgs of (Cid * Cto)
            static member prepareNewDirectory (ct) = NewDirectoryArgs(ct)
            static member prepareNewFromTemplate (tem) (ct) = NewFromTemplateArgs(tem, ct)
            static member prepareGetNode (id) (ct) = GetNodeArgs(id, ct)
            static member preparePutBytes (data) (mlinks) (ct) = PutBytesArgs(data, mlinks, ct)
            static member preparePutNode (dn) (ct) = PutNodeArgs(dn, ct)
            static member prepareGetData (id) (ct) = GetDataArgs(id, ct)
            static member prepareGetLinks (id) (ct) = GetLinksArgs(id, ct)

        type ObjectDSLResults =
            private
            | Err
            | NewDirectoryResult of Async<DagNode>
            | NewFromTemplateResult of Async<DagNode>
            | GetNodeResult of Async<DagNode>
            | PutBytesResult of Async<DagNode>
            | PutNodeResult of Async<DagNode>
            | GetDataResult of Async<Stream>
            | GetLinksResult of AsyncSeq<IMerkleLink>

        // 3. private low-level interop "calls" that are bound to the expressions of the language
        let inline private newDirectoryCall (client:IpfsClient) (ct: Cto) = async {
            let op =
                match ct with
                | Some(c) -> client.Object.NewDirectoryAsync(c)
                | None -> client.Object.NewDirectoryAsync()
            return! Async.AwaitTask op
        }

        let inline private newFromTemplateCall (client:IpfsClient) (pair: ObjectTemplate * Cto) = async {
            let op =
                match snd pair with
                | Some(c) -> client.Object.NewAsync(fst pair, c)
                | None -> client.Object.NewAsync(fst pair)
            return! Async.AwaitTask op
        }

        let inline private getNodeCall (client:IpfsClient) (pair: Cid * Cto) = async {
            let op =
                match snd pair with
                | Some(c) -> client.Object.GetAsync(fst pair, c)
                | None -> client.Object.GetAsync(fst pair)
            return! Async.AwaitTask op
        }

        let inline private putBytesCall (client:IpfsClient) (t: byte[] * MerkleLinks * Cto) = async {
            let (data, ml, ct) = t
            let op =
                match ct with
                | Some(c) -> client.Object.PutAsync(data, ml, c)
                | None -> client.Object.PutAsync(data, ml)
            return! Async.AwaitTask op
        }

        let inline private putNodeCall (client:IpfsClient) (pair: DagNode * Cto) = async {
            let op =
                match snd pair with
                | Some(c) -> client.Object.PutAsync(fst pair, c)
                | None -> client.Object.PutAsync(fst pair)
            return! Async.AwaitTask op
        }

        let inline private getDataCall (client:IpfsClient) (pair: Cid * Cto) = async {
            let op =
                match snd pair with
                | Some(c) -> client.Object.DataAsync(fst pair, c)
                | None -> client.Object.DataAsync(fst pair)
            return! Async.AwaitTask op
        }

        let inline private getLinksCall (client:IpfsClient) (pair: Cid * Cto) = asyncSeq {
            let op =
                match snd pair with
                | Some(c) -> client.Object.LinksAsync(fst pair, c)
                | None -> client.Object.LinksAsync(fst pair)
            let! results = Async.AwaitTask op

            for link in results do
                yield link
        }

        // 4. surface area - public functions that build expressions to support the embedded language
        let inline newDirectory (client) = NewDirectory(newDirectoryCall client)
        let inline newFromTemplate (client) = NewFromTemplate(newFromTemplateCall client)
        let inline getNode (client) = GetNode(getNodeCall client)
        let inline putBytes (client) = PutBytes(putBytesCall client)
        let inline putNode (client) = PutNode(putNodeCall client)
        let inline getData (client) = GetData(getDataCall client)
        let inline getLinks (client) = GetLinks(getLinksCall client)

        // 5. interpreters
        let interpret (expression:ObjectDSL) (args:ObjectDSLArgs) : Async<ObjectDSLResults> = async {
            match expression, args with
            | NewDirectory(f), NewDirectoryArgs(a) -> return NewDirectoryResult(f a)
            | NewFromTemplate(f), NewFromTemplateArgs(a) -> return NewFromTemplateResult(f a)
            | GetNode(f), GetNodeArgs(a) -> return GetNodeResult(f a)
            | PutBytes(f), PutBytesArgs(a) -> return PutBytesResult(f a)
            | PutNode(f), PutNodeArgs(a) -> return PutNodeResult(f a)
            | GetData(f), GetDataArgs(a) -> return GetDataResult(f a)
            | GetLinks(f), GetLinksArgs(a) -> return GetLinksResult(f a)
            | _,_ -> return Err
        }

    module PinDSL =

        // 1. [x] the embedded language specification
        // 2. [ ] arguments to be passed by, and results returned by interpreter as specified by the embedded language
        // 3. [ ] private low-level interop "calls" that are bound to the expressions of the language
        // 4. [ ] surface area - public functions that build expressions to support the embedded language
        // 5. [ ] interpreters

        // 1. the embedded language specification
        type PinDSL =
            private
            | Add of pin:((IpfsFilePath * Recursive * Cto) -> AsyncSeq<Cid>)
            | Ls of pins:(Cto -> AsyncSeq<Cid>)
            | Remove of pin:((Cid * Recursive * Cto) -> AsyncSeq<Cid>)

    type private Topic = string
    type private Message = string

    module PubSubDSL =

        // 1. [x] the embedded language specification
        // 2. [ ] arguments to be passed by, and results returned by interpreter as specified by the embedded language
        // 3. [ ] private low-level interop "calls" that are bound to the expressions of the language
        // 4. [ ] surface area - public functions that build expressions to support the embedded language
        // 5. [ ] interpreters

        // 1. the embedded language specification
        type PubSubDSL =
            private
            | Ls of subscribed:(Cto -> AsyncSeq<Topic>)
            | Peers of subscribed:((Topic * Cto) -> AsyncSeq<Peer>)
            | Publish of channel:((Topic * Message * Cto) -> Async<unit>)
            | Subscribe of channel:((Topic * Action<IPublishedMessage> * Cto) -> Async<unit>)
    
    type private Persist = bool option

    module SwarmDSL =

        // 1. [x] the embedded language specification
        // 2. [ ] arguments to be passed by, and results returned by interpreter as specified by the embedded language
        // 3. [ ] private low-level interop "calls" that are bound to the expressions of the language
        // 4. [ ] surface area - public functions that build expressions to support the embedded language
        // 5. [ ] interpreters

        // 1. the embedded language specification
        type SwarmDSL =
            private
            | Addressess of localpeers:(Cto -> AsyncSeq<Peer>)
            | Peers of globalpeers:(Cto -> AsyncSeq<Peer>)
            | Connect of peer:((MultiAddress * Cto) -> Async<unit>)
            | Disconnect of peer:((MultiAddress * Cto) -> Async<unit>)
            | AddAddressFilter of filter:((MultiAddress * Persist * Cto) -> Async<MultiAddress>)
            | ListAddressFilter of filters:((Persist * Cto) -> AsyncSeq<MultiAddress>)
            | RemoveAddressFilter of filter:((MultiAddress * Persist * Cto) -> Async<MultiAddress>)

    [<AutoOpen>]
    module IpfsClientDSL =
        open BitswapDSL
        open BlockDSL
        open BootstrapDSL
        open ConfigDSL
        open DagDSL
        open DhtDSL
        open FileSystemDSL
        open GenericDSL
        open KeyDSL
        open NameDSL
        open ObjectDSL
        open PinDSL
        open PubSubDSL
        open SwarmDSL
        
        type IpfsClientDSLExpression<'Result> =
            | BitswapProcedure    of BitswapDSL
            | BlockProcedure      of BlockDSL
            | BootstrapProcedure  of BootstrapDSL
            | ConfigProcedure     of ConfigDSL
            | DagProcedure        of DagDSL<'Result>
            | DhtProcedure        of DhtDSL
            | FileSystemProcedure of FileSystemDSL
            | GenericProcedure    of GenericDSL
            | KeyProcedure        of KeyDSL
            | NameProcedure       of NameDSL
            | ObjectProcedure     of ObjectDSL
            | PinProcedure        of PinDSL
            | PubSubProcedure     of PubSubDSL
            | SwarmProcedure      of SwarmDSL
            | Sequential          of current:IpfsClientDSLExpression<'Result> * prev:IpfsClientDSLExpression<'Result>

        let rec flatMap : ('a -> 'b) -> (IpfsClientDSLExpression<'a>) -> (IpfsClientDSLExpression<'b>) =
            fun f ->
                function
                | BitswapProcedure(proc)    -> BitswapProcedure(proc)
                | BlockProcedure(proc)      -> BlockProcedure(proc)
                | BootstrapProcedure(proc)  -> BootstrapProcedure(proc)
                | ConfigProcedure(proc)     -> ConfigProcedure(proc)
                | DagProcedure(proc)        -> DagProcedure(DagDSL.flatMap f proc)
                | DhtProcedure(proc)        -> DhtProcedure(proc)
                | FileSystemProcedure(proc) -> FileSystemProcedure(proc)
                | GenericProcedure(proc)    -> GenericProcedure(proc)
                | KeyProcedure(proc)        -> KeyProcedure(proc)
                | NameProcedure(proc)       -> NameProcedure(proc)
                | ObjectProcedure(proc)     -> ObjectProcedure(proc)
                | PinProcedure(proc)        -> PinProcedure(proc)
                | PubSubProcedure(proc)     -> PubSubProcedure(proc)
                | SwarmProcedure(proc)      -> SwarmProcedure(proc)
                | Sequential(current, prev) -> Sequential(flatMap f current, flatMap f prev)

        /// combines two statements by first running the right one, and then the left one
        let (<+) : (IpfsClientDSLExpression<'a>) -> (IpfsClientDSLExpression<'a>) -> (IpfsClientDSLExpression<'a>) =
            fun s1 s2 -> Sequential(s1, s2)

        /// combines two statements by first running the left one, and then the right one
        let (+>) : (IpfsClientDSLExpression<'a>) -> (IpfsClientDSLExpression<'a>) -> (IpfsClientDSLExpression<'a>) =
            fun s1 s2 -> Sequential(s2, s1)
        
        type IpfsClientDSLExpression<'Result> with
            static member Map (x:IpfsClientDSLExpression<'Result>, f) = flatMap f x
        
        /// the "free monad", also called a program, this little recursive structure models all possible
        /// execution scenarios of using the IPFS API with the embedded langauge, more precisely,
        /// the following two can occur:
        /// (1) the program you write never terminates, and gets stuck in an infinite recursive loop
        /// (2) the program iterates recursively until it reaches a value, which it returns and terminates
        type FreeIpfsClient<'a> =
            /// Free, the recursive step,
            /// a statement in the embedded language about the next step in the program
            | Free of IpfsClientDSLExpression<FreeIpfsClient<'a>>

            /// Pure, the final state,
            /// a pure value returned by the program
            | Pure of 'a

        /// binds fsharp (and embedded) expressions into programs of the embedded language 
        let rec bindFree : ('a -> FreeIpfsClient<'b>) -> FreeIpfsClient<'a> -> FreeIpfsClient<'b> =
            fun f ->
                function
                | Free(clientDSL) -> Free(flatMap (bindFree f) clientDSL)
                | Pure(expression) -> f expression

        /// lifts expressions of the embedded language into programs of the embedded language
        let liftFree : IpfsClientDSLExpression<'a> -> FreeIpfsClient<'a> =
            fun statement -> Free (flatMap Pure statement)
        
        type FreeIpfsClient<'a> with
            static member Bind (x:FreeIpfsClient<'a>, f) = bindFree f x

        type FreeIpfsClientBuilder() =
            member this.Return     x = Pure x
            member this.ReturnFrom x = x
            member this.Zero      () = Pure ()
            member this.Bind (ma, f) = bindFree f ma

        let ipfsClient = FreeIpfsClientBuilder()

        let create = monad { return! IpfsClient() }

        /// given an instance of IPFS client and a continuation program,
        /// yields control flow to the continuation after bootstraping the client to use default peers
        let usingDefaultPeers (client:IpfsClient) (continuation:FreeIpfsClient<'a>) = ipfsClient {
            let useDefaultPeers = (BootstrapProcedure(addDefaults client))
            let combined = 
                match continuation with
                // sequencing step
                | Free(cont) -> Free(useDefaultPeers +> cont)
                // reductive step
                | Pure(expression) -> Pure(expression)
            return! combined 
        }

        let findingProviders (client:IpfsClient) = ipfsClient {
            return! liftFree (DhtProcedure(findProviders client))
        }

        let createWithDefaultPeers (continuation:FreeIpfsClient<'a>) = ipfsClient {
            let client = create
            return! usingDefaultPeers client continuation
        }