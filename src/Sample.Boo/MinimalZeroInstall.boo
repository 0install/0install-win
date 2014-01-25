import ZeroInstall
import ZeroInstall.Model
import ZeroInstall.Injector

def Main(argv as (string)):
    requirements = Requirements(InterfaceID:argv[0])

    locator = ServiceLocator(CliHandler())
    selections = locator.Solver.Solve(requirements)
    missing = locator.SelectionsManager.GetUncachedImplementations(selections)
    locator.Fetcher.Fetch(missing)
    Executor(selections, locator.Store).Start()
