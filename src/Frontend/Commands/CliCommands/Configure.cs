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
using JetBrains.Annotations;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Store;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// View or change <see cref="Config"/>.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class Configure : CliCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "config";

        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionConfig; } }

        /// <inheritdoc/>
        protected override string Usage { get { return "[NAME [VALUE|default]]"; } }

        /// <inheritdoc/>
        protected override int AdditionalArgsMax { get { return 2; } }
        #endregion

        #region State
        private ConfigTab _configTab;

        /// <inheritdoc/>
        public Configure([NotNull] ICommandHandler handler) : base(handler)
        {
            Options.Add("tab=", () => Resources.OptionConfigTab, (ConfigTab tab) => _configTab = tab);
        }
        #endregion

        /// <inheritdoc/>
        public override ExitCode Execute()
        {
            switch (AdditionalArgs.Count)
            {
                case 0:
                    Handler.ShowConfig(Config, _configTab);
                    Config.Save();
                    break;

                case 1:
                    GetOptions(AdditionalArgs[0]);
                    break;

                case 2:
                    SetOption(AdditionalArgs[0], AdditionalArgs[1]);
                    Config.Save();
                    break;
            }

            return ExitCode.OK;
        }

        private void GetOptions(string key)
        {
            try
            {
                Handler.Output(key, Config.GetOption(key));
            }
                #region Error handling
            catch (KeyNotFoundException)
            {
                throw new OptionException(string.Format(Resources.UnknownConfigKey, key), key);
            }
            #endregion
        }

        private void SetOption(string key, string value)
        {
            try
            {
                if (value == "default") Config.ResetOption(key);
                else Config.SetOption(key, value);
            }
                #region Error handling
            catch (KeyNotFoundException)
            {
                throw new OptionException(string.Format(Resources.UnknownConfigKey, key), key);
            }
            catch (FormatException ex)
            {
                throw new OptionException(ex.Message, key);
            }
            #endregion
        }
    }
}
