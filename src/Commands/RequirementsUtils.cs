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
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Model;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Provides utility methods for <see cref="Requirements"/>.
    /// </summary>
    public static class RequirementsUtils
    {
        /// <summary>
        /// Configures an <see cref="OptionSet"/> parser to write data to a <see cref="Requirements"/> isntance.
        /// </summary>
        [CLSCompliant(false)]
        public static void FromCommandLine(this Requirements requirements, OptionSet options)
        {
            #region Sanity checks
            if (requirements == null) throw new ArgumentNullException("requirements");
            if (options == null) throw new ArgumentNullException("options");
            #endregion

            options.Add("command=", Resources.OptionCommand, command => requirements.CommandName = command);
            options.Add("version=", Resources.OptionVersionRange,
                (VersionRange range) => requirements.Versions = range);
            options.Add("version-for==", Resources.OptionVersionRangeFor,
                (string interfaceID, VersionRange range) => requirements.VersionsFor.Add(new VersionFor {InterfaceID = interfaceID, Versions = range}));
            options.Add("before=", Resources.OptionBefore, delegate(ImplementationVersion version)
            {
                if (requirements.Versions == null) requirements.Versions = new VersionRange();
                requirements.Versions = requirements.Versions.Intersect(new Constraint {Before = version});
            });
            options.Add("not-before=", Resources.OptionNotBefore, delegate(ImplementationVersion version)
            {
                if (requirements.Versions == null) requirements.Versions = new VersionRange();
                requirements.Versions = requirements.Versions.Intersect(new Constraint {NotBefore = version});
            });
            options.Add("s|source", Resources.OptionSource,
                unused => requirements.Architecture = new Architecture(requirements.Architecture.OS, Cpu.Source));
            options.Add("os=", Resources.OptionOS + "\n" + FrontendCommand.SupportedValues(Architecture.KnownOS),
                (OS os) => requirements.Architecture = new Architecture(os, requirements.Architecture.Cpu));
            options.Add("cpu=", Resources.OptionCpu + "\n" + FrontendCommand.SupportedValues(Architecture.KnownCpu),
                (Cpu cpu) => requirements.Architecture = new Architecture(requirements.Architecture.OS, cpu));
        }
    }
}
