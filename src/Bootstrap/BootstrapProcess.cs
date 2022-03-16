// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NanoByte.Common.Streams;
using NanoByte.Common.Tasks;
using NDesk.Options;
using ZeroInstall.Archives.Extractors;
using ZeroInstall.Model;
using ZeroInstall.Model.Selection;
using ZeroInstall.Services;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Store.Configuration;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Trust;

namespace ZeroInstall
{
    /// <summary>
    /// Represents the process of downloading and running an instance of Zero Install. Handles command-line arguments.
    /// </summary>
    public sealed class BootstrapProcess : ServiceProvider
    {
        private readonly EmbeddedConfig _embeddedConfig = EmbeddedConfig.Load();

        #region Command-line options
        private readonly bool _gui;

        /// <summary>The command-line argument parser used to evaluate user input.</summary>
        private readonly OptionSet _options;

        /// <summary>The path of a Zero Install instance already deployed on this machine, if any.</summary>
        private string? _deployedInstance = WindowsUtils.IsWindows ? RegistryUtils.GetSoftwareString("Zero Install", "InstallLocation") : null;

        /// <summary>A specific version of Zero Install to download.</summary>
        private VersionRange? _version;

        /// <summary>A directory to search for feeds and archives to import.</summary>
        private string _contentDir = Path.Combine(Locations.InstallBase, "content");

        /// <summary>Arguments passed through to the target process.</summary>
        private readonly List<string> _userArgs = new();

        /// <summary>Do not run the application after downloading it.</summary>
        private bool _noRun;

        private string HelpText
        {
            get
            {
                string exeName = Path.GetFileNameWithoutExtension(new Uri(Assembly.GetEntryAssembly()!.CodeBase).LocalPath);

                using var buffer = new MemoryStream();
                var writer = new StreamWriter(buffer);
                if (_embeddedConfig is {AppUri: not null, AppName: not null})
                {
                    writer.WriteLine("This bootstrapper downloads and {0} {1} using Zero Install.", _embeddedConfig.IntegrateArgs == null ? "runs" : "integrates", _embeddedConfig.AppName);
                    writer.WriteLine("Usage: {0} [OPTIONS] [[--] APP-ARGS]", exeName);
                    writer.WriteLine();
                    writer.WriteLine("Samples:");
                    writer.WriteLine("  {0}               Run {1}.", exeName, _embeddedConfig.AppName);
                    writer.WriteLine("  {0} --offline     Run {1} without downloading anything.", exeName, _embeddedConfig.AppName);
                    writer.WriteLine("  {0} -x            Run {1} with argument '-x'.", exeName, _embeddedConfig.AppName);
                    writer.WriteLine("  {0} -- --offline  Run {1} with argument '--offline'.", exeName, _embeddedConfig.AppName);
                }
                else
                {
                    writer.WriteLine("This bootstrapper downloads and runs Zero Install.");
                    writer.WriteLine("Usage: {0} [OPTIONS] [[--] 0INSTALL-ARGS]", exeName);
                    writer.WriteLine();
                    writer.WriteLine("Samples:");
                    writer.WriteLine("  {0} self deploy  Deploy Zero Install to this computer.", exeName);
                    writer.WriteLine("  {0} central      Open main Zero Install GUI.", exeName);
                    writer.WriteLine("  {0} run vlc      Run VLC via Zero Install.", exeName);
                    writer.WriteLine("  {0} -- --help    Show help for Zero Install instead of Bootstrapper.", exeName);
                }
                writer.WriteLine();
                writer.WriteLine("Options:");
                _options.WriteOptionDescriptions(writer);
                writer.Flush();
                return buffer.ReadToString();
            }
        }

