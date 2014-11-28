using NanoByte.Common.Tasks;
using ZeroInstall.Services;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;

class MinimalZeroInstall : ServiceLocator
{
    public static void Main(string[] args)
    {
        new MinimalZeroInstall().Run(new Requirements {InterfaceUri = new FeedUri(args[0])});
    }

    private MinimalZeroInstall() : base(new CliTaskHandler())
    {}

    private void Run(Requirements requirements)
    {
        var selections = Solver.Solve(requirements);
        var missing = SelectionsManager.GetUncachedImplementations(selections);
        Fetcher.Fetch(missing);
        Executor.Start(selections);
    }
}
