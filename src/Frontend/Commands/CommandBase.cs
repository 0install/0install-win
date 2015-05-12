/*
 * Copyright 2010-2015 Bastian Eicher
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.Commands.CliCommands;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Services;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Common base class for frontend commands.
    /// </summary>
    public abstract class CommandBase : ServiceLocator
    {
        /// <summary>
        /// Creates a new command base.
        /// </summary>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about download and IO tasks.</param>
        protected CommandBase([NotNull] ITaskHandler handler) : base(handler)
        {}

        private AppList _appList;

        /// <summary>
        /// Lazy-loaded <see cref="AppList"/>. Not thread-safe!
        /// </summary>
        [NotNull]
        protected AppList AppList
        {
            get
            {
                if (_appList == null)
                {
                    try
                    {
                        _appList = XmlStorage.LoadXml<AppList>(AppList.GetDefaultPath());
                    }
                        #region Error handling
                    catch (FileNotFoundException)
                    {
                        _appList = new AppList();
                    }
                    catch (IOException ex)
                    {
                        Log.Warn(ex);
                        _appList = new AppList();
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Log.Warn(ex);
                        _appList = new AppList();
                    }
                    catch (InvalidDataException ex)
                    {
                        Log.Warn(ex);
                        _appList = new AppList();
                    }
                    #endregion
                }

                return _appList;
            }
        }

        /// <summary>
        /// Converts an interface or feed URI to its canonical representation.
        /// </summary>
        /// <exception cref="UriFormatException"><paramref name="uri"/> is an invalid interface URI.</exception>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Justification = "This method handles a number of non-standard URI types which cannot be represented by the regular Uri class.")]
        [NotNull]
        public FeedUri GetCanonicalUri(string uri)
        {
            if (string.IsNullOrEmpty(uri)) throw new UriFormatException();

            try
            {
                if (uri.StartsWith("alias:")) return ResolveAlias(uri.Substring("alias:".Length));
                else if (uri.StartsWith("file://")) return new FeedUri(uri);
                else if (uri.StartsWith("file:/")) throw new UriFormatException(Resources.FilePrefixAbsoluteUsage);
                else if (uri.StartsWith("file:")) return new FeedUri(Path.GetFullPath(uri.Substring("file:".Length)));
                else if (uri.StartsWith("http:") || uri.StartsWith("https:")) return new FeedUri(uri);

                string path = Path.GetFullPath(WindowsUtils.IsWindows ? Environment.ExpandEnvironmentVariables(uri) : uri);
                if (File.Exists(path)) return new FeedUri(path);

                var result = TryResolveCatalog(uri);
                if (result != null) return result;

                return new FeedUri(path);
            }
                #region Error handling
            catch (ArgumentException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new UriFormatException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new UriFormatException(ex.Message);
            }
            #endregion
        }

        [NotNull]
        private FeedUri ResolveAlias(string aliasName)
        {
            AppEntry appEntry;
            AddAlias.GetAppAlias(AppList, aliasName, out appEntry);
            if (appEntry == null) throw new UriFormatException(string.Format(Resources.AliasNotFound, aliasName));
            return appEntry.InterfaceUri;
        }

        [CanBeNull]
        private FeedUri TryResolveCatalog(string shortName)
        {
            var feed = FindByShortName(shortName);
            if (feed == null) return null;

            Log.Info(string.Format(Resources.ResolvedUsingCatalog, shortName, feed.Uri));
            return feed.Uri;
        }

        /// <summary>
        /// Uses <see cref="Catalog.FindByShortName"/> to find a <see cref="Feed"/> matching a specific short name.
        /// </summary>
        /// <param name="shortName">The short name to look for. Must match either <see cref="Feed.Name"/> or <see cref="EntryPoint.BinaryName"/> of <see cref="Command.NameRun"/>.</param>
        /// <returns>The first matching <see cref="Feed"/>; <see langword="null"/> if no match was found.</returns>
        [CanBeNull]
        protected Feed FindByShortName([NotNull] string shortName)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(shortName)) throw new ArgumentNullException("shortName");
            #endregion

            return
                CatalogManager.GetCachedSafe().FindByShortName(shortName) ??
                CatalogManager.GetOnlineSafe().FindByShortName(shortName);
        }

        /// <summary>
        /// Periodically checks Zero Install itself for updates in a background proccess.
        /// </summary>
        protected void SelfUpdateCheck()
        {
            if (!SelfUpdateUtils.NoAutoCheck && !SelfUpdateUtils.IsBlocked && Config.NetworkUse == NetworkLevel.Full && Handler.Verbosity != Verbosity.Batch)
            {
                Log.Debug("Determining whether self-update check is due");
                if (FeedManager.IsStale(Config.SelfUpdateUri))
                {
                    Log.Info("Starting periodic background self-update check");
                    RunCommandBackground(SelfUpdate.Name);
                }
            }
        }

        /// <summary>
        /// Executes a "0install" command in a new background process. Returns immediately.
        /// </summary>
        /// <param name="command">The <see cref="CliCommand.Name"/> of the command to execute.</param>
        /// <param name="args">Additional arguments to pass to the command.</param>
        protected static void RunCommandBackground([NotNull] string command, [NotNull] params string[] args)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(command)) throw new ArgumentNullException("command");
            #endregion

            if (ProgramUtils.GuiAssemblyName == null)
            {
                Log.Info("Skipping background command because there is no GUI subsystem available");
                return;
            }

            try
            {
                ProcessUtils.Assembly(ProgramUtils.GuiAssemblyName, args.Prepend("--background").Prepend(command)).Start();
            }
                #region Error handling
            catch (OperationCanceledException)
            {}
            catch (IOException ex)
            {
                Log.Warn(ex);
            }
            #endregion
        }
    }
}
