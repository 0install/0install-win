/*
 * Copyright 2010-2012 Bastian Eicher
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
using System.Net;
using System.Reflection;
using Common.Storage;
using Common.Streams;
using Common.Utils;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Injector;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Represents a command issued via the command-line. Not to be confused with <see cref="Model.Command"/>!
    /// </summary>
    /// <remarks>Specific sub-classes of this class are used to handle a commands like "0install COMMAND [OPTIONS]".</remarks>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 types only need to be disposed when using snapshots")]
    [CLSCompliant(false)]
    public abstract class FrontendCommand
    {
        #region Variables
        /// <summary>Indicates whether <see cref="Parse"/> has already been called.</summary>
        protected bool IsParsed;

        /// <summary>The command-line argument parser used to evaluate user input.</summary>
        protected readonly OptionSet Options = new OptionSet();

        /// <summary>Feeds to add, terms to search for, etc.</summary>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Using a List<T> for performance reasons")]
        protected readonly List<string> AdditionalArgs = new List<string>();
        #endregion

        #region Properties
        /// <summary>
        /// Combines UI access, preferences and resources used to solve dependencies and download implementations.
        /// </summary>
        public Policy Policy { get; private set; }

        /// <summary>
        /// A short description of what this command does.
        /// </summary>
        protected abstract string Description { get; }

        /// <summary>
        /// The additional arguments to be displayed after the command name in the help text.
        /// </summary>
        protected abstract string Usage { get; }

        /// <summary>
        /// A short title describing what this command does or <see langword="null"/>.
        /// </summary>
        public virtual string ActionTitle { get { return null; } }

        /// <summary>
        /// The help text describing the available command-line options and their effects.
        /// </summary>
        public string HelpText
        {
            get
            {
                using (var buffer = new MemoryStream())
                {
                    var writer = new StreamWriter(buffer);
                    Options.WriteOptionDescriptions(writer);
                    writer.Flush();

                    // ToDo: Add flow formatting for better readability on console
                    return Resources.Usage + " 0install " + GetName() + " " + Usage + Environment.NewLine + Environment.NewLine +
                        Description + Environment.NewLine + Environment.NewLine +
                            Resources.Options + Environment.NewLine + StreamUtils.ReadToString(buffer);
                }
            }
        }

        /// <summary>
        /// Uses reflection
        /// </summary>
        private string GetName()
        {
            var field = GetType().GetField("Name", BindingFlags.Public | BindingFlags.Static);
            // ReSharper disable AssignNullToNotNullAttribute
            return (field == null) ? null : field.GetValue(null).ToString();
            // ReSharper restore AssignNullToNotNullAttribute
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="policy">Provides additional class dependencies.</param>
        protected FrontendCommand(Policy policy)
        {
            Policy = policy;

            Options.Add("?|h|help", Resources.OptionHelp, unused =>
            {
                Policy.Handler.Output(Resources.CommandLineArguments, HelpText);
                throw new OperationCanceledException(); // Don't handle any of the other arguments
            });
            Options.Add("V|version", Resources.OptionVersion, unused =>
            {
                Policy.Handler.Output(Resources.VersionInformation, AppInfo.Name + " " + AppInfo.Version + (Locations.IsPortable ? " - " + Resources.PortableMode : "") + Environment.NewLine + AppInfo.Copyright + Environment.NewLine + Resources.LicenseInfo);
                throw new OperationCanceledException(); // Don't handle any of the other arguments
            });
            Options.Add("v|verbose", Resources.OptionVerbose, unused => Policy.Verbosity++);
        }
        #endregion

        //--------------------//

        #region Parse
        /// <summary>
        /// Parses command-line arguments and stores the result in the command.
        /// </summary>
        /// <param name="args">The command-line arguments to be parsed.</param>
        /// <exception cref="OperationCanceledException">Thrown if the user asked to see help information, version information, etc..</exception>
        /// <exception cref="OptionException">Thrown if <paramref name="args"/> contains unknown options.</exception>
        /// <exception cref="IOException">Thrown if a problem occurred while creating a directory.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a directory is not permitted.</exception>
        /// <exception cref="InvalidInterfaceIDException">Thrown when trying to set an invalid interface ID.</exception>
        public virtual void Parse(IEnumerable<string> args)
        {
            IsParsed = true;

            AdditionalArgs.AddRange(Options.Parse(args));
        }
        #endregion

        #region Execute
        /// <summary>
        /// Executes the commands specified by the command-line arguments.
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
        /// <exception cref="Win32Exception">Thrown if the main executable could not be launched.</exception>
        /// <exception cref="BadImageFormatException">Thrown if the main executable could not be launched.</exception>
        /// <exception cref="InvalidOperationException">Thrown if this method is called before calling <see cref="Parse"/>.</exception>
        /// <remarks>When inheriting this method is usually replaced.</remarks>
        public abstract int Execute();
        #endregion

        #region Helpers
        /// <summary>
        /// Converts an interface or feed ID to its canonical representation.
        /// </summary>
        /// <remarks>Aliases prefixed by "alias:" are resolved to the IDs they represent and relative local paths are converted to absolute paths. Everything else stays unchanged.</remarks>
        protected static string GetCanonicalID(string id)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException("id");
            #endregion

            if (id.StartsWith("alias:"))
            { // Look up AppEntry (and thus interface ID) belonging to an alias
                string aliasName = id.Substring("alias:".Length);
                AppEntry appEntry;
                AddAlias.GetAppAlias(AppList.Load(AppList.GetDefaultPath(false)), aliasName, out appEntry);
                return (appEntry == null ? id : appEntry.InterfaceID);
            }
            else if (ModelUtils.IsValidUri(id))
                return id;
            else
            { // Assume invalid URIs are local paths
                try
                {
                    return Path.GetFullPath(id);
                }
                    #region Error handling
                catch (ArgumentException)
                {
                    return id;
                }
                catch (NotSupportedException)
                {
                    return id;
                }
                catch (IOException)
                {
                    return id;
                }
                #endregion
            }
        }
        #endregion
    }
}
