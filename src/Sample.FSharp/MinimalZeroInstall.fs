open ZeroInstall.Backend
open ZeroInstall.Model
open ZeroInstall.Injector

let resolver = new Resolver(new CliHandler())
let solve = resolver.Solver.Solve
let uncached = resolver.SelectionsManager.GetUncachedImplementations
let fetch = resolver.Fetcher.Fetch
let execute selections = (new Executor (selections, resolver.Store)).Start()

let run requirements =
    let selections = solve requirements
    fetch (uncached selections)
    execute selections

[<EntryPoint>]
let main args =
    ignore(run(new Requirements(InterfaceID = args.[0])))
    0