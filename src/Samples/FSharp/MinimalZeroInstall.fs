open ZeroInstall.Services
open ZeroInstall.Store
open ZeroInstall.Store.Model

let locator = new ServiceLocator(new CliServiceHandler())
let solve = locator.Solver.Solve
let uncached = locator.SelectionsManager.GetUncachedImplementations
let fetch = locator.Fetcher.Fetch
let execute = locator.Executor.Start

let run requirements =
    let selections = solve requirements
    fetch (uncached selections)
    execute selections

[<EntryPoint>]
let main args =
    ignore(run(new Requirements(InterfaceID = args.[0])))
    0