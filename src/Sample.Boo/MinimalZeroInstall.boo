import ZeroInstall.Backend
import ZeroInstall.Model
import ZeroInstall.Injector

def Main(argv as (string)):
    requirements = Requirements(InterfaceID:argv[0])

    resolver = Resolver(CliHandler())
    selections = resolver.Solver.Solve(requirements)
    missing = resolver.SelectionsManager.GetUncachedImplementations(selections)
    resolver.Fetcher.Fetch(missing)
    Executor(selections, resolver.Store).Start()