        /// <summary>
        /// Creates a new bootstrap process.
        /// </summary>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about download and IO tasks.</param>
        /// <param name="gui"><c>true</c> if the application was launched in GUI mode; <c>false</c> if it was launched in command-line mode.</param>
        public BootstrapProcess(ITaskHandler handler, bool gui)
            : base(handler)
        {
            _gui = gui;

            _options = new OptionSet
            {
                {
                    "?|h|help", () => "Show the built-in help text.", _ =>
                    {
                        Handler.Output("Help", HelpText);
                        throw new OperationCanceledException(); // Don't handle any of the other arguments
                    }
                },
                {
                    "batch", () => "Automatically answer questions with defaults when possible. Avoid unnecessary console output (e.g. progress bars).", _ =>
                    {
                        if (Handler.Verbosity >= Verbosity.Verbose) throw new OptionException("Cannot combine --batch and --verbose", "verbose");
                        Handler.Verbosity = Verbosity.Batch;
                    }
                },
                {
                    "v|verbose", () => "More verbose output. Use twice for even more verbose output.", _ =>
                    {
                        if (Handler.Verbosity == Verbosity.Batch) throw new OptionException("Cannot combine --batch and --verbose", "batch");
                        Handler.Verbosity++;
                    }
                },
                {
                    "no-existing", () => "Do not detect and use existing Zero Install instances. Always use downloaded and cached instance.", _ => _deployedInstance = null
                },
                {
                    "version=", () => "Select a specific {VERSION} of Zero Install. Implies --no-existing.", (VersionRange range) =>
                    {
                        _version = range;
                        _deployedInstance = null;
                    }
                },
                {
                    "feed=", () => "Specify an alternative {FEED} for Zero Install. Must be an absolute URI. Implies --no-existing.", feed =>
                    {
                        Config.SelfUpdateUri = new(feed);
                        _deployedInstance = null;
                    }
                },
                {
                    "content-dir=", () => "Specifies a {DIRECTORY} to search for feeds and archives to import. The default is a directory called 'content'.", path =>
                    {
                        if (!Directory.Exists(path)) throw new DirectoryNotFoundException($"Directory '{path}' not found.");
                        _contentDir = path;
                    }
                },
                {
                    "o|offline", () => "Run in off-line mode, not downloading anything.", _ => Config.NetworkUse = NetworkLevel.Offline
                }
            };

            if (_embeddedConfig is {AppUri: not null, AppName: not null})
            {
                _options.Add("no-run", () => $"Do not run {_embeddedConfig.AppName} after downloading it.", _ => _noRun = true);
                _options.Add("s|silent", () => "Equivalent to --no-run --batch.", _ =>
                {
                    _noRun = true;
                    Handler.Verbosity = Verbosity.Batch;
                });
            }

            // Work-around to disable interspersed arguments (needed for passing arguments through to sub-processes)
            _options.Add("<>", value =>
            {
                _userArgs.Add(value);

                // Stop using options parser, treat everything from here on as unknown
                _options.Clear();
            });
        }
        #endregion

        /// <summary>
        /// Executes the bootstrap process controlled by command-line arguments.
        /// </summary>
        /// <param name="args">The command-line arguments to handle.</param>
        /// <returns>The exit status code to end the process with.</returns>
        public ExitCode Execute(IEnumerable<string> args)
        {
            // Aggressively restore default self-update URI (e.g., to "fix" broken deployments)
            if (_embeddedConfig.SelfUpdateUri == null)
                Config.SelfUpdateUri = new FeedUri(Config.DefaultSelfUpdateUri);
            // Only apply custom self-update URI if 0install isn't deployed yet and the URI hasn't already been customized
            else if (Config.SelfUpdateUri == new FeedUri(Config.DefaultSelfUpdateUri) && _deployedInstance == null)
                Config.SelfUpdateUri = _embeddedConfig.SelfUpdateUri;

            // Write potentially customized config to the user profile
            // NOTE: This must be done before parsing command-line options, since that may apply non-persistent modifications to the config.
            Config.Save();

            _userArgs.AddRange(_options.Parse(args));
            if (_embeddedConfig.AppUri == null) ShareArgsWithZeroInstall();

            TrustKeys();
            ImportContent();

            var exitCode = RunZeroInstallIntegrate();
            return exitCode == ExitCode.OK ? RunZeroInstall() : exitCode;
        }

