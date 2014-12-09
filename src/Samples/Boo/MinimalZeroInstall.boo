import NanoByte.Common.Tasks
import ZeroInstall.Services
import ZeroInstall.Store.Model

def Main(argv as (string)):
    services = ServiceLocator(CliTaskHandler())

    selections = services.Solver.Solve(Requirements(argv[0]))
    missing = services.SelectionsManager.GetUncachedImplementations(selections)
    services.Fetcher.Fetch(missing)
    services.Executor.Start(selections)
