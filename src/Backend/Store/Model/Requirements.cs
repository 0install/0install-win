/*
 * Copyright 2010-2015 Bastian Eicher
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
using System.Xml.Serialization;
using JetBrains.Annotations;
using NanoByte.Common.Collections;
using NanoByte.Common.Values;
using Newtonsoft.Json;
using ZeroInstall.Store.Model.Design;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// A set of requirements/restrictions imposed by the user on the <see cref="Implementation"/> selection process. Used as input for the solver.
    /// </summary>
    [Serializable, XmlRoot("requirements", Namespace = Feed.XmlNamespace), XmlType("requirements", Namespace = Feed.XmlNamespace)]
    public class Requirements : IInterfaceUri, ICloneable, IEquatable<Requirements>
    {
        /// <summary>
        /// The URI or local path (must be absolute) to the interface to solve the dependencies for.
        /// </summary>
        [Description("The URI or local path (must be absolute) to the interface to solve the dependencies for.")]
        [XmlIgnore, JsonProperty("interface")]
        public FeedUri InterfaceUri { get; set; }

        /// <summary>
        /// The name of the command in the implementation to execute. Will default to <see cref="Store.Model.Command.NameRun"/> or <see cref="Store.Model.Command.NameCompile"/> if <c>null</c>. Will not try to find any command if set to <see cref="string.Empty"/>.
        /// </summary>
        [Description("The name of the command in the implementation to execute. Will default to 'run' or 'compile' if null. Will not try to find any command if set to ''.")]
        [TypeConverter(typeof(CommandNameConverter))]
        [XmlAttribute("command"), JsonProperty("command"), CanBeNull]
        public string Command { get; set; }

        // Order is always alphabetical, duplicate entries are not allowed

        private LanguageSet _languages = new LanguageSet();

        /// <summary>
        /// The preferred languages for the implementation.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Complete set can be replaced by PropertyGrid.")]
        [Description("The preferred languages for the implementation.")]
        [XmlIgnore, JsonIgnore]
        public LanguageSet Languages { get { return _languages; } }

        /// <summary>
        /// The architecture to find executables for. Find for the current system if left at default value.
        /// </summary>
        /// <remarks>Will default to <see cref="Store.Model.Architecture.CurrentSystem"/> if left at default value. Will not try to find any command if set to <see cref="string.Empty"/>.</remarks>
        [Description("The architecture to find executables for. Find for the current system if left at default value.")]
        [XmlIgnore, JsonIgnore]
        public Architecture Architecture { get; set; }

        #region XML/JSON serialization
        /// <summary>Used for XMLserialization.</summary>
        /// <seealso cref="InterfaceUri"/>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Used for XML serialization")]
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        [XmlAttribute("interface"), JsonIgnore]
        public string InterfaceUriString { get { return (InterfaceUri == null) ? null : InterfaceUri.ToStringRfc(); } set { InterfaceUri = (value == null) ? null : new FeedUri(value); } }

        /// <summary>Used for XML and JSON serialization.</summary>
        /// <seealso cref="Languages"/>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        [DefaultValue(""), JsonProperty("langs", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string LanguagesString { get { return _languages.ToString(); } set { _languages = new LanguageSet(value); } }

        /// <summary>Used for XML and JSON serialization.</summary>
        /// <seealso cref="Architecture"/>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DefaultValue(false), XmlAttribute("source"), JsonProperty("source")]
        public bool Source { get { return Architecture.Cpu == Cpu.Source; } set { if (value) Architecture = new Architecture(Architecture.OS, Cpu.Source); } }

        /// <summary>Used for XML and JSON serialization.</summary>
        /// <seealso cref="Architecture"/>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DefaultValue("*"), XmlAttribute("os"), JsonProperty("os", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string OSString { get { return Architecture.OS.ConvertToString(); } set { Architecture = new Architecture(value.ConvertFromString<OS>(), Architecture.Cpu); } }

        /// <summary>Used for XML and JSON serialization.</summary>
        /// <seealso cref="Architecture"/>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DefaultValue("*"), XmlAttribute("machine"), JsonProperty("cpu", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string CpuString { get { return Architecture.Cpu.ConvertToString(); } set { Architecture = new Architecture(Architecture.OS, value.ConvertFromString<Cpu>()); } }
        #endregion

        private readonly Dictionary<FeedUri, VersionRange> _extraRestrictions = new Dictionary<FeedUri, VersionRange>();

        /// <summary>
        /// The ranges of versions of specific sub-implementations that can be chosen.
        /// </summary>
        [Description("The ranges of versions of specific sub-implementations that can be chosen.")]
        [XmlIgnore, JsonProperty("extra_restrictions"), NotNull]
        public Dictionary<FeedUri, VersionRange> ExtraRestrictions { get { return _extraRestrictions; } }

        // Order is not important (but is preserved), duplicate entries are not allowed (but not enforced)
        private readonly List<string> _distributions = new List<string>();

        /// <summary>
        /// Specifies that the selected implementations must be from one of the given distributions (e.g. Debian, RPM).
        /// The special value <see cref="Restriction.DistributionZeroInstall"/> may be used to require implementations provided by Zero Install (i.e. one not provided by a <see cref="PackageImplementation"/>).
        /// </summary>
        /// <remarks>Used internally by solvers, copied from <see cref="Restriction.Distributions"/>, not set directly by user, not serialized.</remarks>
        [Browsable(false)]
        [XmlIgnore, JsonIgnore, NotNull]
        public ICollection<string> Distributions { get { return _distributions; } }

        #region Constructor
        /// <summary>
        /// Cretes an empty requirements object. Use this to fill in values incrementally, e.g. when parsing command-line arguments.
        /// </summary>
        public Requirements()
        {}

        /// <summary>
        /// Creates a new requirements object.
        /// </summary>
        /// <param name="interfaceUri">The URI or local path (must be absolute) to the interface to solve the dependencies for.</param>
        /// <param name="command">he name of the command in the implementation to execute. Will default to <see cref="Store.Model.Command.NameRun"/> or <see cref="Store.Model.Command.NameCompile"/> if <c>null</c>. Will not try to find any command if set to <see cref="string.Empty"/>.</param>
        /// <param name="architecture">The architecture to find executables for. Find for the current system if left at default value.</param>
        public Requirements([NotNull] FeedUri interfaceUri, [CanBeNull] string command = null, Architecture architecture = default(Architecture))
        {
            InterfaceUri = interfaceUri;
            Command = command;
            Architecture = architecture;
        }

        /// <summary>
        /// Creates a new requirements object.
        /// </summary>
        /// <param name="interfaceUri">The URI or local path (must be absolute) to the interface to solve the dependencies for. Must be an HTTP(S) URL or an absolute local path.</param>
        /// <exception cref="UriFormatException"><paramref name="interfaceUri"/> is not a valid HTTP(S) URL or an absolute local path.</exception>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Justification = "Convenience overload that internally calls the Uri version")]
        public Requirements([NotNull] string interfaceUri)
            : this(new FeedUri(interfaceUri))
        {}

        /// <summary>
        /// Creates a new requirements object.
        /// </summary>
        /// <param name="interfaceUri">The URI or local path (must be absolute) to the interface to solve the dependencies for. Must be an HTTP(S) URL or an absolute local path.</param>
        /// <param name="command">he name of the command in the implementation to execute. Will default to <see cref="Store.Model.Command.NameRun"/> or <see cref="Store.Model.Command.NameCompile"/> if <c>null</c>. Will not try to find any command if set to <see cref="string.Empty"/>.</param>
        /// <param name="architecture">The architecture to find executables for. Find for the current system if left at default value.</param>
        /// <exception cref="UriFormatException"><paramref name="interfaceUri"/> is not a valid HTTP(S) URL or an absolute local path.</exception>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Justification = "Convenience overload that internally calls the Uri version")]
        public Requirements([NotNull] string interfaceUri, [CanBeNull] string command = null, Architecture architecture = default(Architecture))
            : this(new FeedUri(interfaceUri), command, architecture)
        {}
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Requirements"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Requirements"/>.</returns>
        public Requirements Clone()
        {
            var requirements = new Requirements(InterfaceUri, Command, Architecture);
            requirements.Languages.AddRange(Languages);
            requirements.ExtraRestrictions.AddRange(ExtraRestrictions);
            requirements.Distributions.AddRange(Distributions);
            return requirements;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion

        #region Conversion
        /// <summary>
        /// Returns the requirements in the form "InterfaceUri (Command)". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.IsNullOrEmpty(Command) ? InterfaceUri.ToStringRfc() : InterfaceUri.ToStringRfc() + " (" + Command + ")";
        }

        /// <summary>
        /// Transforms the requirements into a command-line arguments.
        /// </summary>
        [NotNull]
        public string[] ToCommandLineArgs()
        {
            var args = new List<string>();

            if (Command != null) args.AddRange(new[] {"--command", Command});
            if (Architecture.Cpu == Cpu.Source) args.Add("--source");
            else
            {
                if (Architecture.OS != OS.All) args.AddRange(new[] {"--os", Architecture.OS.ConvertToString()});
                if (Architecture.Cpu != Cpu.All) args.AddRange(new[] {"--cpu", Architecture.Cpu.ConvertToString()});
            }
            foreach (var language in Languages)
                args.AddRange(new[] {"--language", language.ToString()});
            foreach (var pair in ExtraRestrictions)
                args.AddRange(new[] {"--version-for", pair.Key.ToStringRfc(), pair.Value.ToString()});
            args.Add(InterfaceUri.ToStringRfc());

            return args.ToArray();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(Requirements other)
        {
            if (other == null) return false;
            if (InterfaceUri != other.InterfaceUri) return false;
            if (Command != other.Command) return false;
            if (Architecture != other.Architecture) return false;
            if (!Languages.SequencedEquals(other.Languages)) return false;
            if (!ExtraRestrictions.UnsequencedEquals(other.ExtraRestrictions)) return false;
            if (!Distributions.UnsequencedEquals(other.Distributions)) return false;
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
                int result = (InterfaceUri != null ? InterfaceUri.GetHashCode() : 0);
                result = (result * 397) ^ (Command != null ? Command.GetHashCode() : 0);
                result = (result * 397) ^ Architecture.GetHashCode();
                result = (result * 397) ^ Languages.GetSequencedHashCode();
                result = (result * 397) ^ ExtraRestrictions.GetUnsequencedHashCode();
                result = (result * 397) ^ Distributions.GetUnsequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
