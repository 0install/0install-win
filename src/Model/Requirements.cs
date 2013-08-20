/*
 * Copyright 2010-2013 Bastian Eicher
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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml.Serialization;
using Common.Collections;
using Common.Utils;

namespace ZeroInstall.Model
{
    /// <summary>
    /// A set of requirements/restrictions imposed by the user on the <see cref="Implementation"/> selection process.
    /// </summary>
    /// <remarks>This is used as input for the solver.</remarks>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [Description("A set of requirements/restrictions imposed by the user on the Implementation selection process.")]
    [XmlRoot("requirements", Namespace = Feed.XmlNamespace), XmlType("requirements", Namespace = Feed.XmlNamespace)]
    public class Requirements : ICloneable, IEquatable<Requirements>
    {
        #region Properites
        private string _interfaceID;

        /// <summary>
        /// The URI or local path (must be absolute) to the interface to solve the dependencies for.
        /// </summary>
        /// <exception cref="InvalidInterfaceIDException">Thrown when trying to set an invalid interface ID.</exception>
        [Description("The URI or local path (must be absolute) to the interface to solve the dependencies for.")]
        [XmlAttribute("interface")]
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
        /// <remarks>Will default to <see cref="Command.NameRun"/> or <see cref="Command.NameCompile"/> if <see langword="null"/>. Will not try to find any command if set to <see cref="string.Empty"/>.</remarks>
        [Description("The name of the command in the implementation to execute.")]
        [XmlAttribute("command")]
        public string CommandName { get; set; }

        /// <summary>
        /// The architecture to find executables for. Find for the current system if left at default value.
        /// </summary>
        /// <remarks>Will default to <see cref="Model.Architecture.CurrentSystem"/> if <see langword="null"/>. Will not try to find any command if set to <see cref="string.Empty"/>.</remarks>
        [Description("The architecture to find executables for. Find for the current system if left at default value.")]
        [XmlIgnore]
        public Architecture Architecture { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Architecture"/>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        [XmlAttribute("arch"), DefaultValue("*-*")]
        public string ArchitectureString { get { return Architecture.ToString(); } set { Architecture = new Architecture(value); } }

        // Order is always alphabetical, duplicate entries are not allowed
        private LanguageSet _languages = new LanguageSet();

        /// <summary>
        /// The natural language(s) to look for.
        /// </summary>
        /// <example>For example, the value "en_GB fr" would be search for British English or French.</example>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Complete set can be replaced by PropertyGrid.")]
        [Description("The natural language(s) to look for.")]
        [XmlIgnore]
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

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Architecture"/>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        [XmlAttribute("langs"), DefaultValue("")]
        public string LanguagesString { get { return _languages.ToString(); } set { _languages = new LanguageSet(value); } }

        /// <summary>
        /// The range of versions of the implementation that can be chosen. <see langword="null"/> for no limit.
        /// </summary>
        [Description("The range of versions of the implementation that can be chosen. null for no limit.")]
        [XmlIgnore]
        public VersionRange Versions { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Versions"/>
        [XmlAttribute("version"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public string VersionsString { get { return (Versions == null) ? null : Versions.ToString(); } set { Versions = string.IsNullOrEmpty(value) ? null : new VersionRange(value); } }

        // Preserve order
        private readonly C5.ArrayList<VersionFor> _versionsFor = new C5.ArrayList<VersionFor>();

        /// <summary>
        /// The ranges of versions of specific sub-implementations that can be chosen.
        /// </summary>
        [Description("The ranges of versions of specific sub-implementations that can be chosen.")]
        [XmlElement("version-for")]
        public C5.ArrayList<VersionFor> VersionsFor { get { return _versionsFor; } }
        #endregion

        //--------------------//

        #region Normalize
        /// <summary>
        /// Fills in missing values.
        /// </summary>
        public void Normalize()
        {
            Architecture = new Architecture(
                (Architecture.OS == OS.All) ? Architecture.CurrentSystem.OS : Architecture.OS,
                (Architecture.Cpu == Cpu.All) ? Architecture.CurrentSystem.Cpu : Architecture.Cpu);
            if (CommandName == null)
                CommandName = (Architecture.Cpu == Cpu.Source ? Command.NameCompile : Command.NameRun);
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Requirements"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Requirements"/>.</returns>
        public Requirements Clone()
        {
            var requirements = new Requirements {InterfaceID = InterfaceID, CommandName = CommandName, Architecture = Architecture, Versions = Versions};
            requirements._languages.AddAll(_languages);
            requirements._versionsFor.AddAll(_versionsFor);

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

            if (CommandName != null) builder.Append("--command=" + CommandName.EscapeArgument() + " ");
            if (Architecture.Cpu == Cpu.Source) builder.Append("--source ");
            else
            {
                if (Architecture.OS != OS.All) builder.Append("--os=" + Architecture.OS.ConvertToString().EscapeArgument() + " ");
                if (Architecture.Cpu != Cpu.All) builder.Append("--cpu=" + Architecture.Cpu.ConvertToString().EscapeArgument() + " ");
            }
            //builder.Append("--languages=" + _languages.ToXmlString().EscapeArgument() + " ");
            if (Versions != null) builder.Append("--version=" + Versions.ToString().EscapeArgument() + " ");
            foreach (var pair in VersionsFor)
                builder.Append("--version-for=" + pair.InterfaceID.EscapeArgument() + " " + pair.Versions.ToString().EscapeArgument() + " ");
            builder.Append(InterfaceID.EscapeArgument());

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
            if (Versions != other.Versions) return false;
            if (!_versionsFor.SequencedEquals(other._versionsFor)) return false;
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
                // ReSharper disable once NonReadonlyFieldInGetHashCode
                result = (result * 397) ^ _languages.GetSequencedHashCode();
                result = (result * 397) ^ (Versions != null ? Versions.GetHashCode() : 0);
                result = (result * 397) ^ _versionsFor.GetSequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
