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
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Services;
using ZeroInstall.Store;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// View or change <see cref="Config"/>.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class Configure : FrontendCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "config";

        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionConfig; } }

        /// <inheritdoc/>
        protected override string Usage { get { return "[NAME [VALUE]]"; } }

        /// <inheritdoc/>
        public Configure(IBackendHandler handler) : base(handler)
        {}
        #endregion

        /// <inheritdoc/>
        public override int Execute()
        {
            switch (AdditionalArgs.Count)
            {
                case 0:
                    // Only save if the user confirmed the changes
                    if (Handler.ShowConfig(Config)) Config.Save();
                    return 0;

                case 1:
                    return PrintOption(AdditionalArgs[0]);

                case 2:
                    return SetOption(AdditionalArgs[0], AdditionalArgs[1]);

                default:
                    throw new OptionException(Resources.TooManyArguments, "");
            }
        }

        #region Helpers
        /// <summary>
        /// Prints the value of an option or an error message.
        /// </summary>
        /// <returns>The exit status code to end the process with. 0 means OK, 1 means generic error.</returns>
        private int PrintOption(string key)
        {
            string value;
            try
            {
                value = Config.GetOption(key);
            }
                #region Error handling
            catch (KeyNotFoundException)
            {
                Handler.Output("Configuration error", string.Format("Unknown option '{0}'", key));
                return 1;
            }
            #endregion

            Handler.Output(key, value);
            return 0;
        }

        /// <summary>
        /// Sets the value of an option or prints an error message.
        /// </summary>
        /// <returns>The exit status code to end the process with. 0 means OK, 1 means generic error.</returns>
        private int SetOption(string key, string value)
        {
            try
            {
                Config.SetOption(key, value);
            }
                #region Error handling
            catch (KeyNotFoundException)
            {
                Handler.Output("Configuration error", string.Format("Unknown option '{0}'", key));
                return 1;
            }
            catch (FormatException ex)
            {
                Handler.Output("Configuration error", ex.Message);
                return 1;
            }
            #endregion

            Config.Save();
            return 0;
        }
        #endregion
    }
}
