open NanoByte.Common.Tasks
open ZeroInstall.Services
open ZeroInstall.Store.Model

let services = new ServiceLocator(new CliTaskHandler())
let solve = services.Solver.Solve
let uncached = services.SelectionsManager.GetUncachedImplementations
let fetch = services.Fetcher.Fetch
let execute = services.Executor.Start

let run requirements =
    let selections = solve requirements
    fetch (uncached selections)
    execute selections

[<EntryPoint>]
let main args =
    ignore(run(new Requirements(args.[0])))
    0