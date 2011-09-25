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
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using Common;
using Common.Collections;
using Common.Utils;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Injector;
using ZeroInstall.Model;
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
        public override string ActionTitle { get { return Resources.ActionRun; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public Run(Policy policy) : base(policy)
        {
            //Options.Remove("xml");
            //Options.Remove("show");

            Options.Add("m|main=", Resources.OptionMain, newMain => _main = newMain);
            Options.Add("w|wrapper=", Resources.OptionWrapper, newWrapper => _wrapper = newWrapper);
            Options.Add("no-wait", Resources.OptionNoWait, unused => NoWait = true);

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
            #region Sanity checks
            if (!IsParsed) throw new InvalidOperationException(Resources.NotParsed);
            #endregion

            Policy.Handler.ShowProgressUI(Cancel);

            Solve();

            // If any implementations need to be downloaded rerun solver in refresh mode (unless it was already in that mode to begin with)
            if (!EnumerableUtils.IsEmpty(UncachedImplementations) && !Policy.FeedManager.Refresh && Policy.Config.NetworkUse != NetworkLevel.Offline)
            {
                Policy.FeedManager.Refresh = true;
                Solve();
            }
            SelectionsUI();

            DownloadUncachedImplementations();

            // If any of the feeds are getting old spawn background update process
            if (StaleFeeds && Policy.Config.NetworkUse != NetworkLevel.Offline)
            {
                // ToDo: Automatically switch to GTK# on Linux
                ProcessUtils.LaunchHelperAssembly("0install-win", "update --batch " + Requirements.ToCommandLineArgs());
            }

            if (Canceled) throw new UserCancelException();
            return LaunchImplementation();
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Launches the selected implementation.
        /// </summary>
        /// <returns>The exit code of the process or 0 if waiting is disabled.</returns>
        /// <exception cref="ImplementationNotFoundException">Thrown if one of the <see cref="Model.Implementation"/>s is not cached yet.</exception>
        /// <exception cref="CommandException">Thrown if there was a problem locating the implementation executable.</exception>
        /// <exception cref="Win32Exception">Thrown if the main executable could not be launched.</exception>
        protected int LaunchImplementation()
        {
            // Prevent the user from pressing any buttons once the child process is being launched
            Policy.Handler.DisableProgressUI();

            // Prepare new child process
            var executor = new Executor(Selections, Policy.Fetcher.Store) {Main = _main, Wrapper = _wrapper};

            // Hook into process launching if API hooking is applicable
            RunHook runHook = null;
            if (Policy.Config.AllowApiHooking && WindowsUtils.IsWindows)
            {
                try
                {
                    runHook = new RunHook(Policy, executor);
                }
                    #region Error handling
                catch (ImplementationNotFoundException)
                {
                    Log.Warn("API hooking not possible due to uncached implementation");
                }
                #endregion
            }

            Process process;
            try
            {
                // Launch new child process
                process = executor.Start(AdditionalArgs.ToArray());
            }
            finally
            {
                // Hook out of process launching when done
                if (runHook != null) runHook.Dispose();
            }

            // Wait for a moment before closing the GUI so that focus is retained until it can be passed on to the child process
            Thread.Sleep(1000);
            Policy.Handler.CloseProgressUI();

            if (NoWait || process == null) return 0;
            process.WaitForExit();
            return process.ExitCode;
        }
        #endregion
    }
}
