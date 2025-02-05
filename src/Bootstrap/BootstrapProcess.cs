// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Diagnostics;
using NanoByte.Common.Native;
using ZeroInstall.Services;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Store.Configuration;

namespace ZeroInstall;

/// <summary>
/// Represents the process of downloading and running an instance of Zero Install. Handles command-line arguments.
/// </summary>
public sealed partial class BootstrapProcess : ServiceProvider
{
    private readonly IBootstrapHandler _handler;

    /// <summary>
    /// Executes the bootstrap process controlled by command-line arguments.
    /// </summary>
    /// <param name="args">The command-line arguments to handle.</param>
    /// <returns>The exit status code to end the process with.</returns>
    public ExitCode Execute(IEnumerable<string> args)
    {
        LoadConfig();
        _userArgs.Add(_options.Parse(args));
        if (_machineWide && !WindowsUtils.IsAdministrator) throw new NotAdminException();

        if (BootstrapConfig.Instance.CustomizableStorePath && !_prepareOffline) CustomizeStorePath();

        SaveConfig();
        TrustKeys();

        ImportEmbedded();
        ImportDirectory();

        if (BootstrapConfig.Instance.AppUri == null) ApplySharedOptions();
        if (_offline) Config.NetworkUse = NetworkLevel.Offline;

        if (_prepareOffline) return ExecuteExport();
        var exitCode = ExecuteIntegrate();
        return exitCode == ExitCode.OK ? ExecuteRun() : exitCode;
    }

    /// <summary>
    /// Runs <c>0install export</c>.
    /// </summary>
    private ExitCode ExecuteExport()
    {
        var args = new List<string> {"export", "--refresh"};

        if (BootstrapConfig.Instance.AppUri is {} appUri)
        {
            args.Add(appUri.ToStringRfc());
            if (_appVersion != null) args.Add($"--version={_appVersion}");
            args.Add("--include-zero-install");
        }
        else
        {
            args.Add(Config.SelfUpdateUri?.ToStringRfc() ?? Config.DefaultSelfUpdateUri);
            if (_version != null) args.Add($"--version={_version}");
        }
        args.Add(Locations.InstallBase);

        return SwitchToZeroInstall(args);
    }

    /// <summary>
    /// Runs <c>0install integrate</c> if requested by <see cref="BootstrapConfig"/>.
    /// </summary>
    private ExitCode ExecuteIntegrate()
    {
        if (BootstrapConfig.Instance is {AppUri: {} appUri, IntegrateArgs: {} integrateArgs} && !_noIntegrate)
        {
            var args = new List<string> { "integrate", appUri.ToStringRfc(), "--no-download" };
            if (_machineWide) args.Add("--machine");
            args.Add(WindowsUtils.SplitArgs(_integrateArgs ?? integrateArgs));

            if (ZeroInstallDeployment.FindOther(_machineWide) is {} deployedInstance)
                EnsureRequestedZeroInstallVersion(deployedInstance);

            var startInfo = ZeroInstall(args);
            return _handler.RunTaskAndReturn(ResultTask.Create(
                $"Integrating {BootstrapConfig.Instance.AppName}",
                () => (ExitCode)startInfo.Run()));
        }

        return ExitCode.OK;
    }

    /// <summary>
    /// Checks if a deployed instance of Zero Install matches the requested <see cref="_version"/> and updates it if not.
    /// </summary>
    private void EnsureRequestedZeroInstallVersion(string path)
    {
        if (_version == null) return;

        string versionFile = Path.Combine(path, "0install.exe");
        if (!File.Exists(versionFile)) return;

        var deployedVersion = new ImplementationVersion(FileVersionInfo.GetVersionInfo(versionFile).ProductVersion.GetLeftPartAtFirstOccurrence('+'));
        if (!_version.Match(deployedVersion))
        {
            Log.Info($"Zero Install instance deployed at {path} has version {deployedVersion} that does not match desired version {_version}");
            var startInfo = ZeroInstallCached(["self", "deploy", "--batch", path]);
            _handler.RunTask(new ActionTask("Updating Zero Install", () => startInfo.Run()));
        }
    }

    /// <summary>
    /// Runs <c>0install</c> with <c>run</c>, <c>central</c> or other user-provided arguments.
    /// </summary>
    private ExitCode ExecuteRun()
    {
        var args = new List<string>();

        if (BootstrapConfig.Instance is {AppUri: {} appUri, AppArgs: var appArgs})
        {
            if (_noRun)
                args.AddRange(["download", appUri.ToStringRfc()]);
            else
            {
                args.Add("run");
                if (_appVersion != null) args.AddRange(["--version", _appVersion.ToString()]);
                if (_handler.IsGui && !_wait) args.Add("--no-wait");
                args.Add(appUri.ToStringRfc());
                args.Add(WindowsUtils.SplitArgs(appArgs));
            }
        }

        args.Add(_userArgs);

        if (args is [])
        {
            if (_handler.IsGui) args.Add("central");
            else
            {
                _handler.Output("Help", HelpText);
                return ExitCode.UserCanceled;
            }
        }

        return SwitchToZeroInstall(args);
    }
}
