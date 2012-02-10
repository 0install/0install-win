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
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Injector;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// View or change <see cref="Config"/>.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class Configure : CommandBase
    {
        #region Constants
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public const string Name = "config";
        #endregion

        #region Properties
        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionConfig; } }

        /// <inheritdoc/>
        protected override string Usage { get { return "[NAME [VALUE]]"; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public Configure(Policy policy) : base(policy)
        {}
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        public override int Execute()
        {
            if (!IsParsed) throw new InvalidOperationException(Resources.NotParsed);

            switch (AdditionalArgs.Count)
            {
                case 0:
                    if (Policy.Handler.ShowConfig(Policy.Config))
                    { // Only save if the user confirmed the changes
                        Policy.Config.Save();
                    }
                    return 0;

                case 1:
                    return PrintOption(AdditionalArgs[0]);

                case 2:
                    return SetOption(AdditionalArgs[0], AdditionalArgs[1]);

                default:
                    throw new OptionException(Resources.TooManyArguments, "");
            }
        }
        #endregion

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
                value = Policy.Config.GetOption(key);
            }
                #region Error handling
            catch (KeyNotFoundException)
            {
                Policy.Handler.Output("Configuration error", string.Format("Unknown option '{0}'", key));
                return 1;
            }
            #endregion

            Policy.Handler.Output(key, value);
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
                Policy.Config.SetOption(key, value);
            }
                #region Error handling
            catch (KeyNotFoundException)
            {
                Policy.Handler.Output("Configuration error", string.Format("Unknown option '{0}'", key));
                return 1;
            }
            catch (FormatException ex)
            {
                Policy.Handler.Output("Configuration error", ex.Message);
                return 1;
            }
            #endregion

            Policy.Config.Save();
            return 0;
        }
        #endregion
    }
}
