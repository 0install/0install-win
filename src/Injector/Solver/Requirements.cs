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
                Feed.ValidateInterfaceID(value);
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

        private readonly C5.LinkedList<CultureInfo> _languages = new C5.LinkedList<CultureInfo>();
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

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Requirements"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Requirements"/>.</returns>
        public Requirements CloneRequirements()
        {
            var requirements = new Requirements {InterfaceID = InterfaceID, CommandName = CommandName, Architecture = Architecture, NotBeforeVersion = NotBeforeVersion, BeforeVersion = BeforeVersion};
            requirements._languages.AddAll(_languages);

            return requirements;
        }

        /// <summary>
        /// Creates a deep copy of this <see cref="Requirements"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Requirements"/>.</returns>
        public object Clone()
        {
            return CloneRequirements();
        }
        #endregion

        #region Conversion
        /// <summary>
        /// Returns the requirements in the form "InterfaceID (CommandName)". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            if (NotBeforeVersion == null && BeforeVersion == null) return string.Format("{0} ({1})", InterfaceID, CommandName);
            else return string.Format("{0} ({1}): {2} <= Version < {3}", InterfaceID, CommandName, NotBeforeVersion, BeforeVersion);
        }

        /// <summary>
        /// Transforms the requirements into a command-line argument string.
        /// </summary>
        public string ToCommandLineArgs()
        {
            var builder = new StringBuilder();
            if (!string.IsNullOrEmpty(CommandName)) builder.Append("--command=" + StringUtils.Escape(CommandName) + " ");
            if (Architecture.OS != OS.All) builder.Append("--os=" + Architecture.OSToString() + " ");
            if (Architecture.Cpu != Cpu.All) builder.Append("--cpu=" + Architecture.CpuToString() + " ");
            // ToDo: Add Languages support
            if (NotBeforeVersion != null) builder.Append("--not-before=" + NotBeforeVersion + " ");
            if (BeforeVersion != null) builder.Append("--before=" + BeforeVersion + " ");
            builder.Append(StringUtils.Escape(InterfaceID));
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
            if (!_languages.SequencedEquals(other._languages)) return false;
            if (NotBeforeVersion != other.NotBeforeVersion) return false;
            if (BeforeVersion != other.BeforeVersion) return false;
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
                int result = (_interfaceID != null ? _interfaceID.GetHashCode() : 0);
                result = (result * 397) ^ (CommandName != null ? CommandName.GetHashCode() : 0);
                result = (result * 397) ^ Architecture.GetHashCode();
                result = (result * 397) ^ _languages.GetSequencedHashCode();
                result = (result * 397) ^ (NotBeforeVersion != null ? NotBeforeVersion.GetHashCode() : 0);
                result = (result * 397) ^ (BeforeVersion != null ? BeforeVersion.GetHashCode() : 0);
                return result;
            }
        }
        #endregion
    }
}
