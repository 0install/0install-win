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
using Common.Utils;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Injector;
using ZeroInstall.Injector.Solver;
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
        /// <summary>Apply the configuration system-wide instead of just for the current user.</summary>
        protected bool Global;
        #endregion

        #region Properties
        /// <inheritdoc/>
        protected override string Usage { get { return "[OPTIONS] INTERFACE"; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        protected AppCommand(Policy policy) : base(policy)
        {
            Options.Add("g|global", Resources.OptionGlobal, unused => Global = true);
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

            // Run Solver to ensure feed is cached
            Policy.FeedManager.Refresh = true;
            bool stale;
            Policy.Solver.Solve(new Requirements {InterfaceID = interfaceID}, Policy, out stale);

            // ToDo: Get data from additional feeds
            var feed = Policy.FeedManager.GetFeed(interfaceID, Policy, out stale);

            return ExecuteHelper(interfaceID, feed);
        }

        /// <summary>
        /// Template method that performs the actual operation.
        /// </summary>
        /// <param name="interfaceID">The interface for the application to perform the operation on.</param>
        /// <param name="feed">The feed for the application to perform the operation on.</param>
        /// <returns>The exit status code to end the process with. 0 means OK, 1 means generic error.</returns>
        protected abstract int ExecuteHelper(string interfaceID, Feed feed);
        #endregion
    }
}