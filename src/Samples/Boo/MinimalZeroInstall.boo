import NanoByte.Common.Tasks
import ZeroInstall.Services
import ZeroInstall.Store.Model

def Main(argv as (string)):
    requirements = Requirements(InterfaceID:argv[0])

    services = ServiceLocator(CliTaskHandler())
    selections = services.Solver.Solve(requirements)
    missing = services.SelectionsManager.GetUncachedImplementations(selections)
    services.Fetcher.Fetch(missing)
    services.Executor.Start(selections)
