open ZeroInstall.Model
open ZeroInstall.Injector
open ZeroInstall.Injector.Solver

let policy = Policy.CreateDefault(new CliHandler())

let select requirements = policy.Solve requirements

let download requirements =
    let selections = select requirements
    policy.Fetch (selections.GetUncachedImplementations(policy))
    selections

let run requirements =
    let selections = download requirements
    (new Executor (selections, policy.Fetcher.Store)).Start()

[<EntryPoint>]
let main args = 
    ignore (run (new Requirements(InterfaceID = args.[0])))
    0