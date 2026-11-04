// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Diagnostics;
using NanoByte.Common.Native;
using ZeroInstall.Model.Selection;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store.Configuration;

namespace ZeroInstall;

partial class BootstrapProcess
{
    /// <summary>
    /// Runs Zero Install and hides the Bootstrap GUI.
    /// </summary>
    private ExitCode SwitchToZeroInstall(IList<string> args)
    {
        var process = GetStartInfo(args).Start();

        if (_handler.IsGui && WindowsUtils.IsWindows)
        {
            // Wait for 0install window to become visible before closing bootstrapper window (for a smoother visual transition)
            try
            {
                process.WaitForInputIdle(milliseconds: 5000);
            }
            catch (InvalidOperationException)
            {}
        }
        _handler.Dispose();

        return (ExitCode)process.WaitForExitCode();
    }

    /// <summary>
    /// Returns process start information for an instance of Zero Install.
    /// </summary>
    private ProcessStartInfo GetStartInfo(IList<string> args)
    {
        AddSharedOptions(args);
        return ZeroInstallDeployed(args) ?? ZeroInstallCached(args);
    }

    /// <summary>
    /// Returns process start information for a deployed instance of Zero Install.
    /// </summary>
    private ProcessStartInfo? ZeroInstallDeployed(IEnumerable<string> args)
        => _version == null
        && ZeroInstallDeployment.FindOther(_machineWide) is {} deployedInstance
        && File.Exists(Path.Combine(deployedInstance, LaunchAssembly + ".exe"))
            ? ProcessUtils.Assembly(Path.Combine(deployedInstance, LaunchAssembly), args.ToArray())
            : null;

    private string LaunchAssembly => _handler.IsGui ? "0install-win" : "0install";

    /// <summary>
    /// Returns process start information for a cached (downloaded) instance of Zero Install.
    /// </summary>
    private ProcessStartInfo ZeroInstallCached(IEnumerable<string> args)
    {
        var requirements = new Requirements(SelfUpdateUri, _handler.IsGui ? Command.NameRunGui : Command.NameRun);
        if (_version != null) requirements.ExtraRestrictions[requirements.InterfaceUri] = _version;

        Selections selections;
        try
        {
            selections = Solver.Solve(requirements);

            if (FeedManager.ShouldRefresh)
            {
                Log.Info("Found solution, but cache is stale; trying refresh");
                FeedManager.Refresh = true;
                if (Solver.TrySolve(requirements) is {} refreshedSelections) selections = refreshedSelections;
                else Log.Warn("Continuing with stale solution");
            }
        }
        catch (SolverException ex) when (_version != null && !FeedManager.Refresh && Config.NetworkUse != NetworkLevel.Offline)
        {
            Log.Info($"Solving for version {_version} failed, possibly because feed is outdated; trying refresh", ex);
            FeedManager.Refresh = true;
            selections = Solver.Solve(requirements);
        }

        try
        {
            foreach (var implementation in SelectionsManager.GetUncachedImplementations(selections))
                Fetcher.Fetch(implementation);
        }
        catch (WebException ex) when (_version == null)
        {
            Log.Warn("Unable to download selected version, trying to find cached version", ex);
            if (TrySolveOffline(requirements) is not {} offlineSelections) throw;
            selections = offlineSelections;
        }

        return Executor.Inject(selections)
                       .AddArguments(args.ToArray())
                       .ToStartInfo();
    }
}
