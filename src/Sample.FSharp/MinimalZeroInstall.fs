open ZeroInstall.Model
open ZeroInstall.Injector

let resolver = new Resolver(new CliHandler())
let solve = resolver.Solver.Solve
let uncached = resolver.SelectionsManager.GetUncachedImplementations
let fetch = resolver.Fetcher.Fetch
let execute selections = ignore ((new Executor (selections, resolver.Store)).Start())

[<EntryPoint>]
let main args = 
    let selections = solve (new Requirements(InterfaceID = args.[0]))
    fetch (uncached selections)
    execute selections
    0