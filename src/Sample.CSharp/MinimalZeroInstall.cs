using ZeroInstall.Injector;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;

static class MinimalZeroInstall
{
    public static void Main(string[] args)
    {
        Run(new Requirements {InterfaceID = args[0]});
    }

    private static void Run(Requirements requirements)
    {
        var policy = Policy.CreateDefault(new CliHandler());
        var selections = policy.Solve(requirements);
        var missing = selections.GetUncachedImplementations(policy);
        policy.Fetch(missing);
        new Executor(selections, policy.Fetcher.Store).Start();
    }
}
