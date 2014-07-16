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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using NanoByte.Common.Collections;
using NanoByte.Common.Utils;
using Newtonsoft.Json;
using ZeroInstall.Store.Model.Design;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// A set of requirements/restrictions imposed by the user on the <see cref="Implementation"/> selection process. Used as input for the solver.
    /// </summary>
    public class Requirements : IInterfaceID, ICloneable, IEquatable<Requirements>
    {
        #region Properties
        private string _interfaceID;

        /// <summary>
        /// The URI or local path (must be absolute) to the interface to solve the dependencies for.
        /// </summary>
        /// <exception cref="InvalidInterfaceIDException">Thrown when trying to set an invalid interface ID.</exception>
        [Description("The URI or local path (must be absolute) to the interface to solve the dependencies for.")]
        [JsonProperty("interface")]
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
        /// The name of the command in the implementation to execute. Will default to <see cref="Store.Model.Command.NameRun"/> or <see cref="Store.Model.Command.NameCompile"/> if <see langword="null"/>. Will not try to find any command if set to <see cref="string.Empty"/>.
        /// </summary>
        [Description("The name of the command in the implementation to execute. Will default to 'run' or 'compile' if null. Will not try to find any command if set to ''.")]
        [JsonProperty("command")]
        [TypeConverter(typeof(CommandNameConverter))]
        public string Command { get; set; }

        /// <summary>
        /// The architecture to find executables for. Find for the current system if left at default value.
        /// </summary>
        /// <remarks>Will default to <see cref="Store.Model.Architecture.CurrentSystem"/> if <see langword="null"/>. Will not try to find any command if set to <see cref="string.Empty"/>.</remarks>
        [Description("The architecture to find executables for. Find for the current system if left at default value.")]
        [JsonIgnore]
        public Architecture Architecture { get; set; }

        /// <summary>Used for JSON serialization.</summary>
        /// <seealso cref="Architecture"/>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [JsonProperty("source")]
        public bool Source { get { return Architecture.Cpu == Cpu.Source; } set { if (value) Architecture = new Architecture(Architecture.OS, Cpu.Source); } }

        /// <summary>Used for JSON serialization.</summary>
        /// <seealso cref="Architecture"/>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [JsonProperty("os")]
        public string OSString { get { return Architecture.OS.ConvertToString(); } set { Architecture = new Architecture(value.ConvertFromString<OS>(), Architecture.Cpu); } }

        /// <summary>Used for JSON serialization.</summary>
        /// <seealso cref="Architecture"/>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [JsonProperty("cpu")]
        public string CpuString { get { return Architecture.Cpu.ConvertToString(); } set { Architecture = new Architecture(Architecture.OS, value.ConvertFromString<Cpu>()); } }

        // Order is always alphabetical, duplicate entries are not allowed
        private LanguageSet _languages = new LanguageSet();

        /// <summary>
        /// The natural language(s) to look for.
        /// </summary>
        /// <example>For example, the value "en_GB fr" would be search for British English or French.</example>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Complete set can be replaced by PropertyGrid.")]
        [Description("The natural language(s) to look for.")]
        [JsonIgnore]
        public LanguageSet Languages
        {
            get { return _languages; }
            set
            {
                #region Sanity checks
                if (value == null) throw new ArgumentNullException("value");
                #endregion

                _languages = value;
            }
        }

        /// <summary>Used for JSON serialization.</summary>
        /// <seealso cref="Architecture"/>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        [JsonProperty("langs", DefaultValueHandling = DefaultValueHandling.Ignore), DefaultValue("")]
        public string LanguagesString { get { return _languages.ToString(); } set { _languages = new LanguageSet(value); } }

        private readonly Dictionary<string, VersionRange> _extraRestrictions = new Dictionary<string, VersionRange>();

        /// <summary>
        /// The ranges of versions of specific sub-implementations that can be chosen.
        /// </summary>
        [Description("The ranges of versions of specific sub-implementations that can be chosen.")]
        [JsonProperty("extra_restrictions")]
        public Dictionary<string, VersionRange> ExtraRestrictions { get { return _extraRestrictions; } }
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Requirements"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Requirements"/>.</returns>
        public Requirements Clone()
        {
            var requirements = new Requirements {InterfaceID = InterfaceID, Command = Command, Architecture = Architecture, Languages = new LanguageSet(Languages)};
            requirements.ExtraRestrictions.AddRange(ExtraRestrictions);
            return requirements;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion

        #region Conversion
        /// <summary>
        /// Returns the requirements in the form "InterfaceID (Command)". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.IsNullOrEmpty(Command) ? InterfaceID : InterfaceID + " (" + Command + ")";
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(Requirements other)
        {
            if (other == null) return false;
            if (InterfaceID != other.InterfaceID) return false;
            if (Command != other.Command) return false;
            if (Architecture != other.Architecture) return false;
            if (!_languages.SequencedEquals(other._languages)) return false;
            if (!_extraRestrictions.UnsequencedEquals(other._extraRestrictions)) return false;
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
                result = (result * 397) ^ (Command != null ? Command.GetHashCode() : 0);
                result = (result * 397) ^ Architecture.GetHashCode();
                // ReSharper disable once NonReadonlyFieldInGetHashCode
                result = (result * 397) ^ _languages.GetSequencedHashCode();
                result = (result * 397) ^ _extraRestrictions.GetUnsequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