        /// <summary>
        /// Handles arguments passed to 0install that are also applicable to the bootstrapper.
        /// </summary>
        private void ShareArgsWithZeroInstall()
        {
            foreach (string arg in _userArgs)
            {
                switch (arg)
                {
                    case "--batch":
                        Handler.Verbosity = Verbosity.Batch;
                        break;
                    case "--verbose" or "-v":
                        Handler.Verbosity = Verbosity.Verbose;
                        break;
                    case "--offline" or "-o":
                        Config.NetworkUse = NetworkLevel.Offline;
                        break;
                }
            }
        }

        /// <summary>
        /// Adds keys for Zero Install (and optionally an app) to the <see cref="TrustDB"/>.
        /// </summary>
        private void TrustKeys()
        {
            try
            {
                var trust = TrustDB.Load();
                trust.TrustKey("88C8A1F375928691D7365C0259AA3927C24E4E1E", new Domain("apps.0install.net"));
                if (_embeddedConfig is {AppUri: not null, AppName: not null, AppFingerprint: not null})
                    trust.TrustKey(_embeddedConfig.AppFingerprint, new Domain(_embeddedConfig.AppUri.Host));
                trust.Save();
            }
            #region Error handling
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            #endregion
        }

        /// <summary>
        /// Imports bundled feeds and implementations/archives.
        /// </summary>
        private void ImportContent()
        {
            if (!Directory.Exists(_contentDir)) return;

            foreach (string path in Directory.GetFiles(_contentDir, "*.xml"))
            {
                Log.Info($"Importing feed from {path}");
                try
                {
                    FeedManager.ImportFeed(path);
                }
                #region Error handling
                catch (ReplayAttackException)
                {
                    Log.Info("Ignored feed because a newer version is already in cache");
                }
                #endregion
            }

            foreach (string path in Directory.GetFiles(_contentDir))
            {
                var manifestDigest = new ManifestDigest();
                manifestDigest.ParseID(Path.GetFileNameWithoutExtension(path));
                if (manifestDigest.Best != null)
                {
                    Log.Info($"Importing implementation {manifestDigest.Best} from {path}");
                    try
                    {
                        var extractor = ArchiveExtractor.For(Archive.GuessMimeType(path), Handler);
                        ImplementationStore.Add(manifestDigest, builder => Handler.RunTask(new ReadFile(path, stream => extractor.Extract(builder, stream))));
                    }
                    #region Error handling
                    catch (ImplementationAlreadyInStoreException)
                    {}
                    #endregion
                }
            }
        }

        /// <summary>
        /// Runs <c>0install integrate</c> if requested by <see cref="_embeddedConfig"/>.
        /// </summary>
        private ExitCode RunZeroInstallIntegrate()
        {
            if (_embeddedConfig is {AppUri: null} or {IntegrateArgs: null})
                return ExitCode.OK;

            var startInfo = ZeroInstall(
                new[] {"integrate", _embeddedConfig.AppUri.ToStringRfc()}
                   .Concat(WindowsUtils.SplitArgs(_embeddedConfig.IntegrateArgs))
                   .ToArray());

            var exitCode = ExitCode.UserCanceled;
            Handler.RunTask(new SimpleTask(
                $"Integrating {_embeddedConfig.AppName}",
                () => exitCode = (ExitCode)startInfo.Run()));
            return exitCode;
        }

