/*
 * Copyright 2010-2016 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 *
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NanoByte.Common.Streams;
using NanoByte.Common.Tasks;
using NDesk.Options;
using ZeroInstall.Services;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Store;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Implementations.Archives;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall
{
    /// <summary>
    /// Represents the process of downloading and running an instance of Zero Install. Handles command-line arguments.
    /// </summary>
    public class BootstrapProcess : ServiceLocator
    {
        #region Command-line options
        private readonly bool _gui;

        /// <summary>The command-line argument parser used to evaluate user input.</summary>
        [NotNull]
        private readonly OptionSet _options;

        private bool _noExisting;

        /// <summary>A specific version of Zero Install to download.</summary>
        [CanBeNull]
        private VersionRange _version;

        /// <summary>A directory to search for feeds and archives to import.</summary>
        private string _contentDir = Path.Combine(Locations.InstallBase, "content");

        /// <summary>Arguments passed through to the target process.</summary>
        [NotNull]
        private readonly List<string> _targetArgs = new List<string>();

        /// <summary>
        /// <c>true</c> if <see cref="Program.ExeName"/> file name indicates unattended installations should be per-user; <c>false</c> if they should be machine-wide.
        /// </summary>
        /// <remarks>Old installers were seperated into per-user and machine-wide. We now emulate this by looking at the EXE file name.</remarks>
        private static bool IsPerUser => StringUtils.EqualsIgnoreCase(Program.ExeName, "zero-install-per-user");

        private string HelpText
        {
            get
            {
                string exeName = Program.ExeName.EscapeArgument();
                string help;
                using (var buffer = new MemoryStream())
                {
                    var writer = new StreamWriter(buffer);
                    switch (EmbeddedConfig.Instance.AppMode)
                    {
                        case BootstrapMode.None:
                            writer.WriteLine("This bootstrapper downloads and runs Zero Install.");
                            writer.WriteLine("Usage: {0} [OPTIONS] [[--] 0INSTALL-ARGS]", exeName);
                            writer.WriteLine();
                            writer.WriteLine("Samples:");
                            writer.WriteLine("  {0} central             Open main Zero Install GUI.", exeName);
                            writer.WriteLine("  {0} maintenance deploy  Deploy Zero Install to this computer.", exeName);
                            writer.WriteLine("  {0} run vlc             Run VLC via Zero Install.", exeName);
                            writer.WriteLine("  {0} -- --help           Show help for Zero Install instead of Bootstrapper.", exeName);
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
                    help = buffer.ReadToString();
                }
                return help;
            }
        }

        /// <summary>
        /// Creates a new bootstrap process.
        /// </summary>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about download and IO tasks.</param>
        /// <param name="gui"><c>true</c> if the application was launched in GUI mode; <c>false</c> if it was launched in command-line mode.</param>
        public BootstrapProcess([NotNull] ITaskHandler handler, bool gui) : base(handler)
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
                        Config.SelfUpdateUri = new FeedUri(feed);
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
                        _options.Clear();
                    }
                }
            };

            if (EmbeddedConfig.Instance.AppMode == BootstrapMode.None)
            {
                _options.Add("silent", () => "Deploy Zero Install in unattended mode. Equivalent to \"maintenance deploy --batch\".", _ =>
                {
                    _targetArgs.Clear();
                    _targetArgs.AddRange(new[] {"maintenance", "deploy", "--batch"});
                    if (!IsPerUser) _targetArgs.Add("--machine");
                });
                _options.Add("verysilent", () => "Deploy Zero Install in unattended mode with no UI. Equivalent to \"maintenance deploy --batch --background\".", _ =>
                {
                    _targetArgs.Clear();
                    _targetArgs.AddRange(new[] {"maintenance", "deploy", "--batch", "--background"});
                    if (!IsPerUser) _targetArgs.Add("--machine");
                });
            }
        }
        #endregion

        /// <summary>
        /// Executes the bootstrap process controlled by command-line arguments.
        /// </summary>
        /// <param name="args">The command-line arguments to handle.</param>
        /// <returns>The exit status code to end the process with.</returns>
        public ExitCode Execute([NotNull, ItemNotNull] IEnumerable<string> args)
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
        {
            switch (EmbeddedConfig.Instance.AppMode)
            {
                case BootstrapMode.Run:
                    return new[] {"run", EmbeddedConfig.Instance.AppUri.ToStringRfc()};
                case BootstrapMode.Integrate:
                    return new[] {"integrate", EmbeddedConfig.Instance.AppUri.ToStringRfc()};
                default:
                    return new string[0];
            }
        }

        /// <summary>
        /// Handles arguments passed to the target that are also applicable to the bootstrapper.
        /// </summary>
        private void HandleSharedOptions()
        {
            foreach (string arg in _targetArgs)
            {
                switch (arg)
                {
                    case "batch":
                        Handler.Verbosity = Verbosity.Batch;
                        break;
                    case "o":
                    case "offline":
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
        /// Imports implementation archives into the <see cref="IStore"/>.
        /// </summary>
        private void ImportArchives()
        {
            foreach (string path in Directory.GetFiles(_contentDir))
            {
                Debug.Assert(path != null);
                var digest = new ManifestDigest();
                digest.ParseID(Path.GetFileNameWithoutExtension(path));
                if (digest.Best != null && !Store.Contains(digest))
                {
                    try
                    {
                        Store.AddArchives(new[]
                        {
                            new ArchiveFileInfo
                            {
                                Path = path,
                                MimeType = Archive.GuessMimeType(path)
                            }
                        }, digest, Handler);
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
        [NotNull]
        public ProcessStartInfo GetStartInfo()
        {
            return _noExisting
                ? GetCached()
                : (GetExistingInstance() ?? GetCached());
        }

        /// <summary>
        /// Returns process start information for an existing (local) instance of Zero Install.
        /// </summary>
        [CanBeNull]
        private ProcessStartInfo GetExistingInstance()
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

        private Requirements _requirements;
        private Selections _selections;

        /// <summary>
        /// Returns process start information for a cached (downloaded) instance of Zero Install.
        /// </summary>
        [NotNull]
        private ProcessStartInfo GetCached()
        {
            _requirements = new Requirements(Config.SelfUpdateUri, _gui ? Command.NameRunGui : Command.NameRun);
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

            return Executor
                .Inject(_selections)
                .AddArguments(_targetArgs.ToArray())
                .ToStartInfo();
        }

        /// <summary>
        /// Runs the Solver to select a version of Zero Install.
        /// </summary>
        private void Solve()
        {
            _selections = Solver.Solve(_requirements);
        }

        /// <summary>
        /// Runs the Solver in refresh mode (re-download all feeds).
        /// </summary>
        private void SolveRefresh()
        {
            FeedManager.Refresh = true;
            Solve();
        }

        /// <summary>
        /// Runs the Solver in offlie mode.
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
            var uncached = SelectionsManager.GetUncachedImplementations(_selections);
            Fetcher.Fetch(uncached);
        }
    }
}
