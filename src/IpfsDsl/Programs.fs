namespace Ipfs.DSL.ExamplePrograms

[<AutoOpen>]
module Programs =
    open System.IO
    open Ipfs.CoreApi
    open Ipfs.Api
    open Ipfs.DSL
    open Ipfs

    let usingDefaultPeersProgram (client:IpfsClient) (receiver:'a -> Async<unit>) (cont:IpfsClientProgram<'a,'b>) = ipfs {
        let command = 
            BootstrapProcedure(
                BootstrapDSL.addDefaults client,
                BootstrapDSLArgs.prepareAddDefaults Cancellation.dontUse,
                fun _ -> IpfsDSL.run receiver cont)
        return! liftFree command
    }

    let addFileStreamProgram (client:IpfsClient) (file:Stream) (name:string) (options:AddFileOptions) (receiver:Cid -> Async<unit>) (cont:Cid -> IpfsClientProgram<Async<unit>,'b>)= ipfs {
        let command =
            FileSystemProcedure(
                FileSystemDSL.addStream client,
                FileSystemDSLArgs.prepareAddStream file name options Cancellation.dontUse,
                fun r ->
                    match r with

                    | FileSystemR(AddStreamResult(futureNode)) -> async {
                        let! node = futureNode
                        let cont' = cont node.Id
                        do! receiver node.Id
                        do! IpfsDSL.run (fun x -> x) cont'}

                    | _ -> async { return ()})
        return! liftFree command
    }

    let readStreamProgram (client:IpfsClient) (id:Cid) (receiver: Stream -> Async<unit>) = ipfs {
        let command =
            BlockProcedure(
                BlockDSL.get client,
                BlockDSLArgs.prepareGet id Cancellation.dontUse,
                fun r ->
                    match r with
                    
                    | BlockR(GetResult(futureDataBlock)) -> async {
                        let! dataBlock = futureDataBlock
                        do! receiver dataBlock.DataStream}
                        
                    | _ -> async { return ()})
        return! liftFree command
    }

    let writeReadStreamProgram (client:IpfsClient) (file:Stream) (name:string) = ipfs {
        let mutable id : Cid = null

        let store cid = async { do id <- cid }
        let work a = async { printf "%A" id }

        let stateConfigurator = fun someWriter -> 
            usingDefaultPeersProgram client work someWriter

        let reader = fun cid ->
            readStreamProgram client cid work

        let writer = addFileStreamProgram client file name (AddFileOptions()) store
        let writerThenReader = writer reader
        
        return! stateConfigurator writerThenReader
    }