        /// <summary>
        /// Runs <c>0install</c> with <c>run</c>, <c>central</c> or other user-provided arguments.
        /// </summary>
        private ExitCode RunZeroInstall()
        {
            var args = new List<string>();

            if (_embeddedConfig is {AppUri: not null, AppName: not null})
            {
                string appUri = _embeddedConfig.AppUri.ToStringRfc();
                if (_noRun)
                    args.AddRange(new[] {"download", appUri});
                else
                {
                    args.AddRange(new[] {"run", appUri});
                    string[] appArgs = WindowsUtils.SplitArgs(_embeddedConfig.AppArgs);
                    if (appArgs.Length != 0)
                    {
                        args.Add("--");
                        args.AddRange(appArgs);
                    }
                }
            }

            args.AddRange(_userArgs);

            if (args.Count == 0)
            {
                if (_gui) args.Add("central");
                else
                {
                    Handler.Output("Help", HelpText);
                    return ExitCode.UserCanceled;
                }
            }

            var startInfo = ZeroInstall(args.ToArray());
            Handler.Dispose(); // Close Bootstrap UI
            return (ExitCode)startInfo.Run();
        }

        /// <summary>
        /// Returns process start information for an instance of Zero Install.
        /// </summary>
        private ProcessStartInfo ZeroInstall(params string[] args)
        {
            // Forwards bootstrapper arguments that are also applicable to 0install
            args = Handler.Verbosity switch
            {
                Verbosity.Batch => args.Prepend("--batch"),
                Verbosity.Verbose => args.Prepend("--verbose"),
                Verbosity.Debug => args.Prepend("--verbose").Prepend("--verbose"),
                _ => args
            };
            if (Config.NetworkUse == NetworkLevel.Offline)
                args = args.Prepend("--offline");

            return ZeroInstallDeployed(args) ?? ZeroInstallCached(args);
        }

        /// <summary>
        /// Returns process start information for a deployed instance of Zero Install.
        /// </summary>
        public ProcessStartInfo? ZeroInstallDeployed(params string[] args)
        {
            if (!string.IsNullOrEmpty(_deployedInstance))
            {
                string launchAssembly = _gui ? "0install-win" : "0install";
                if (File.Exists(Path.Combine(_deployedInstance, launchAssembly + ".exe")))
                    return ProcessUtils.Assembly(Path.Combine(_deployedInstance, launchAssembly), args);
            }
            return null;
        }

        private Requirements? _requirements;
        private Selections? _selections;

        /// <summary>
        /// Returns process start information for a cached (downloaded) instance of Zero Install.
        /// </summary>
        public ProcessStartInfo ZeroInstallCached(params string[] args)
        {
            _requirements = new Requirements(Config.SelfUpdateUri ?? new(Config.DefaultSelfUpdateUri), _gui ? Command.NameRunGui : Command.NameRun);
            if (_version != null) _requirements.ExtraRestrictions[_requirements.InterfaceUri] = _version;

            Solve();
            if (FeedManager.ShouldRefresh)
            {
                try
                {
                    SolveRefresh();
                }
                catch (WebException ex)
                {
                    Log.Warn("Unable to check for updates");
                    Log.Warn(ex);
                }
            }

            try
            {
                Fetch();
            }
            catch (WebException ex)
            {
                Log.Warn("Unable to download updates, try to find older version");
                Log.Warn(ex);
                SolveOffline();
                Fetch();
            }

            return Executor.Inject(_selections!)
                           .AddArguments(args)
                           .ToStartInfo();
        }

        /// <summary>
        /// Runs the Solver to select a version of Zero Install.
        /// </summary>
        private void Solve() => _selections = Solver.Solve(_requirements!);

        /// <summary>
        /// Runs the Solver in refresh mode (re-download all feeds).
        /// </summary>
        private void SolveRefresh()
        {
            FeedManager.Refresh = true;
            Solve();
        }

        /// <summary>
        /// Runs the Solver in offline mode.
        /// </summary>
        private void SolveOffline()
        {
            FeedManager.Refresh = false;
            Config.NetworkUse = NetworkLevel.Offline;
            Solve();
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
}
