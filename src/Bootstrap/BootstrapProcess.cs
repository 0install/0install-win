// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using NanoByte.Common.Native;
using ZeroInstall.Services;
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
        UpdateSelfUpdateUri();

        // Read settings from .exe.config file, if present
        Config.ReadFromAppSettings();

        // Write potentially customized config to the user profile
        // NOTE: This must be done before parsing command-line options, since that may apply non-persistent modifications to the config.
        Config.Save();

        _userArgs.Add(_options.Parse(args));
        if (_machineWide && !WindowsUtils.IsAdministrator) throw new NotAdminException("You must be an administrator to perform machine-wide operations.");
        if (_embeddedConfig.AppUri == null) ShareArgsWithZeroInstall();
        if (_embeddedConfig.CustomizablePath) CustomizePath();

        TrustKeys();
        ImportContent();

        if (_prepareOffline)
        {
            return _embeddedConfig.AppUri == null
                ? SwitchToZeroInstall("export", Config.SelfUpdateUri?.ToStringRfc() ?? Config.DefaultSelfUpdateUri, Locations.InstallBase)
                : SwitchToZeroInstall("export", "--include-zero-install", _embeddedConfig.AppUri.ToStringRfc(), Locations.InstallBase);
        }

        var exitCode = ExecuteIntegrate();
        return exitCode == ExitCode.OK ? ExecuteRun() : exitCode;
    }

    /// <summary>
    /// Runs <c>0install integrate</c> if requested by <see cref="_embeddedConfig"/>.
    /// </summary>
    private ExitCode ExecuteIntegrate()
    {
        if (_noIntegrate || _embeddedConfig is {AppUri: null} or {IntegrateArgs: null})
            return ExitCode.OK;

        var args = new List<string> {"integrate", _embeddedConfig.AppUri.ToStringRfc(), "--no-download"};
        if (_machineWide) args.Add("--machine");
        args.Add(WindowsUtils.SplitArgs(_embeddedConfig.IntegrateArgs));

        return RunZeroInstall(args.ToArray());
    }

    /// <summary>
    /// Runs <c>0install</c> with <c>run</c>, <c>central</c> or other user-provided arguments.
    /// </summary>
    private ExitCode ExecuteRun()
    {
        var args = new List<string>();

        if (_embeddedConfig is {AppUri: not null, AppName: not null})
        {
            args.Add(_noRun ? "download" : "run");

            if (!_noRun)
            {
                if (_version != null) args.Add(new [] {"--version", _version.ToString()});
                if (_handler.IsGui) args.Add("--no-wait");
            }

            args.Add(_embeddedConfig.AppUri.ToStringRfc());

            if (!_noRun)
            {
                string[] appArgs = WindowsUtils.SplitArgs(_embeddedConfig.AppArgs);
                if (appArgs.Length != 0)
                {
                    args.Add("--");
                    args.Add(appArgs);
                }
            }
        }

        args.Add(_userArgs);

        if (args.Count == 0)
        {
            if (_handler.IsGui) args.Add("central");
            else
            {
                _handler.Output("Help", HelpText);
                return ExitCode.UserCanceled;
            }
        }

        return SwitchToZeroInstall(args.ToArray());
    }
}
