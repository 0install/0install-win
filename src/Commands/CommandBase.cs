/*
 * Copyright 2010-2011 Bastian Eicher
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
using Common;
using Common.Streams;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Fetchers;
using ZeroInstall.Injector;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Represents a command issued via the command-line.
    /// </summary>
    /// <remarks>Specific sub-classes of this class are used to handle a commands like "0install COMMAND [OPTIONS]".</remarks>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 types only need to be disposed when using snapshots")]
    [CLSCompliant(false)]
    public abstract class CommandBase
    {
        #region Variables
        /// <summary>Indicates whether <see cref="Parse"/> has already been called.</summary>
        private bool _parsed;

        /// <summary>The command-line argument parser used to evaluate user input.</summary>
        protected readonly OptionSet Options = new OptionSet();

        /// <summary>A callback object used when the the user needs to be asked questions or is to be informed about progress.</summary>
        protected readonly IHandler Handler;

        /// <summary>Combines configuration and resources used to solve dependencies and download implementations.</summary>
        protected readonly Policy Policy;

        /// <summary>The detail level of messages printed to the console. 0 = normal, 1 = verbose, 2 = very verbose</summary>
        protected int Verbosity;

        /// <summary>Feeds to add, terms to search for, etc.</summary>
        protected readonly C5.IList<string> AdditionalArgs = new C5.ArrayList<string>();
        #endregion

        #region Properties
        /// <summary>
        /// A short description of what this command does.
        /// </summary>
        protected abstract string Description { get; }

        /// <summary>
        /// The additional arguments to be displayed after the command name in the help text.
        /// </summary>
        protected abstract string Usage { get; }

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
                    return Resources.Usage + " 0install " + GetName() + " " + Usage + "\n\n" +
                           Description + "\n\n" +
                           Resources.Options + "\n" + StreamUtils.ReadToString(buffer);
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
        /// <param name="handler">A callback object used when the the user needs to be asked questions or is to be informed about progress.</param>
        /// <param name="policy">Combines configuration and resources used to solve dependencies and download implementations.</param>
        protected CommandBase(IHandler handler, Policy policy)
        {
            Handler = handler;
            Policy = policy;

            Options.Add("?|h|help", Resources.OptionHelp, unused =>
            {
                handler.Output(Resources.CommandLineArguments, HelpText);
                throw new UserCancelException(); // Don't handle any of the other arguments
            });
            Options.Add("V|version", Resources.OptionVersion, unused =>
            {
                handler.Output(Resources.VersionInformation, ApplicationInfo.Name + " " + ApplicationInfo.Version + "\n" + ApplicationInfo.Copyright + "\n" + Resources.LicenseInfo);
                throw new UserCancelException(); // Don't handle any of the other arguments
            });

            Options.Add("with-store=", Resources.OptionWithStore, path => Policy.AdditionalStore = new DirectoryStore(path));
            Options.Add("o|offline", Resources.OptionOffline, unused => Policy.Preferences.NetworkLevel = NetworkLevel.Offline);
            Options.Add("v|verbose", Resources.OptionVerbose, unused => Verbosity++);
        }
        #endregion

        //--------------------//

        #region Parse
        /// <summary>
        /// Parses command-line arguments.
        /// </summary>
        /// <param name="args">The command-line arguments to be parsed.</param>
        /// <exception cref="OptionException">Thrown if <paramref name="args"/> contains unknown options.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the underlying filesystem of the user profile can not store file-changed times accurate to the second.</exception>
        /// <exception cref="IOException">Thrown if a problem occurred while creating a directory.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a directory is not permitted.</exception>
        /// <exception cref="InvalidInterfaceIDException">Thrown when trying to set an invalid interface ID.</exception>
        public virtual void Parse(IEnumerable<string> args)
        {
            _parsed = true;

            AdditionalArgs.AddAll(Options.Parse(args));
        }
        #endregion

        #region Execute
        /// <summary>
        /// Performs the actual steps to be executed without creating the user output.
        /// </summary>
        /// <exception cref="UserCancelException">Thrown if a download or IO task was canceled.</exception>
        /// <exception cref="OptionException">Thrown if the number of arguments passed in on the command-line is incorrect.</exception>
        /// <exception cref="WebException">Thrown if a file could not be downloaded from the internet.</exception>
        /// <exception cref="NotSupportedException">Thrown if an archive type is unknown or not supported.</exception>
        /// <exception cref="IOException">Thrown if a downloaded file could not be written to the disk or extracted or if an external application or file required by the solver could not be accessed.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to an <see cref="IStore"/> is not permitted.</exception>
        /// <exception cref="InvalidInterfaceIDException">Thrown if no interface ID was specified while one was needed.</exception>
        /// <exception cref="SolverException">Thrown if the <see cref="ISolver"/> was unable to solve all dependencies.</exception>
        /// <exception cref="FetcherException">Thrown if an <see cref="Model.Implementation"/> could not be downloaded.</exception>
        /// <exception cref="DigestMismatchException">Thrown if an <see cref="Model.Implementation"/>'s <see cref="Archive"/>s don't match the associated <see cref="ManifestDigest"/>.</exception>
        /// <exception cref="ImplementationNotFoundException">Thrown if one of the <see cref="ImplementationBase"/>s is not cached yet.</exception>
        /// <exception cref="CommandException">Thrown if there was a problem locating the implementation executable.</exception>
        /// <exception cref="Win32Exception">Thrown if the main executable could not be launched.</exception>
        /// <exception cref="BadImageFormatException">Thrown if the main executable could not be launched.</exception>
        /// <exception cref="InvalidOperationException">Thron if this method is called before calling <see cref="Parse"/>.</exception>
        /// <remarks>When inheriting this method is usually extended.</remarks>
        protected virtual void ExecuteHelper()
        {
            if (!_parsed) throw new InvalidOperationException(Resources.NotParsed);
        }

        /// <summary>
        /// Executes the commands specified by the command-line arguments.
        /// </summary>
        /// <returns>The exit status code to end the process with. 0 means OK, 1 means generic error.</returns>
        /// <exception cref="UserCancelException">Thrown if a download or IO task was canceled.</exception>
        /// <exception cref="OptionException">Thrown if the number of arguments passed in on the command-line is incorrect.</exception>
        /// <exception cref="WebException">Thrown if a file could not be downloaded from the internet.</exception>
        /// <exception cref="NotSupportedException">Thrown if an archive type is unknown or not supported.</exception>
        /// <exception cref="IOException">Thrown if a downloaded file could not be written to the disk or extracted or if an external application or file required by the solver could not be accessed.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to an <see cref="IStore"/> is not permitted.</exception>
        /// <exception cref="InvalidInterfaceIDException">Thrown if no interface ID was specified while one was needed.</exception>
        /// <exception cref="SolverException">Thrown if the <see cref="ISolver"/> was unable to solve all dependencies.</exception>
        /// <exception cref="FetcherException">Thrown if an <see cref="Model.Implementation"/> could not be downloaded.</exception>
        /// <exception cref="DigestMismatchException">Thrown if an <see cref="Model.Implementation"/>'s <see cref="Archive"/>s don't match the associated <see cref="ManifestDigest"/>.</exception>
        /// <exception cref="ImplementationNotFoundException">Thrown if one of the <see cref="ImplementationBase"/>s is not cached yet.</exception>
        /// <exception cref="CommandException">Thrown if there was a problem locating the implementation executable.</exception>
        /// <exception cref="Win32Exception">Thrown if the main executable could not be launched.</exception>
        /// <exception cref="BadImageFormatException">Thrown if the main executable could not be launched.</exception>
        /// <exception cref="InvalidOperationException">Thron if this method is called before calling <see cref="Parse"/>.</exception>
        /// <remarks>When inheriting this method is usually replaced.</remarks>
        public abstract int Execute();
        #endregion
    }
}
