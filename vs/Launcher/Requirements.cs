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
using System.Globalization;
using System.IO;
using Common.Utils;
using NDesk.Options;
using ZeroInstall.Launcher.Properties;
using ZeroInstall.Model;

namespace ZeroInstall.Launcher
{
    /// <summary>
    /// A set of requirements/restrictions imposed by the user on the implementation selection process.
    /// </summary>
    [Serializable]
    public class Requirements
    {
        #region Properties
        private string _interfaceID;
        /// <summary>
        /// The URI or local path (must be absolute) to the interface to solve the dependencies for.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when trying to set an invalid interface ID.</exception>
        public string InterfaceID
        {
            get { return _interfaceID; }
            set
            {
                if (!string.IsNullOrEmpty(value) && !Path.IsPathRooted(value))
                {
                    if (!value.StartsWith("http:") && !value.StartsWith("https:")) throw new ArgumentException(string.Format(Resources.InvalidInterfaceID, value));
                    if (StringUtils.CountOccurences(value, '/') < 3) throw new ArgumentException(string.Format(Resources.MissingSlashInUri, value));
                }

                _interfaceID = value;
            }
        }

        /// <summary>
        /// The name of the command in the implementation to execute.
        /// </summary>
        /// <remarks>Will default to <see cref="Command.NameRun"/> if <see langword="null"/>. Will remove all commands if set to <see cref="string.Empty"/>.</remarks>
        public string CommandName { get; set; }

        /// <summary>
        /// The architecture to find executables for. Find for the current system if left at default value.
        /// </summary>
        public Architecture Architecture { get; set; }

        private readonly List<CultureInfo> _languages = new List<CultureInfo>();
        /// <summary>
        /// The preferred languages for implementations in decreasing order. Use system locale if empty.
        /// </summary>
        public ICollection<CultureInfo> Languages { get { return _languages; } }

        /// <summary>
        /// The lowest-numbered version of the implementation that can be chosen.
        /// </summary>
        public ImplementationVersion NotBeforeVersion { get; set; }

        /// <summary>
        /// This version and all later versions of the implementation are unsuitable.
        /// </summary>
        public ImplementationVersion BeforeVersion { get; set; }
        #endregion

        //--------------------//

        #region Parsing
        /// <summary>
        /// Setus up an <see cref="OptionSet"/> to parse requirements listed in command-line arguments in the default format.
        /// </summary>
        internal void HookUpOptionParsing(OptionSet options)
        {
            #region Sanity checks
            if (options == null) throw new ArgumentNullException("options");
            #endregion

            options.Add("command=", Resources.OptionCommand, command => CommandName = command);
            options.Add("before=", Resources.OptionBefore, version => BeforeVersion = new ImplementationVersion(version));
            options.Add("not-before=", Resources.OptionNotBefore, version => NotBeforeVersion = new ImplementationVersion(version));
            options.Add("s|source", Resources.OptionSource, unused => Architecture = new Architecture(Architecture.OS, Cpu.Source));
            options.Add("os=", Resources.OptionOS, os => Architecture = new Architecture(Architecture.ParseOS(os), Architecture.Cpu));
            options.Add("cpu=", Resources.OptionCpu, cpu => Architecture = new Architecture(Architecture.OS, Architecture.ParseCpu(cpu)));
        }
        #endregion
    }
}
