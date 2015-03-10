/*
 * Copyright 2010-2014 Bastian Eicher
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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NanoByte.Common.Streams;
using NanoByte.Common.Tasks;
using NanoByte.Common.Values;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Services;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Services.Injector;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Commands.FrontendCommands
{
    /// <summary>
    /// Represents a command issued via the command-line. Not to be confused with <see cref="Command"/>!
    /// </summary>
    /// <remarks>Specific sub-classes of this class are used to handle a commands like "0install COMMAND [OPTIONS]".</remarks>
    [CLSCompliant(false)]
    public abstract class FrontendCommand : ServiceLocator
    {
        #region Metadata
        /// <summary>
        /// The name of this command as used in command-line arguments in lower-case.
        /// </summary>
        public string Name
        {
            get
            {
                var field = GetType().GetField("Name", BindingFlags.Public | BindingFlags.Static);
                return (field == null) ? null : field.GetValue(null).ToString();
            }
        }

        /// <summary>
        /// A short description of what this command does.
        /// </summary>
        protected abstract string Description { get; }

        /// <summary>
        /// The additional arguments to be displayed after the command name in the help text.
        /// </summary>
        protected abstract string Usage { get; }

        /// <summary>
        /// The minimum number of <see cref="AdditionalArgs"/> allowed. Checked in <see cref="Parse"/>.
        /// </summary>
        protected virtual int AdditionalArgsMin { get { return 0; } }

        /// <summary>
        /// The maximum number of <see cref="AdditionalArgs"/> allowed. Checked in <see cref="Parse"/>.
        /// </summary>
        protected virtual int AdditionalArgsMax { get { return int.MaxValue; } }

        /// <summary>
        /// The help text describing the available command-line options and their effects.
        /// </summary>
        protected string HelpText
        {
            get
            {
                using (var buffer = new MemoryStream())
                {
                    var writer = new StreamWriter(buffer);
                    Options.WriteOptionDescriptions(writer);
                    writer.Flush();

                    // TODO: Add flow formatting for better readability on console
                    return Resources.Usage + " 0install " + Name + " " + Usage + Environment.NewLine + Environment.NewLine +
                           Description + Environment.NewLine + Environment.NewLine +
                           Resources.Options + Environment.NewLine + buffer.ReadToString();
                }
            }
        }

        /// <summary>The command-line argument parser used to evaluate user input.</summary>
        protected readonly OptionSet Options = new OptionSet();
        #endregion

        #region State
        /// <summary>
        /// A callback object used when the the user needs to be asked questions or informed about download and IO tasks.
        /// </summary>
        // Type covariance: ServiceLocator -> FrontendCommand, ITaskHandler -> ICommandHandler
        [NotNull]
        public new ICommandHandler Handler { get; private set; }

        /// <summary>Feeds to add, terms to search for, etc.</summary>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Using a List<T> for performance reasons")]
        [NotNull, ItemNotNull]
        protected readonly List<string> AdditionalArgs = new List<string>();

        private AppList _appList;

        /// <summary>
        /// Lazy-loaded <see cref="AppList"/>. Not thread-safe!
        /// </summary>
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
        /// Creates a new command.
        /// </summary>
        protected FrontendCommand([NotNull] ICommandHandler handler) : base(handler)
        {
            Handler = handler;

            Options.Add("?|h|help", () => Resources.OptionHelp, _ =>
            {
                Handler.Output(Resources.CommandLineArguments, HelpText);
                throw new OperationCanceledException(); // Don't handle any of the other arguments
            });
            Options.Add("background", () => Resources.OptionBackground, _ => Handler.Background = true);
            Options.Add("batch", () => Resources.OptionBatch, _ => Handler.Verbosity = Verbosity.Batch);
            Options.Add("v|verbose", () => Resources.OptionVerbose, _ => Handler.Verbosity++);
        }
        #endregion

        /// <summary>
        /// Parses command-line arguments and stores the result in the command.
        /// </summary>
        /// <param name="args">The command-line arguments to be parsed.</param>
        /// <exception cref="OperationCanceledException">The user asked to see help information, version information, etc..</exception>
        /// <exception cref="OptionException"><paramref name="args"/> contains unknown options.</exception>
        /// <exception cref="IOException">A problem occurred while creating a directory.</exception>
        /// <exception cref="UnauthorizedAccessException">more privileges are required.</exception>
        /// <exception cref="UriFormatException">The URI or local path specified is invalid.</exception>
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public virtual void Parse([NotNull, ItemNotNull] IEnumerable<string> args)
        {
            #region Sanity checks
            if (args == null) throw new ArgumentNullException("args");
            #endregion

            // Automatically show help for missing args
            if (AdditionalArgsMin > 0 && !args.Any()) args = new[] {"--help"};

            AdditionalArgs.AddRange(Options.Parse(args));

            if (AdditionalArgs.Count < AdditionalArgsMin) throw new OptionException(Resources.MissingArguments, "");
            if (AdditionalArgsMin == 1 && string.IsNullOrEmpty(AdditionalArgs[0])) throw new OptionException(Resources.MissingArguments, "");

            if (AdditionalArgs.Count > AdditionalArgsMax) throw new OptionException(Resources.TooManyArguments, "");
        }

        /// <summary>
        /// Executes the commands specified by the command-line arguments. Must call <see cref="Parse"/> first!
        /// </summary>
        /// <returns>The exit status code to end the process with. 0 means OK, 1 means generic error.</returns>
        /// <exception cref="OperationCanceledException">The user canceled the process.</exception>
        /// <exception cref="OptionException">The number of arguments passed in on the command-line is incorrect.</exception>
        /// <exception cref="WebException">A file could not be downloaded from the internet.</exception>
        /// <exception cref="NotSupportedException">A file format, protocal, etc. is unknown or not supported.</exception>
        /// <exception cref="IOException">A downloaded file could not be written to the disk or extracted or if an external application or file required by the solver could not be accessed.</exception>
        /// <exception cref="UnauthorizedAccessException">An operation failed due to insufficient rights.</exception>
        /// <exception cref="InvalidDataException">A problem occurred while deserializing an XML file.</exception>
        /// <exception cref="SignatureException">The signature data could not be handled for some reason.</exception>
        /// <exception cref="UriFormatException">The URI or local path specified is invalid.</exception>
        /// <exception cref="DigestMismatchException">An <see cref="Implementation"/>'s <see cref="Archive"/>s don't match the associated <see cref="ManifestDigest"/>.</exception>
        /// <exception cref="SolverException">The <see cref="ISolver"/> was unable to solve all dependencies.</exception>
        /// <exception cref="ImplementationNotFoundException">One of the <see cref="ImplementationBase"/>s is not cached yet.</exception>
        /// <exception cref="ExecutorException">The <see cref="IExecutor"/> was unable to process the <see cref="Selections"/>.</exception>
        /// <exception cref="Win32Exception">An executable could not be launched.</exception>
        /// <exception cref="BadImageFormatException">An executable could not be launched.</exception>
        /// <remarks>When inheriting this method is usually replaced.</remarks>
        public abstract int Execute();

        #region Helpers
        /// <summary>
        /// Converts an interface or feed URI to its canonical representation.
        /// </summary>
        /// <exception cref="UriFormatException"><paramref name="uri"/> is an invalid interface URI.</exception>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Justification = "This method handles a number of non-standard URI types which cannot be represented by the regular Uri class.")]
        public FeedUri GetCanonicalUri(string uri)
        {
            if (string.IsNullOrEmpty(uri)) throw new UriFormatException();

            try
            {
                if (uri.StartsWith("alias:")) return ResolveAlias(uri.Substring("alias:".Length));
                else if (uri.StartsWith("file://")) return new FeedUri(uri);
                else if (uri.StartsWith("file:/")) throw new ArgumentException(Resources.FilePrefixAbsoluteUsage);
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

        private FeedUri ResolveAlias(string aliasName)
        {
            AppEntry appEntry;
            AddAlias.GetAppAlias(AppList, aliasName, out appEntry);
            if (appEntry == null) throw new UriFormatException(string.Format(Resources.AliasNotFound, aliasName));
            return appEntry.InterfaceUri;
        }

        private FeedUri TryResolveCatalog(string shortName)
        {
            var feed = CatalogManager.GetCachedSafe().FindByShortName(shortName);
            if (feed == null)
            {
                feed = CatalogManager.GetOnlineSafe().FindByShortName(shortName);
                if (feed == null) return null;
            }

            Log.Info(string.Format(Resources.ResolvedUsingCatalog, shortName, feed.Uri));
            return feed.Uri;
        }

        /// <summary>
        /// Periodically checks Zero Install itself for updates in a background proccess.
        /// </summary>
        protected void SelfUpdateCheck()
        {
            if (!SelfUpdateUtils.NoAutoCheck && !SelfUpdateUtils.IsBlocked && FeedManager.IsStale(Config.SelfUpdateUri))
                RunCommandBackground(SelfUpdate.Name);
        }

        /// <summary>
        /// Executes a "0install" command in a new background process. Returns immediately.
        /// </summary>
        /// <param name="command">The <see cref="Name"/> of the command to execute.</param>
        /// <param name="args">Additional arguments to pass to the command.</param>
        protected static void RunCommandBackground([NotNull] string command, [NotNull] params string[] args)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(command)) throw new ArgumentNullException("command");
            #endregion

            if (WindowsUtils.IsWindows)
                ProcessUtils.LaunchAssembly("0install-win", command + " --background " + args.JoinEscapeArguments());
            //else if (UnixUtils.IsUnix)
            //    ProcessUtils.LaunchAssembly("0install-win", command + " --background " + args.JoinEscapeArguments());
        }

        /// <summary>
        /// Generates a localized instruction string describing multiple selectable values.
        /// </summary>
        protected static string SupportedValues<T>(IEnumerable<T> values)
        {
            return string.Format(Resources.SupportedValues, StringUtils.Join(", ", values.Select(AttributeUtils.ConvertToString)));
        }
        #endregion
    }
}
