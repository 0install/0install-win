import ZeroInstall.Model
import ZeroInstall.Injector
import ZeroInstall.Injector.Solver
 
def Main(args as (string)):
    run(Requirements(InterfaceID:args[0]))
 
def run(requirements):
    policy = Policy.CreateDefault(CliHandler())
    selections = policy.Solve(requirements)
    missing = SelectionsUtils.GetUncachedImplementations(selections, policy)
    policy.Fetch(missing)
    Executor(selections, policy.Fetcher.Store).Start()