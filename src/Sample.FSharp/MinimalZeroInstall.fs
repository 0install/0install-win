open ZeroInstall.Backend
open ZeroInstall.Model
open ZeroInstall.Injector

let resolver = new Resolver(new CliHandler())
let solve = resolver.Solver.Solve
let uncached = resolver.SelectionsManager.GetUncachedImplementations
let fetch = resolver.Fetcher.Fetch
let execute selections = (new Executor (selections, resolver.Store)).Start()

[<EntryPoint>]
let main args = 
    let requirements = new Requirements(InterfaceID = args.[0])
    let selections = solve requirements
    fetch (uncached selections)
    ignore (execute selections)
    0