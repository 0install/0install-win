import ZeroInstall.Backend
import ZeroInstall.Model
import ZeroInstall.Injector

def run(resolver as Resolver, requirements):
    selections = resolver.Solver.Solve(requirements)
    missing = resolver.SelectionsManager.GetUncachedImplementations(selections)
    resolver.Fetcher.Fetch(missing)
    Executor(selections, resolver.Store).Start()

def Main(argv as (string)):
    run(
        Resolver(CliHandler()),
        Requirements(InterfaceID:argv[0]))