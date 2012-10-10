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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Common.Utils;
using ZeroInstall.Model;

namespace ZeroInstall.Injector.Solver
{
    /// <summary>
    /// A set of requirements/restrictions imposed by the user on the implementation selection process.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    public class Requirements : ICloneable, IEquatable<Requirements>
    {
        private string _interfaceID;

        /// <summary>
        /// The URI or local path (must be absolute) to the interface to solve the dependencies for.
        /// </summary>
        /// <exception cref="InvalidInterfaceIDException">Thrown when trying to set an invalid interface ID.</exception>
        public string InterfaceID
        {
            get { return _interfaceID; }
            set
            {
                ModelUtils.ValidateInterfaceID(value);
                _interfaceID = value;
            }
        }

        /// <summary>
        /// The name of the command in the implementation to execute.
        /// </summary>
        /// <remarks>Will default to <see cref="Command.NameRun"/> if <see langword="null"/>. Will not try to find any command if set to <see cref="string.Empty"/>.</remarks>
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
        /// The range of versions of the implementation that can be chosen. <see langword="null"/> for no limit.
        /// </summary>
        public VersionRange Versions { get; set; }

        private readonly Dictionary<string, VersionRange> _versionsFor = new Dictionary<string, VersionRange>();

        /// <summary>
        /// The ranges of versions of specific sub-implementations that can be chosen.
        /// </summary>
        public IDictionary<string, VersionRange> VersionsFor { get { return _versionsFor; } }

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Requirements"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Requirements"/>.</returns>
        public Requirements Clone()
        {
            var requirements = new Requirements {InterfaceID = InterfaceID, CommandName = CommandName, Architecture = Architecture, Versions = Versions};
            requirements._languages.AddRange(_languages);

            return requirements;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion

        #region Conversion
        /// <summary>
        /// Returns the requirements in the form "InterfaceID (CommandName)". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            if (Versions == null) return string.Format("{0} ({1})", InterfaceID, CommandName);
            else return string.Format("{0} ({1}): {2}", InterfaceID, CommandName, Versions);
        }

        /// <summary>
        /// Transforms the requirements into a command-line argument string.
        /// </summary>
        public string ToCommandLineArgs()
        {
            var builder = new StringBuilder();

            if (CommandName != null) builder.Append("--command=" + StringUtils.EscapeArgument(CommandName) + " ");
            if (Architecture.Cpu == Cpu.Source) builder.Append("--source ");
            else
            {
                if (Architecture.OS != OS.All) builder.Append("--os=" + StringUtils.EscapeArgument(Architecture.OSString) + " ");
                if (Architecture.Cpu != Cpu.All) builder.Append("--cpu=" + StringUtils.EscapeArgument(Architecture.CpuString) + " ");
            }
            if (Versions != null) builder.Append("--version=" + StringUtils.EscapeArgument(Versions.ToString()) + " ");
            foreach (var pair in VersionsFor)
                builder.Append("--version-for=" + StringUtils.EscapeArgument(pair.Key) + " " +  StringUtils.EscapeArgument(pair.Value.ToString()) + " ");
            builder.Append(StringUtils.EscapeArgument(InterfaceID));

            return builder.ToString();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(Requirements other)
        {
            if (other == null) return false;
            if (InterfaceID != other.InterfaceID) return false;
            if (CommandName != other.CommandName) return false;
            if (Architecture != other.Architecture) return false;
            // ToDo: if (!_languages.SequencedEquals(other._languages)) return false;
            if (Versions != other.Versions) return false;
            return true;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(Requirements) && Equals((Requirements)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = (InterfaceID != null ? InterfaceID.GetHashCode() : 0);
                result = (result * 397) ^ (CommandName != null ? CommandName.GetHashCode() : 0);
                result = (result * 397) ^ Architecture.GetHashCode();
                // ToDo: result = (result * 397) ^ _languages.GetSequencedHashCode();
                result = (result * 397) ^ (Versions != null ? Versions.GetHashCode() : 0);
                return result;
            }
        }
        #endregion
    }
}
