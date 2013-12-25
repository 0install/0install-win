using ZeroInstall.Backend;
using ZeroInstall.Model;
using ZeroInstall.Injector;

class MinimalZeroInstall : ServiceLocator
{
    public static void Main(string[] args)
    {
        new MinimalZeroInstall().Run(new Requirements {InterfaceID = args[0]});
    }

    private MinimalZeroInstall() : base(new CliHandler())
    {}

    private void Run(Requirements requirements)
    {
        var selections = Solver.Solve(requirements);
        var missing = SelectionsManager.GetUncachedImplementations(selections);
        Fetcher.Fetch(missing);
        new Executor(selections, Store).Start();
    }
}
