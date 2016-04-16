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
using System.Windows.Forms;
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

namespace ZeroInstall.Bootstrap
{
    /// <summary>
    /// Downloads and executes an instance of Zero Install.
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
        private string _importDir = Path.Combine(Locations.InstallBase, "bundled");

        /// <summary>Arguments passed through to the target process.</summary>
        [NotNull]
        private readonly List<string> _targetArgs = new List<string>();

        /// <summary>The file name of the currently running EXE.</summary>
        private static readonly string _exeName = Path.GetFileName(Application.ExecutablePath);

        /// <summary>
        /// <c>true</c> if <see cref="_exeName"/> file name indicates unattended installations should be per-user; <c>false</c> if they should be machine-wide.
        /// </summary>
        /// <remarks>Old installers were seperated into per-user and machine-wide. We now emulate this by looking at the EXE file name.</remarks>
        private static bool IsPerUser { get { return _exeName == "zero-install-per-user.exe"; } }

        private string HelpText
        {
            get
            {
                string help;
                using (var buffer = new MemoryStream())
                {
                    var writer = new StreamWriter(buffer);
                    writer.WriteLine("This bootstrapper downloads and runs Zero Install.");
                    writer.WriteLine("Usage: " + _exeName + " [OPTIONS] [-- 0INSTALL-ARGS]");
                    writer.WriteLine();
                    writer.WriteLine("Samples:");
                    writer.WriteLine("  " + _exeName + " -- central  Open main Zero Install GUI");
                    writer.WriteLine("  " + _exeName + " -- run vlc  Run VLC via Zero Install");
                    writer.WriteLine("  " + _exeName + " --silent    Deploy Zero Install for use without bootstrapper");
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
        /// <param name="gui"><c>true</c> if the application was launched in GUI mode; <c>false</c> if it was launched in command-line mode.</param>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about download and IO tasks.</param>
        public BootstrapProcess(bool gui, [NotNull] ITaskHandler handler) : base(handler)
        {
            _gui = gui;

            //// Only use per-user default cache location
            //Store = new DirectoryStore(
            //    Locations.GetCacheDirPath("0install.net", machineWide: false, resource: "implementations"),
            //    useWriteProtection: false);

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
                        if (Handler.Verbosity >= Verbosity.Verbose) throw new OptionException("Cannot combine --batch and verbose", "verbose");
                        Handler.Verbosity = Verbosity.Batch;
                    }
                },
                {
                    "v|verbose", () => "More verbose output. Use twice for even more verbose output.", _ =>
                    {
                        if (Handler.Verbosity == Verbosity.Batch) throw new OptionException("Cannot combine --batch and verbose", "batch");
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
                    "import=", () => "Specifies a {DIRECTORY} to search for feeds and archives to import. The default is a directory next to the bootstrapper called 'bundled'.", path =>
                    {
                        if (!Directory.Exists(path)) throw new DirectoryNotFoundException(string.Format("Directory '{0}' not found.", path));
                        _importDir = path;
                    }
                },
                {
                    "o|offline", () => "Run in off-line mode, not downloading anything.", _ => Config.NetworkUse = NetworkLevel.Offline
                },
                {
                    "silent", () => "Automatically deploy Zero Install in unattended mode.", _ =>
                    {
                        Handler.Verbosity = Verbosity.Batch;
                        _noExisting = true;

                        _targetArgs.Clear();
                        _targetArgs.AddRange(new[] {"maintenance", "deploy", "--batch"});
                        if (!IsPerUser) _targetArgs.Add("--machine");
                    }
                },
                {
                    "verysilent", () => "Automatically deploy Zero Install in unattended mode with no UI.", _ =>
                    {
                        Handler.Verbosity = Verbosity.Batch;
                        _noExisting = true;

                        _targetArgs.Clear();
                        _targetArgs.AddRange(new[] {"maintenance", "deploy", "--batch", "--background"});
                        if (!IsPerUser) _targetArgs.Add("--machine");
                    }
                },
                {"mergetasks", () => "Does nothing. For compatibility with old Inno Setup installer.", _ => { }},
                {"norestart", () => "Does nothing. For compatibility with old Inno Setup installer.", _ => { }},

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
        }
        #endregion

        /// <summary>
        /// Executes the bootstrap process controlled by command-line arguments.
        /// </summary>
        /// <param name="args">The command-line arguments to handle.</param>
        /// <returns>The exit status code to end the process with.</returns>
        public ExitCode Execute([NotNull, ItemNotNull] IEnumerable<string> args)
        {
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

            if (Directory.Exists(_importDir))
            {
                ImportFeeds();
                ImportArchives();
            }

            var startInfo = GetStartInfo();
            Handler.Dispose();
            return (ExitCode)startInfo.Run();
        }

        /// <summary>
        /// Imports bundled feeds and GnuPG keys.
        /// </summary>
        private void ImportFeeds()
        {
            if (!Directory.Exists(_importDir)) return;

            foreach (string path in Directory.GetFiles(_importDir, "*.xml"))
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
            foreach (string path in Directory.GetFiles(_importDir))
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
        private ProcessStartInfo GetStartInfo()
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
                string launchAssembly = _gui
                    ? "0install-win" //(WindowsUtils.IsWindows ? "0install-win" : "0install-gtk")
                    : "0install";

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

            return Executor.GetStartInfo(_selections, _targetArgs.ToArray());
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
