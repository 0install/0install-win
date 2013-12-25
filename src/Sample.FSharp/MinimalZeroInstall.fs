open ZeroInstall.Backend
open ZeroInstall.Model
open ZeroInstall.Injector

let locator = new ServiceLocator(new CliHandler())
let solve = locator.Solver.Solve
let uncached = locator.SelectionsManager.GetUncachedImplementations
let fetch = locator.Fetcher.Fetch
let execute selections = (new Executor (selections, locator.Store)).Start()

let run requirements =
    let selections = solve requirements
    fetch (uncached selections)
    execute selections

[<EntryPoint>]
let main args =
    ignore(run(new Requirements(InterfaceID = args.[0])))
    0