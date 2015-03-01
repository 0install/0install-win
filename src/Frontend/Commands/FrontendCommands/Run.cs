﻿/*
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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Native;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Services;
using ZeroInstall.Services.Injector;
using ZeroInstall.Store;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Commands.FrontendCommands
{
    /// <summary>
    /// This behaves similarly to <see cref="Download"/>, except that it also runs the program after ensuring it is in the cache.
    /// </summary>
    [CLSCompliant(false)]
    public class Run : Download
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "run";

        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionRun; } }

        /// <inheritdoc/>
        protected override string Usage { get { return base.Usage + " [ARGS]"; } }

        /// <inheritdoc/>
        protected override int AdditionalArgsMax { get { return int.MaxValue; } }
        #endregion

        #region State
        /// <summary>Immediately returns once the chosen program has been launched instead of waiting for it to finish executing.</summary>
        protected bool NoWait;

        /// <inheritdoc/>
        public Run([NotNull] ICommandHandler handler) : base(handler)
        {
            //Options.Remove("xml");
            //Options.Remove("show");

            Options.Add("m|main=", () => Resources.OptionMain, newMain => Executor.Main = newMain);
            Options.Add("w|wrapper=", () => Resources.OptionWrapper, newWrapper => Executor.Wrapper = newWrapper);
            Options.Add("no-wait", () => Resources.OptionNoWait, _ => NoWait = true);

            // Work-around to disable interspersed arguments (needed for passing arguments through to sub-processes)
            Options.Add("<>", value =>
            {
                AdditionalArgs.Add(value);

                // Stop using options parser, treat everything from here on as unknown
                Options.Clear();
            });
        }
        #endregion

        /// <inheritdoc/>
        public override int Execute()
        {
            Solve();
            if (UncachedImplementations.Count != 0) RefreshSolve();

            DownloadUncachedImplementations();

            Handler.CancellationToken.ThrowIfCancellationRequested();
            Handler.DisableUI();
            var process = LaunchImplementation();
            Handler.CloseUI();

            BackgroundUpdate();

            if (process == null) return 0;
            if (NoWait) return (WindowsUtils.IsWindows ? process.Id : 0);
            else
            {
                process.WaitForExit();
                return process.ExitCode;
            }
        }

        #region Helpers
        /// <inheritdoc/>
        protected override void Solve()
        {
            if (Config.NetworkUse == NetworkLevel.Full)
            {
                // Temporarily change configuration to prefer cached implementations (we download updates in the background later on)
                Config.NetworkUse = NetworkLevel.Minimal;

                try
                {
                    base.Solve();
                }
                finally
                {
                    // Restore original configuration
                    Config.NetworkUse = NetworkLevel.Full;
                }
            }
            else base.Solve();
        }

        /// <summary>
        /// Launches the selected implementation.
        /// </summary>
        /// <returns>The newly created <see cref="Process"/>; <see langword="null"/> if no external process was started.</returns>
        /// <exception cref="ImplementationNotFoundException">One of the <see cref="Implementation"/>s is not cached yet.</exception>
        /// <exception cref="ExecutorException">The <see cref="IExecutor"/> was unable to process the <see cref="Selections"/>.</exception>
        /// <exception cref="Win32Exception">An executable could not be launched.</exception>
        [SuppressMessage("Microsoft.Performance", "CA1820:TestForEmptyStringsUsingStringLength", Justification = "Explicit test for empty but non-null strings is intended")]
        [CanBeNull]
        protected Process LaunchImplementation()
        {
            if (Requirements.Command == "") throw new OptionException(Resources.NoRunWithEmptyCommand, "--command");

            using (CreateRunHook())
            {
                try
                {
                    return Executor.Start(Selections, AdditionalArgs.ToArray());
                }
                    #region Error handling
                catch (KeyNotFoundException ex)
                {
                    // Gracefully handle broken external selections
                    if (SelectionsDocument) throw new InvalidDataException(ex.Message, ex);
                    else throw;
                }
                #endregion
            }
        }

        private IDisposable CreateRunHook()
        {
            if (!NoWait && Config.AllowApiHooking && WindowsUtils.IsWindows)
            {
                try
                {
                    return new RunHook(Selections, Executor, FeedManager, Handler);
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

        /// <summary>
        /// If any of the feeds are getting old spawn a background update process.
        /// </summary>
        private void BackgroundUpdate()
        {
            if (FeedManager.Stale)
            {
                ProcessUtils.LaunchAssembly(
                    /*MonoUtils.IsUnix ? "0install-gtk" :*/ "0install-win",
                    "update --batch " + Requirements.ToCommandLine());
            }
        }
        #endregion
    }
}
