/*
 * Copyright 2010-2013 Bastian Eicher
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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Common;
using Common.Utils;
using NDesk.Options;
using ZeroInstall.Backend;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Injector;
using ZeroInstall.Model;
using ZeroInstall.Store;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// This behaves similarly to <see cref="Download"/>, except that it also runs the program after ensuring it is in the cache.
    /// </summary>
    [CLSCompliant(false)]
    public class Run : Download
    {
        #region Constants
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "run";
        #endregion

        #region Variables
        /// <summary>An alternative executable to to run from the main <see cref="Model.Implementation"/> instead of <see cref="Element.Main"/>.</summary>
        private string _main;

        /// <summary>Instead of executing the selected program directly, pass it as an argument to this program. Useful for debuggers.</summary>
        private string _wrapper;

        /// <summary>Immediately returns once the chosen program has been launched instead of waiting for it to finish executing.</summary>
        protected bool NoWait;
        #endregion

        #region Properties
        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionRun; } }

        /// <inheritdoc/>
        protected override string Usage { get { return base.Usage + " [ARGS]"; } }

        /// <inheritdoc/>
        protected override int AdditionalArgsMax { get { return int.MaxValue; } }

        /// <inheritdoc/>
        public override string ActionTitle { get { return Resources.ActionRun; } }

        /// <inheritdoc/>
        public override int GuiDelay { get { return FeedManager.Refresh ? 0 : 1500; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public Run(IBackendHandler handler) : base(handler)
        {
            //Options.Remove("xml");
            //Options.Remove("show");

            Options.Add("m|main=", () => Resources.OptionMain, newMain => _main = newMain);
            Options.Add("w|wrapper=", () => Resources.OptionWrapper, newWrapper => _wrapper = newWrapper);
            Options.Add("no-wait", () => Resources.OptionNoWait, unused => NoWait = true);

            // Work-around to disable interspersed arguments (needed for passing arguments through to sub-processes)
            Options.Add("<>", value =>
            {
                AdditionalArgs.Add(value);

                // Stop using options parser, treat everything from here on as unknown
                Options.Clear();
            });
        }
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        public override int Execute()
        {
            Handler.ShowProgressUI();

            Solve();
            if (UncachedImplementations.Count != 0) RefreshSolve();
            ShowSelections();

            DownloadUncachedImplementations();
            BackgroundUpdate();

            Handler.CancellationToken.ThrowIfCancellationRequested();
            return LaunchImplementation();
        }
        #endregion

        #region Helpers
        /// <summary>
        /// If any of the feeds are getting old spawn a background update process.
        /// </summary>
        private void BackgroundUpdate()
        {
            if (StaleFeeds && Config.EffectiveNetworkUse == NetworkLevel.Full)
            {
                ProcessUtils.LaunchAssembly(
                    /*MonoUtils.IsUnix ? "0install-gtk" :*/ "0install-win",
                    "update --batch " + Requirements.ToCommandLineArgs());
            }
        }

        /// <summary>
        /// Launches the selected implementation.
        /// </summary>
        /// <returns>The exit code of the process or 0 if waiting is disabled.</returns>
        /// <exception cref="ImplementationNotFoundException">Thrown if one of the <see cref="Model.Implementation"/>s is not cached yet.</exception>
        /// <exception cref="CommandException">Thrown if there was a problem locating the implementation executable.</exception>
        /// <exception cref="Win32Exception">Thrown if an executable could not be launched.</exception>
        [SuppressMessage("Microsoft.Performance", "CA1820:TestForEmptyStringsUsingStringLength", Justification = "Explicit test for empty but non-null strings is intended")]
        protected int LaunchImplementation()
        {
            if (Requirements.Command == "") throw new OptionException(Resources.NoRunWithEmptyCommand, "--command");

            // Prevent the user from pressing any buttons once the child process is being launched
            Handler.DisableProgressUI();

            // Prepare new child process
            var executor = new Executor(Selections, Store) {Main = _main, Wrapper = _wrapper};

            using (var runHook = CreateRunHook(executor))
            {
                Process process;
                try
                {
                    // Launch new child process
                    process = executor.Start(AdditionalArgs.ToArray());

                    if (process == null) return 0;
                }
                    #region Error handling
                catch (KeyNotFoundException ex)
                {
                    // Gracefully handle broken external selections
                    if (SelectionsDocument) throw new InvalidDataException(ex.Message, ex);
                    else throw;
                }
                #endregion

                Handler.CloseProgressUI();

                try
                {
                    if (NoWait && runHook == null) return (WindowsUtils.IsWindows ? process.Id : 0);
                    process.WaitForExit();
                    return process.ExitCode;
                }
                finally
                {
                    process.Close();
                }
            }
        }

        private IDisposable CreateRunHook(Executor executor)
        {
            if (Config.AllowApiHooking && WindowsUtils.IsWindows)
            {
                try
                {
                    return new RunHook(executor, FeedManager, Handler);
                }
                    #region Error handling
                catch (ImplementationNotFoundException)
                {
                    Log.Warn(Resources.NoApiHookingNonCacheImpl);
                }
                #endregion
            }

            return null;
        }
        #endregion
    }
}
