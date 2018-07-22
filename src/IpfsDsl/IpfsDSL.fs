namespace Ipfs.DSL

[<AutoOpen>]
module IpfsDSL =
    open SubDSLs
        
    type IpfsDSLResult<'R> =
        | BitswapR    of BitswapDSLResult
        | BlockR      of BlockDSLResult
        | BootstrapR  of BootstrapDSLResult
        | ConfigR     of ConfigDSLResult
        | DagR        of DagDSLResult<'R>
        | DhtR        of DhtDSLResult
        | FileSystemR of FileSystemDSLResult
        | GenericR    of GenericDSLResult
        | KeyR        of KeyDSLResult
        | NameR       of NameDSLResult
        | ObjectR     of ObjectDSLResult
        | PinR        of PinDSLResult
        | PubSubR     of PubSubDSLResult
        | SwarmR      of SwarmDSLResult
        static member bind (result:BitswapDSLResult) = BitswapR(result)
        static member bind (result:BlockDSLResult) = BlockR(result)
        static member bind (result:BootstrapDSLResult) = BootstrapR(result)
        static member bind (result:ConfigDSLResult) = ConfigR(result)
        static member bind (result:DagDSLResult<'R>) = DagR(result)
        static member bind (result:DhtDSLResult) = DhtR(result)
        static member bind (result:FileSystemDSLResult) = FileSystemR(result)
        static member bind (result:GenericDSLResult) = GenericR(result)
        static member bind (result:KeyDSLResult) = KeyR(result)
        static member bind (result:NameDSLResult) = NameR(result)
        static member bind (result:ObjectDSLResult) = ObjectR(result)
        static member bind (result:PinDSLResult) = PinR(result)
        static member bind (result:PubSubDSLResult) = PubSubR(result)
        static member bind (result:SwarmDSLResult) = SwarmR(result)

    type IpfsDSL<'Cont,'R> =
        | BitswapProcedure    of expr:BitswapDSL    * args:BitswapDSLArgs      * cback:(IpfsDSLResult<'R> -> 'Cont)
        | BlockProcedure      of expr:BlockDSL      * args:BlockDSLArgs        * cback:(IpfsDSLResult<'R> -> 'Cont)
        | BootstrapProcedure  of expr:BootstrapDSL  * args:BootstrapDSLArgs    * cback:(IpfsDSLResult<'R> -> 'Cont)
        | ConfigProcedure     of expr:ConfigDSL     * args:ConfigDSLArgs       * cback:(IpfsDSLResult<'R> -> 'Cont)
        | DagProcedure        of expr:DagDSL<'R>    * args:DagDSLArgs          * cback:(IpfsDSLResult<'R> -> 'Cont)
        | DhtProcedure        of expr:DhtDSL        * args:DhtDSLArgs          * cback:(IpfsDSLResult<'R> -> 'Cont)
        | FileSystemProcedure of expr:FileSystemDSL * args:FileSystemDSLArgs   * cback:(IpfsDSLResult<'R> -> 'Cont)
        | GenericProcedure    of expr:GenericDSL    * args:GenericDSLArgs      * cback:(IpfsDSLResult<'R> -> 'Cont)
        | KeyProcedure        of expr:KeyDSL        * args:KeyDSLArgs          * cback:(IpfsDSLResult<'R> -> 'Cont)
        | NameProcedure       of expr:NameDSL       * args:NameDSLArgs         * cback:(IpfsDSLResult<'R> -> 'Cont)
        | ObjectProcedure     of expr:ObjectDSL     * args:ObjectDSLArgs       * cback:(IpfsDSLResult<'R> -> 'Cont)
        | PinProcedure        of expr:PinDSL        * args:PinDSLArgs          * cback:(IpfsDSLResult<'R> -> 'Cont)
        | PubSubProcedure     of expr:PubSubDSL     * args:PubSubDSLArgs       * cback:(IpfsDSLResult<'R> -> 'Cont)
        | SwarmProcedure      of expr:SwarmDSL      * args:SwarmDSLArgs        * cback:(IpfsDSLResult<'R> -> 'Cont)

    let rec flatMapR : ('a -> 'b) -> (IpfsDSL<'Cont,'a>) -> (IpfsDSL<'Cont,'b>) =
        fun f ->
            function
            | BitswapProcedure(proc, args, g)    -> BitswapProcedure(proc, args, g)
            | BlockProcedure(proc, args, g)      -> BlockProcedure(proc, args, g)
            | BootstrapProcedure(proc, args, g)  -> BootstrapProcedure(proc, args, g)
            | ConfigProcedure(proc, args, g)     -> ConfigProcedure(proc, args, g)
            | DagProcedure(proc, args, g)        -> DagProcedure(DagDSL.flatMap f proc, args, g)
            | DhtProcedure(proc, args, g)        -> DhtProcedure(proc, args, g)
            | FileSystemProcedure(proc, args, g) -> FileSystemProcedure(proc, args, g)
            | GenericProcedure(proc, args, g)    -> GenericProcedure(proc, args, g)
            | KeyProcedure(proc, args, g)        -> KeyProcedure(proc, args, g)
            | NameProcedure(proc, args, g)       -> NameProcedure(proc, args, g)
            | ObjectProcedure(proc, args, g)     -> ObjectProcedure(proc, args, g)
            | PinProcedure(proc, args, g)        -> PinProcedure(proc, args, g)
            | PubSubProcedure(proc, args, g)     -> PubSubProcedure(proc, args, g)
            | SwarmProcedure(proc, args, g)      -> SwarmProcedure(proc, args, g)

    let rec flatMap : ('a -> 'b) -> (IpfsDSL<'a,'R>) -> (IpfsDSL<'b,'R>) =
        fun f ->
            function
            | BitswapProcedure(proc, args, g)    -> BitswapProcedure(proc, args, g >> f)
            | BlockProcedure(proc, args, g)      -> BlockProcedure(proc, args, g >> f)
            | BootstrapProcedure(proc, args, g)  -> BootstrapProcedure(proc, args, g >> f)
            | ConfigProcedure(proc, args, g)     -> ConfigProcedure(proc, args, g >> f)
            | DagProcedure(proc, args, g)        -> DagProcedure(proc, args, g >> f)
            | DhtProcedure(proc, args, g)        -> DhtProcedure(proc, args, g >> f)
            | FileSystemProcedure(proc, args, g) -> FileSystemProcedure(proc, args, g >> f)
            | GenericProcedure(proc, args, g)    -> GenericProcedure(proc, args, g >> f)
            | KeyProcedure(proc, args, g)        -> KeyProcedure(proc, args, g >> f)
            | NameProcedure(proc, args, g)       -> NameProcedure(proc, args, g >> f)
            | ObjectProcedure(proc, args, g)     -> ObjectProcedure(proc, args, g >> f)
            | PinProcedure(proc, args, g)        -> PinProcedure(proc, args, g >> f)
            | PubSubProcedure(proc, args, g)     -> PubSubProcedure(proc, args, g >> f)
            | SwarmProcedure(proc, args, g)      -> SwarmProcedure(proc, args, g >> f)
        
    type IpfsDSLArgs =
        private
        | BitswapA    of BitswapDSLArgs
        | BlockA      of BlockDSLArgs
        | BootstrapA  of BootstrapDSLArgs
        | ConfigA     of ConfigDSLArgs
        | DagA        of DagDSLArgs
        | DhtA        of DhtDSLArgs
        | FileSystemA of FileSystemDSLArgs
        | GenericA    of GenericDSLArgs
        | KeyA        of KeyDSLArgs
        | NameA       of NameDSLArgs
        | ObjectA     of ObjectDSLArgs
        | PinA        of PinDSLArgs
        | PubSubA     of PubSubDSLArgs
        | SwarmA      of SwarmDSLArgs
        static member bind (args:BitswapDSLArgs) = BitswapA(args)
        static member bind (args:BlockDSLArgs) = BlockA(args)
        static member bind (args:BootstrapDSLArgs) = BootstrapA(args)
        static member bind (args:ConfigDSLArgs) = ConfigA(args)
        static member bind (args:DagDSLArgs) = DagA(args)
        static member bind (args:DhtDSLArgs) = DhtA(args)
        static member bind (args:FileSystemDSLArgs) = FileSystemA(args)
        static member bind (args:GenericDSLArgs) = GenericA(args)
        static member bind (args:KeyDSLArgs) = KeyA(args)
        static member bind (args:NameDSLArgs) = NameA(args)
        static member bind (args:ObjectDSLArgs) = ObjectA(args)
        static member bind (args:PinDSLArgs) = PinA(args)
        static member bind (args:PubSubDSLArgs) = PubSubA(args)
        static member bind (args:SwarmDSLArgs) = SwarmA(args)
        
    /// the "free monad", also called a program, this little recursive structure models all possible
    /// execution scenarios of using the IPFS API with the embedded langauge, more precisely,
    /// the following two can occur:
    /// (1) the program you write never terminates, and gets stuck in an infinite recursive loop
    /// (2) the program iterates recursively until it reaches a value, which it returns and terminates
    type IpfsClientProgram<'Cont,'R> =
        /// Free, the recursive step,
        /// a statement in the embedded language about the next step in the program
        | Free of IpfsDSL<IpfsClientProgram<'Cont,'R>,'R>

        /// Return, the final state,
        /// a pure value returned by the program
        | Return of 'Cont

    /// binds fsharp (and embedded) expressions into programs of the embedded language 
    let rec bindFree : ('a -> IpfsClientProgram<'b,'r>) -> IpfsClientProgram<'a,'r> -> IpfsClientProgram<'b,'r> =
        fun f ->
            function
            | Free(clientDSL) -> Free(flatMap (bindFree f) clientDSL)
            | Return(expression) -> f expression

    /// lifts expressions of the embedded language into programs of the embedded language
    let liftFree : IpfsDSL<'a,'r> -> IpfsClientProgram<'a,'r> =
        fun statement -> Free (flatMap Return statement)

    type IpfsClientProgramBuilder() =
        member this.Return              x = Return x
        member this.ReturnFrom          x = x
        member this.Zero               () = Return ()
        member this.Bind          (ma, f) = bindFree f ma
        member this.Delay (f: unit -> 'a) = f
        member this.Run               (f) = f()

    let ipfs = IpfsClientProgramBuilder()

    let rec run : ('Cont -> Async<unit>) -> (IpfsClientProgram<'Cont,'r>) -> Async<unit> =
        fun receiver free -> async {
            match free with

            | Return(a) -> return! receiver a

            | Free(BitswapProcedure(p, a, cont)) -> 
                let! partialResult = BitswapDSL.interpret p a
                let contArg = IpfsDSLResult<_>.bind partialResult
                let nextProgram = cont contArg
                return! run receiver nextProgram

            | Free(BlockProcedure(p, a, cont)) ->
                let! partialResult = BlockDSL.interpret p a
                let contArg = IpfsDSLResult<_>.bind partialResult
                let nextProgram = cont contArg
                return! run receiver nextProgram

            | Free(BootstrapProcedure(p, a, cont)) ->
                let! partialResult = BootstrapDSL.interpret p a
                let contArg = IpfsDSLResult<_>.bind partialResult
                let nextProgram = cont contArg
                return! run receiver nextProgram

            | Free(ConfigProcedure(p, a, cont)) ->
                let! partialResult = ConfigDSL.interpret p a
                let contArg = IpfsDSLResult<_>.bind partialResult
                let nextProgram = cont contArg
                return! run receiver nextProgram

            | Free(DagProcedure(p, a, cont)) ->
                let! partialResult = DagDSL.interpret p a
                let contArg = IpfsDSLResult<_>.bind partialResult
                let nextProgram = cont contArg
                return! run receiver nextProgram

            | Free(DhtProcedure(p, a, cont)) ->
                let! partialResult = DhtDSL.interpret p a
                let contArg = IpfsDSLResult<_>.bind partialResult
                let nextProgram = cont contArg
                return! run receiver nextProgram

            | Free(FileSystemProcedure(p, a, cont)) ->
                let! partialResult = FileSystemDSL.interpret p a
                let contArg = IpfsDSLResult<_>.bind partialResult
                let nextProgram = cont contArg
                return! run receiver nextProgram

            | Free(GenericProcedure(p, a, cont)) ->
                let! partialResult = GenericDSL.interpret p a
                let contArg = IpfsDSLResult<_>.bind partialResult
                let nextProgram = cont contArg
                return! run receiver nextProgram

            | Free(KeyProcedure(p, a, cont)) ->
                let! partialResult = KeyDSL.interpret p a
                let contArg = IpfsDSLResult<_>.bind partialResult
                let nextProgram = cont contArg
                return! run receiver nextProgram

            | Free(NameProcedure(p, a, cont)) ->
                let! partialResult = NameDSL.interpret p a
                let contArg = IpfsDSLResult<_>.bind partialResult
                let nextProgram = cont contArg
                return! run receiver nextProgram

            | Free(ObjectProcedure(p, a, cont)) ->
                let! partialResult = ObjectDSL.interpret p a
                let contArg = IpfsDSLResult<_>.bind partialResult
                let nextProgram = cont contArg
                return! run receiver nextProgram
                
            | Free(PinProcedure(p, a, cont)) ->
                let! partialResult = PinDSL.interpret p a
                let contArg = IpfsDSLResult<_>.bind partialResult
                let nextProgram = cont contArg
                return! run receiver nextProgram

            | Free(PubSubProcedure(p, a, cont)) ->
                let! partialResult = PubSubDSL.interpret p a
                let contArg = IpfsDSLResult<_>.bind partialResult
                let nextProgram = cont contArg
                return! run receiver nextProgram

            | Free(SwarmProcedure(p, a, cont)) ->
                let! partialResult = SwarmDSL.interpret p a
                let contArg = IpfsDSLResult<_>.bind partialResult
                let nextProgram = cont contArg
                return! run receiver nextProgram
        }