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
using System.Diagnostics;
using System.IO;
using Common.Storage;
using Common.Utils;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Injector;
using ZeroInstall.Model;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Common base class for <see cref="ZeroInstall.DesktopIntegration"/>-related commands.
    /// </summary>
    [CLSCompliant(false)]
    public abstract class AppCommand : CommandBase
    {
        #region Variables
        /// <summary>Indicate that <see cref="Cancel"/> has been called.</summary>
        protected volatile bool Canceled;

        /// <summary>Apply the operation system-wide instead of just for the current user.</summary>
        private bool _global;
        #endregion

        #region Properties
        /// <inheritdoc/>
        protected override string Usage { get { return "[OPTIONS] INTERFACE"; } }

        /// <inheritdoc/>
        public override string ActionTitle { get { return Resources.ActionAppCommand; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        protected AppCommand(Policy policy) : base(policy)
        {
            Options.Add("batch", Resources.OptionBatch, unused => Policy.Handler.Batch = true);
            Options.Add("g|global", Resources.OptionGlobal, unused => _global = true);
        }
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        public override int Execute()
        {
            #region Sanity checks
            if (!IsParsed) throw new InvalidOperationException(Resources.NotParsed);
            if (AdditionalArgs.Count == 0) throw new OptionException(Resources.MissingArguments, "");
            if (AdditionalArgs.Count > 1) throw new OptionException(Resources.TooManyArguments, "");
            #endregion

            string interfaceID = ModelUtils.CanonicalID(StringUtils.UnescapeWhitespace(AdditionalArgs[0]));

            if (_global && WindowsUtils.IsWindows && !WindowsUtils.IsAdministrator)
            { // Rerun the command with administrative rights
                var commandLine = new LinkedList<string>(Environment.GetCommandLineArgs());
                commandLine.RemoveFirst();
                var startInfo = new ProcessStartInfo(Path.Combine(Locations.InstallBase, "0install-win.exe"), StringUtils.ConcatenateEscape(commandLine)) {Verb = "runas"};
                var process = Process.Start(startInfo);
                process.WaitForExit();
                return process.ExitCode;
            }

            Policy.Handler.ShowProgressUI(Cancel);
            return ExecuteHelper(interfaceID, new CategoryIntegrationManager(_global));
        }

        /// <summary>
        /// Template method that performs the actual operation.
        /// </summary>
        /// <param name="interfaceID">The interface for the application to perform the operation on.</param>
        /// <param name="integrationManager">Manages desktop integration operations.</param>
        /// <returns>The exit status code to end the process with. 0 means OK, 1 means generic error.</returns>
        protected abstract int ExecuteHelper(string interfaceID, CategoryIntegrationManager integrationManager);
        #endregion

        #region Cancel
        /// <summary>
        /// Cancels the <see cref="Execute"/> session.
        /// </summary>
        public virtual void Cancel()
        {
            Canceled = true;
        }
        #endregion
    }
}