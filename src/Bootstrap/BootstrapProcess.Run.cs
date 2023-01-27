// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Diagnostics;
using NanoByte.Common.Native;
using ZeroInstall.Model.Selection;
using ZeroInstall.Services;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store.Configuration;

namespace ZeroInstall;

partial class BootstrapProcess
{
    /// <summary>
    /// Runs Zero Install as an <see cref="ITask"/>.
    /// </summary>
    private ExitCode RunZeroInstall(params string[] args)
    {
        var startInfo = ZeroInstall(args);

        var exitCode = ExitCode.UserCanceled;
        _handler.RunTask(new SimpleTask(
            $"Integrating {_embeddedConfig.AppName}",
            () => exitCode = (ExitCode)startInfo.Run()));

        return exitCode;
    }

    /// <summary>
    /// Runs Zero Install and hides the Bootstrap GUI.
    /// </summary>
    private ExitCode SwitchToZeroInstall(params string[] args)
    {
        var process = ZeroInstall(args).Start();

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
    private ProcessStartInfo ZeroInstall(string[] args)
    {
        // Forwards bootstrapper arguments that are also applicable to 0install
        args = _handler.Verbosity switch
        {
            Verbosity.Batch => args.Prepend("--batch"),
            Verbosity.Verbose => args.Prepend("--verbose"),
            Verbosity.Debug => args.Prepend("--verbose").Prepend("--verbose"),
            _ => args
        };
        if (args.FirstOrDefault() != "central")
        {
            if (_handler.Background) args = args.Prepend("--background");
            if (Config.NetworkUse == NetworkLevel.Offline) args = args.Prepend("--offline");
            if (FeedManager.Refresh) args = args.Prepend("--refresh");
        }

        return ZeroInstallDeployed(args) ?? ZeroInstallCached(args);
    }

    /// <summary>
    /// Returns process start information for a deployed instance of Zero Install.
    /// </summary>
    public ProcessStartInfo? ZeroInstallDeployed(params string[] args)
    {
        if (_version != null) return null;

        var deployedInstance = GetDeployedInstance();
        if (string.IsNullOrEmpty(deployedInstance)) return null;

        string launchAssembly = _handler.IsGui ? "0install-win" : "0install";
        if (!File.Exists(Path.Combine(deployedInstance, launchAssembly + ".exe"))) return null;

        return ProcessUtils.Assembly(Path.Combine(deployedInstance, launchAssembly), args);
    }

    private static string? GetDeployedInstance() => WindowsUtils.IsWindows ? RegistryUtils.GetSoftwareString("Zero Install", "InstallLocation") : null;

    private Requirements? _requirements;
    private Selections? _selections;

    /// <summary>
    /// Returns process start information for a cached (downloaded) instance of Zero Install.
    /// </summary>
    public ProcessStartInfo ZeroInstallCached(params string[] args)
    {
        // To keep things simple, we never try to use the external solver to get Zero Install itself
        Config.ExternalSolverUri = null;

        _requirements = new(Config.SelfUpdateUri ?? new(Config.DefaultSelfUpdateUri), _handler.IsGui ? Command.NameRunGui : Command.NameRun);
        if (_version != null) _requirements.ExtraRestrictions[_requirements.InterfaceUri] = _version;

        try
        {
            _selections = Solver.Solve(_requirements!);

            if (FeedManager.ShouldRefresh)
            {
                Log.Info("Found solution, but cache is stale; trying refresh");
                FeedManager.Refresh = true;
                if (Solver.TrySolve(_requirements!) is {} selections) _selections = selections;
                else Log.Warn("Continuing with stale solution");
            }
        }
        catch (SolverException ex) when (_version != null && !FeedManager.Refresh)
        {
            Log.Info($"Solving for version {_version} failed, possibly because feed is outdated; trying refresh", ex);
            FeedManager.Refresh = true;
            _selections = Solver.Solve(_requirements!);
        }

        try
        {
            Fetch();
        }
        catch (WebException ex) when (_version == null)
        {
            Log.Warn("Unable to download selected version, trying to find cached version", ex);
            _selections = TrySolveOffline(_requirements!);
            if (_selections == null) throw;
        }

        return Executor.Inject(_selections!)
                       .AddArguments(args)
                       .ToStartInfo();
    }

    /// <summary>
    /// Downloads any implementations selected by the Solver that are not in the cache yet.
    /// </summary>
    private void Fetch()
    {
        foreach (var implementation in SelectionsManager.GetUncachedImplementations(_selections!))
            Fetcher.Fetch(implementation);
    }
}
