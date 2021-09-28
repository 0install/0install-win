// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using NanoByte.Common;
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

namespace ZeroInstall
{
    /// <summary>
    /// Represents the process of downloading and running an instance of Zero Install. Handles command-line arguments.
    /// </summary>
    public class BootstrapProcess : ServiceProvider
    {
        #region Command-line options
        private readonly bool _gui;

        /// <summary>The command-line argument parser used to evaluate user input.</summary>
        private readonly OptionSet _options;

        private bool _noExisting;

        /// <summary>A specific version of Zero Install to download.</summary>
        private VersionRange? _version;

        /// <summary>A directory to search for feeds and archives to import.</summary>
        private string _contentDir = Path.Combine(Locations.InstallBase, "content");

        /// <summary>Arguments passed through to the target process.</summary>
        private readonly List<string> _targetArgs = new();

        private string HelpText
        {
            get
            {
                string exeName = Path.GetFileNameWithoutExtension(new Uri(Assembly.GetEntryAssembly()!.CodeBase).LocalPath);

                using var buffer = new MemoryStream();
                var writer = new StreamWriter(buffer);
                switch (EmbeddedConfig.Instance.AppMode)
                {
                    case BootstrapMode.None:
                        writer.WriteLine("This bootstrapper downloads and runs Zero Install.");
                        writer.WriteLine("Usage: {0} [OPTIONS] [[--] 0INSTALL-ARGS]", exeName);
                        writer.WriteLine();
                        writer.WriteLine("Samples:");
                        writer.WriteLine("  {0} self deploy  Deploy Zero Install to this computer.", exeName);
                        writer.WriteLine("  {0} central      Open main Zero Install GUI.", exeName);
                        writer.WriteLine("  {0} run vlc      Run VLC via Zero Install.", exeName);
                        writer.WriteLine("  {0} -- --help    Show help for Zero Install instead of Bootstrapper.", exeName);
                        break;
                    case BootstrapMode.Run:
                        writer.WriteLine("This bootstrapper downloads and runs {0} using Zero Install.", EmbeddedConfig.Instance.AppName);
                        writer.WriteLine("Usage: {0} [OPTIONS] [[--] APP-ARGS]", exeName);
                        writer.WriteLine();
                        writer.WriteLine("Samples:");
                        writer.WriteLine("  {0}               Run {1}.", exeName, EmbeddedConfig.Instance.AppName);
                        writer.WriteLine("  {0} --offline     Run {1} without downloading anything.", exeName, EmbeddedConfig.Instance.AppName);
                        writer.WriteLine("  {0} -x            Run {1} with argument '-x'.", exeName, EmbeddedConfig.Instance.AppName);
                        writer.WriteLine("  {0} -- --offline  Run {1} with argument '--offline'.", exeName, EmbeddedConfig.Instance.AppName);
                        break;
                    case BootstrapMode.Integrate:
                        writer.WriteLine("This bootstrapper downloads and integrates {0} using Zero Install.", EmbeddedConfig.Instance.AppName);
                        writer.WriteLine("Usage: {0} [OPTIONS] [[--] INTEGRATE-ARGS]", exeName);
                        writer.WriteLine();
                        writer.WriteLine("Samples:");
                        writer.WriteLine("  {0}             Show GUI for integrating {1}.", exeName, EmbeddedConfig.Instance.AppName);
                        writer.WriteLine("  {0} --add=menu  Add {1} to start menu.", exeName, EmbeddedConfig.Instance.AppName);
                        writer.WriteLine("  {0} -- --help   Show help for {1} integration instead of Bootstrapper.", exeName, EmbeddedConfig.Instance.AppName);
                        break;
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
                    "no-existing", () => "Do not detect and use existing Zero Install instances. Always use downloaded and cached instance.", _ => _noExisting = true
                },
                {
                    "version=", () => "Select a specific {VERSION} of Zero Install. Implies --no-existing.", (VersionRange range) =>
                    {
                        _version = range;
                        _noExisting = true;
                    }
                },
                {
                    "feed=", () => "Specify an alternative {FEED} for Zero Install. Must be an absolute URI. Implies --no-existing.", feed =>
                    {
                        Config.SelfUpdateUri = new(feed);
                        _noExisting = true;
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
                },

                // Disable interspersed arguments (needed for passing arguments through to target)
                {
                    "<>", value =>
                    {
                        _targetArgs.Add(value);

                        // Stop using options parser, treat everything from here on as unknown
                        _options!.Clear();
                    }
                }
            };
        }
        #endregion

        /// <summary>
        /// Executes the bootstrap process controlled by command-line arguments.
        /// </summary>
        /// <param name="args">The command-line arguments to handle.</param>
        /// <returns>The exit status code to end the process with.</returns>
        public ExitCode Execute(IEnumerable<string> args)
        {
            // Write any customized configuration to the user profile
            // NOTE: This must be done before parsing command-line options, since they may manipulate Config
            Config.Save();

            _targetArgs.AddRange(GetEmbeddedArgs());
            _targetArgs.AddRange(_options.Parse(args));

            if (_targetArgs.Count == 0)
            {
                if (_gui) _targetArgs.Add("central");
                else
                {
                    Handler.Output("Help", HelpText);
                    return ExitCode.UserCanceled;
                }
            }
            else HandleSharedOptions();

            if (Directory.Exists(_contentDir))
            {
                ImportFeeds();
                ImportArchives();
            }

            var startInfo = GetStartInfo();
            Handler.Dispose();
            return (ExitCode)startInfo.Run();
        }

        /// <summary>
        /// Gets implicit command-line arguments based on the <see cref="EmbeddedConfig"/>, if any.
        /// </summary>
        private static IEnumerable<string> GetEmbeddedArgs()
            => EmbeddedConfig.Instance.AppMode switch
            {
                BootstrapMode.Run => new[] {"run", EmbeddedConfig.Instance.AppUri.ToStringRfc()},
                BootstrapMode.Integrate => new[] {"integrate", EmbeddedConfig.Instance.AppUri.ToStringRfc()},
                _ => new string[0]
            };

        /// <summary>
        /// Handles arguments passed to the target that are also applicable to the bootstrapper.
        /// </summary>
        private void HandleSharedOptions()
        {
            foreach (string arg in _targetArgs)
            {
                switch (arg)
                {
                    case "--batch":
                        Handler.Verbosity = Verbosity.Batch;
                        break;
                    case "-o":
                    case "--offline":
                        Config.NetworkUse = NetworkLevel.Offline;
                        break;
                }
            }
        }

        /// <summary>
        /// Imports bundled feeds and GnuPG keys.
        /// </summary>
        private void ImportFeeds()
        {
            if (!Directory.Exists(_contentDir)) return;

            foreach (string path in Directory.GetFiles(_contentDir, "*.xml"))
            {
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
        }

        /// <summary>
        /// Imports implementation archives into the <see cref="IImplementationStore"/>.
        /// </summary>
        private void ImportArchives()
        {
            foreach (string path in Directory.GetFiles(_contentDir))
            {
                Debug.Assert(path != null);
                var digest = new ManifestDigest();
                digest.ParseID(Path.GetFileNameWithoutExtension(path));
                if (digest.Best != null && !ImplementationStore.Contains(digest))
                {
                    try
                    {
                        ImplementationStore.Add(digest, builder =>
                        {
                            var extractor = ArchiveExtractor.For(Archive.GuessMimeType(path), Handler);
                            extractor.Tag = digest.Best;
                            using var stream = File.OpenRead(path);
                            extractor.Extract(builder, stream);
                        });
                    }
                    #region Error handling
                    catch (ImplementationAlreadyInStoreException)
                    {}
                    #endregion
                }
            }
        }

        /// <summary>
        /// Returns process start information for an instance of Zero Install.
        /// </summary>
        public ProcessStartInfo GetStartInfo()
            => _noExisting ? GetCached() : (GetExistingInstance() ?? GetCached());

        /// <summary>
        /// Returns process start information for an existing (local) instance of Zero Install.
        /// </summary>
        private ProcessStartInfo? GetExistingInstance()
        {
            if (!WindowsUtils.IsWindows) return null;

            string existingInstall = RegistryUtils.GetSoftwareString("Zero Install", "InstallLocation");
            if (!string.IsNullOrEmpty(existingInstall))
            {
                string launchAssembly = _gui ? "0install-win" : "0install";

                if (File.Exists(Path.Combine(existingInstall, launchAssembly + ".exe")))
                    return ProcessUtils.Assembly(Path.Combine(existingInstall, launchAssembly), _targetArgs.ToArray());
            }
            return null;
        }

        private Requirements? _requirements;
        private Selections? _selections;

        /// <summary>
        /// Returns process start information for a cached (downloaded) instance of Zero Install.
        /// </summary>
        private ProcessStartInfo GetCached()
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
                           .AddArguments(_targetArgs.ToArray())
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
