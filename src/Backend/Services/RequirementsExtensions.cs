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
using System.Text;
using NanoByte.Common;
using NanoByte.Common.Values;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Services
{
    /// <summary>
    /// Provides extension methods for <see cref="Requirements"/>.
    /// </summary>
    public static class RequirementsExtensions
    {
        /// <summary>
        /// Transforms the requirements into a command-line argument string.
        /// </summary>
        public static string ToCommandLine(this Requirements requirements)
        {
            #region Sanity checks
            if (requirements == null) throw new ArgumentNullException("requirements");
            #endregion

            var builder = new StringBuilder();

            if (requirements.Command != null) builder.Append("--command=" + requirements.Command.EscapeArgument() + " ");
            if (requirements.Architecture.Cpu == Cpu.Source) builder.Append("--source ");
            else
            {
                if (requirements.Architecture.OS != OS.All) builder.Append("--os=" + requirements.Architecture.OS.ConvertToString().EscapeArgument() + " ");
                if (requirements.Architecture.Cpu != Cpu.All) builder.Append("--cpu=" + requirements.Architecture.Cpu.ConvertToString().EscapeArgument() + " ");
            }
            //builder.Append("--languages=" + _languages.ToXmlString().EscapeArgument() + " ");
            foreach (var pair in requirements.ExtraRestrictions)
                builder.Append("--version-for=" + pair.Key.EscapeArgument() + " " + pair.Value.ToString().EscapeArgument() + " ");
            builder.Append(requirements.InterfaceID.EscapeArgument());

            return builder.ToString();
        }
    }
}
