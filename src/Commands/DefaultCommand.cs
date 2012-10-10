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
using System.Text;
using Common.Storage;
using Common.Utils;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Injector;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// The default command used when no command is explicitly specified.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class DefaultCommand : FrontendCommand
    {
        #region Properties
        /// <inheritdoc/>
        protected override string Description
        {
            get
            {
                var builder = new StringBuilder(Resources.TryHelpWith + Environment.NewLine);
                foreach (var possibleCommand in CommandFactory.ValidCommandNames)
                    builder.AppendLine("0install " + possibleCommand);
                return builder.ToString();
            }
        }

        /// <inheritdoc/>
        protected override string Usage { get { return "COMMAND"; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public DefaultCommand(Policy policy) : base(policy)
        {
            Options.Add("V|version", Resources.OptionVersion, unused =>
            {
                Policy.Handler.Output(Resources.VersionInformation, AppInfo.Name + " " + AppInfo.Version + (Locations.IsPortable ? " - " + Resources.PortableMode : "") + Environment.NewLine + AppInfo.Copyright + Environment.NewLine + Resources.LicenseInfo);
                throw new OperationCanceledException(); // Don't handle any of the other arguments
            });
        }
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        public override int Execute()
        {
            if (AdditionalArgs.Count != 0) throw new OptionException(Resources.TooManyArguments + "\n" + StringUtils.JoinEscapeArguments(AdditionalArgs), "");

            return 0;
        }
        #endregion
    }
}
