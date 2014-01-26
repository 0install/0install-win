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
using Common;
using Common.Storage;
using Common.Utils;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Injector;
using ZeroInstall.Model;
using ZeroInstall.Solvers;
using ZeroInstall.Store;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Represents a command issued via the command-line. Not to be confused with <see cref="Model.Command"/>!
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
        /// A short title describing what this command does or <see langword="null"/>.
        /// </summary>
        public virtual string ActionTitle { get { return null; } }

        /// <summary>
        /// The number of milliseconds by which to delay the initial display of the <see cref="IHandler"/> GUI.
        /// </summary>
        public virtual int GuiDelay { get { return 0; } }

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

        /// <summary>
        /// Creates a new command.
        /// </summary>
        protected FrontendCommand(IBackendHandler handler) : base(handler)
        {
            Options.Add("?|h|help", () => Resources.OptionHelp, _ =>
            {
                Handler.Output(Resources.CommandLineArguments, HelpText);
                throw new OperationCanceledException(); // Don't handle any of the other arguments
            });
            Options.Add("v|verbose", () => Resources.OptionVerbose, _ => Handler.Verbosity++);
        }
        #endregion

        #region State
        /// <summary>Feeds to add, terms to search for, etc.</summary>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Using a List<T> for performance reasons")]
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
        #endregion

        /// <summary>
        /// Parses command-line arguments and stores the result in the command.
        /// </summary>
        /// <param name="args">The command-line arguments to be parsed.</param>
        /// <exception cref="OperationCanceledException">Thrown if the user asked to see help information, version information, etc..</exception>
        /// <exception cref="OptionException">Thrown if <paramref name="args"/> contains unknown options.</exception>
        /// <exception cref="IOException">Thrown if a problem occurred while creating a directory.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if more privileges are required.</exception>
        /// <exception cref="InvalidInterfaceIDException">Thrown when trying to set an invalid interface ID.</exception>
        public virtual void Parse(IEnumerable<string> args)
        {
            // ReSharper disable PossibleMultipleEnumeration
            // Automatically show help for missing args
            if (AdditionalArgsMin > 0 && !args.Any()) args = new[] {"--help"};

            AdditionalArgs.AddRange(Options.Parse(args));
            // ReSharper restore PossibleMultipleEnumeration

            if (AdditionalArgs.Count < AdditionalArgsMin) throw new OptionException(Resources.MissingArguments, "");
            if (AdditionalArgsMin == 1 && string.IsNullOrEmpty(AdditionalArgs[0])) throw new OptionException(Resources.MissingArguments, "");

            if (AdditionalArgs.Count > AdditionalArgsMax) throw new OptionException(Resources.TooManyArguments, "");
        }

        /// <summary>
        /// Executes the commands specified by the command-line arguments. Must call <see cref="Parse"/> first!
        /// </summary>
        /// <returns>The exit status code to end the process with. 0 means OK, 1 means generic error.</returns>
        /// <exception cref="OperationCanceledException">Thrown if the user canceled the process.</exception>
        /// <exception cref="OptionException">Thrown if the number of arguments passed in on the command-line is incorrect.</exception>
        /// <exception cref="WebException">Thrown if a file could not be downloaded from the internet.</exception>
        /// <exception cref="NotSupportedException">Thrown if a file format, protocal, etc. is unknown or not supported.</exception>
        /// <exception cref="IOException">Thrown if a downloaded file could not be written to the disk or extracted or if an external application or file required by the solver could not be accessed.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if an operation failed due to insufficient rights.</exception>
        /// <exception cref="InvalidDataException">Thrown if a problem occurred while deserializing an XML file.</exception>
        /// <exception cref="SignatureException">Thrown if the signature data could not be handled for some reason.</exception>
        /// <exception cref="InvalidInterfaceIDException">Thrown if no interface ID was specified while one was needed.</exception>
        /// <exception cref="DigestMismatchException">Thrown if an <see cref="Model.Implementation"/>'s <see cref="Archive"/>s don't match the associated <see cref="ManifestDigest"/>.</exception>
        /// <exception cref="SolverException">Thrown if the <see cref="ISolver"/> was unable to solve all dependencies.</exception>
        /// <exception cref="ImplementationNotFoundException">Thrown if one of the <see cref="ImplementationBase"/>s is not cached yet.</exception>
        /// <exception cref="CommandException">Thrown if there was a problem locating the implementation executable.</exception>
        /// <exception cref="Win32Exception">Thrown if an executable could not be launched.</exception>
        /// <exception cref="BadImageFormatException">Thrown if an executable could not be launched.</exception>
        /// <remarks>When inheriting this method is usually replaced.</remarks>
        public abstract int Execute();

        #region Helpers
        /// <summary>
        /// Converts an interface or feed ID to its canonical representation.
        /// </summary>
        /// <remarks>Aliases prefixed by "alias:" are resolved to the IDs they represent and relative local paths are converted to absolute paths. Everything else stays unchanged.</remarks>
        /// <exception cref="InvalidInterfaceIDException">Thrown if the <paramref name="id"/> is invalid.</exception>
        /// <exception cref="IOException">Thrown if there was a problem checking a local file path.</exception>
        public string GetCanonicalID(string id)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException("id");
            #endregion

            try
            {
                if (id.StartsWith("alias:")) return ResolveAliasId(id);
                else if (id.StartsWith("file:///")) return FileUtils.UnifySlashes(id.Substring(WindowsUtils.IsWindows ? 8 : 7));
                else if (id.StartsWith("file:/")) throw new ArgumentException(Resources.FilePrefixAbsoluteUsage);
                else if (id.StartsWith("file:")) return Path.GetFullPath(FileUtils.UnifySlashes(id.Substring(5)));
                else if (ModelUtils.IsValidUri(id)) return id;
                else
                { // Assume invalid URIs are...
                    if (!id.EndsWithIgnoreCase(".xml"))
                    {
                        // ... pet names or...
                        if (AppList.Contains(id)) return id;

                        // ... short names...
                        var feed = CatalogManager.GetCached().FindByShortName(id);
                        if (feed != null)
                        {
                            Log.Info(string.Format(Resources.ResolvedUsingCatalog, id, feed.Uri));
                            return feed.Uri.ToString();
                        }
                    }

                    // ... local paths
                    return Path.GetFullPath(id);
                }
            }
                #region Error handling
            catch (ArgumentException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new InvalidInterfaceIDException(ex.Message, ex);
            }
            catch (InvalidOperationException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new InvalidInterfaceIDException(ex.Message, ex);
            }
            #endregion
        }

        private string ResolveAliasId(string id)
        {
            string aliasName = id.Substring("alias:".Length);

            AppEntry appEntry;
            AddAlias.GetAppAlias(AppList, aliasName, out appEntry);

            if (appEntry == null) throw new InvalidInterfaceIDException(string.Format(Resources.AliasNotFound, aliasName));
            return appEntry.InterfaceID;
        }

        /// <summary>
        /// Generates a localized instruction string describing multiple selectable values.
        /// </summary>
        internal static string SupportedValues<T>(IEnumerable<T> values)
        {
            return string.Format(Resources.SupportedValues, StringUtils.Join(", ", values.Select(AttributeUtils.ConvertToString)));
        }
        #endregion
    }
}
