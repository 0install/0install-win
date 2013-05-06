using ZeroInstall.Model;
using ZeroInstall.Injector;

internal static class MinimalZeroInstall
{
    public static void Main(string[] args)
    {
        Run(
            new Resolver(new CliHandler()),
            new Requirements {InterfaceID = args[0]});
    }

    private static void Run(Resolver resolver, Requirements requirements)
    {
        var selections = resolver.Solver.Solve(requirements);
        var missing = resolver.SelectionsManager.GetUncachedImplementations(selections);
        resolver.Fetcher.Fetch(missing);
        new Executor(selections, resolver.Store).Start();
    }
}
