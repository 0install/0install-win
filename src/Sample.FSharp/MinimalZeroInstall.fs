open ZeroInstall.Model
open ZeroInstall.Injector
open ZeroInstall.Injector.Solver
 
let run requirements =
   let policy = Policy.CreateDefault(new CliHandler())
   let selections = policy.Solve(requirements)
   let missing = selections.GetUncachedImplementations(policy)
   policy.Fetch(missing)
   let executor = new Executor(selections, policy.Fetcher.Store)
   executor.Start()

[<EntryPoint>]
let main args = 
   let requirements = new Requirements(InterfaceID = args.[0])
   let proc = run requirements
   0 // exit code