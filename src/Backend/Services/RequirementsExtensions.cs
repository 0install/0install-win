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
        /// Transforms the requirements into a command-line arguments.
        /// </summary>
        [NotNull]
        public static string[] ToCommandLineArgs([NotNull] this Requirements requirements)
        {
            #region Sanity checks
            if (requirements == null) throw new ArgumentNullException("requirements");
            #endregion

            var args = new List<string>();

            if (requirements.Command != null) args.AddRange(new[] {"--command", requirements.Command});
            if (requirements.Architecture.Cpu == Cpu.Source) args.Add("--source");
            else
            {
                if (requirements.Architecture.OS != OS.All) args.AddRange(new[] {"--os", requirements.Architecture.OS.ConvertToString()});
                if (requirements.Architecture.Cpu != Cpu.All) args.AddRange(new[] {"--cpu", requirements.Architecture.Cpu.ConvertToString()});
            }
            //args.AddRange(new[] {"--languages", _languages.ToXmlString()});
            foreach (var pair in requirements.ExtraRestrictions)
                args.AddRange(new[] {"--version-for", pair.Key.ToStringRfc(), pair.Value.ToString()});
            args.Add(requirements.InterfaceUri.ToStringRfc());

            return args.ToArray();
        }
    }
}
