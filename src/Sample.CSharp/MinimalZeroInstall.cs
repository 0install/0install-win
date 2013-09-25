using ZeroInstall.Backend;
using ZeroInstall.Injector;
using ZeroInstall.Model;

internal static class MinimalZeroInstall
{
    public static void Main(string[] args)
    {
        var resolver = new Resolver(new CliHandler());
        var requirements = new Requirements {InterfaceID = args[0]};

        var selections = resolver.Solver.Solve(requirements);
        var missing = resolver.SelectionsManager.GetUncachedImplementations(selections);
        resolver.Fetcher.Fetch(missing);
        new Executor(selections, resolver.Store).Start();
    }
}
