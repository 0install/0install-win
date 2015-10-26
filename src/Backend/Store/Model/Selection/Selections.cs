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
using System.Linq;
using System.Xml.Serialization;
using JetBrains.Annotations;
using NanoByte.Common.Collections;
using NanoByte.Common.Storage;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Model.Selection
{
    /// <summary>
    /// Represents a set of <see cref="ImplementationBase"/>s chosen by a solver.
    /// </summary>
    [Serializable, XmlRoot("selections", Namespace = Feed.XmlNamespace), XmlType("selections", Namespace = Feed.XmlNamespace)]
    public sealed class Selections : XmlUnknown, IInterfaceUri, ICloneable, IEquatable<Selections>
    {
        /// <summary>
        /// The URI or local path of the interface this selection is based on.
        /// </summary>
        [Description("The URI or local path of the interface this selection is based on.")]
        [XmlIgnore]
        public FeedUri InterfaceUri { get; set; }

        #region XML serialization
        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="InterfaceUri"/>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Used for XML serialization")]
        [XmlAttribute("interface"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public string InterfaceUriString { get { return (InterfaceUri == null) ? null : InterfaceUri.ToStringRfc(); } set { InterfaceUri = (value == null) ? null : new FeedUri(value); } }
        #endregion

        /// <summary>
        /// Indicates whether the selection was generated for <see cref="Cpu.Source"/>.
        /// </summary>
        [XmlAttribute("source")]
        public bool Source { get; set; }

        /// <summary>
        /// The name of the <see cref="Command"/> in the interface to be started.
        /// </summary>
        [Description("The name of the command in the interface to be started.")]
        [XmlAttribute("command")]
        public string Command { get; set; }

        private readonly List<ImplementationSelection> _implementations = new List<ImplementationSelection>();

        /// <summary>
        /// A list of <see cref="ImplementationSelection"/>s chosen in this selection.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Used for XML serialization")]
        [Description("A list of implementations chosen in this selection.")]
        [XmlElement("selection"), NotNull]
        public List<ImplementationSelection> Implementations { get { return _implementations; } }

        /// <summary>
        /// The main implementation in the selection (the actual program to launch). Identified by <see cref="InterfaceUri"/>.
        /// </summary>
        /// <exception cref="KeyNotFoundException">No <see cref="ImplementationSelection"/> matching <see cref="InterfaceUri"/> was found in <see cref="Implementations"/>.</exception>
        [XmlIgnore]
        public ImplementationSelection MainImplementation { get { return this[InterfaceUri]; } }

        #region Constructor
        /// <summary>
        /// Creates an empty selections document.
        /// </summary>
        public Selections()
        {}

        /// <summary>
        /// Creates a selections document prefilled with <see cref="ImplementationSelection"/>s.
        /// </summary>
        public Selections(IEnumerable<ImplementationSelection> implementations)
        {
            Implementations.AddRange(implementations);
        }
        #endregion

        //--------------------//

        #region Normalize
        /// <summary>
        /// Calls <see cref="ImplementationBase.Normalize"/> for all <see cref="Implementations"/>.
        /// </summary>
        public void Normalize()
        {
            foreach (var implementation in Implementations)
                implementation.Normalize(implementation.FromFeed ?? implementation.InterfaceUri);
        }
        #endregion

        #region Query
        /// <summary>
        /// Determines whether an <see cref="ImplementationSelection"/> for a specific interface is listed in the selection.
        /// </summary>
        /// <param name="interfaceUri">The <see cref="ImplementationSelection.InterfaceUri"/> to look for.</param>
        /// <returns><see langword="true"/> if an implementation was found; <see langword="false"/> otherwise.</returns>
        public bool ContainsImplementation([NotNull] FeedUri interfaceUri)
        {
            return Implementations.Any(implementation => implementation.InterfaceUri == interfaceUri);
        }

        /// <summary>
        /// Returns the <see cref="ImplementationSelection"/> for a specific interface.
        /// </summary>
        /// <param name="interfaceUri">The <see cref="ImplementationSelection.InterfaceUri"/> to look for.</param>
        /// <returns>The first matching implementation.</returns>
        /// <exception cref="KeyNotFoundException">No matching implementation was found.</exception>
        [NotNull]
        public ImplementationSelection this[[NotNull] FeedUri interfaceUri]
        {
            get
            {
                #region Sanity checks
                if (interfaceUri == null) throw new ArgumentNullException("interfaceUri");
                #endregion

                try
                {
                    return Implementations.First(implementation => implementation.InterfaceUri == interfaceUri);
                }
                    #region Error handling
                catch (InvalidOperationException)
                {
                    throw new KeyNotFoundException(string.Format(Resources.ImplementationNotInSelection, interfaceUri));
                }
                #endregion
            }
        }

        /// <summary>
        /// Returns the <see cref="ImplementationSelection"/> for a specific interface. Safe for missing elements.
        /// </summary>
        /// <param name="interfaceUri">The <see cref="ImplementationSelection.InterfaceUri"/> to look for.</param>
        /// <returns>The first matching implementation; <see langword="null"/> if no matching one was found.</returns>
        [CanBeNull]
        public ImplementationSelection GetImplementation([NotNull] FeedUri interfaceUri)
        {
            #region Sanity checks
            if (interfaceUri == null) throw new ArgumentNullException("interfaceUri");
            #endregion

            return Implementations.FirstOrDefault(implementation => implementation.InterfaceUri == interfaceUri);
        }
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Selections"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Selections"/>.</returns>
        public Selections Clone()
        {
            var selections = new Selections {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, InterfaceUri = InterfaceUri, Command = Command};
            selections.Implementations.AddRange(Implementations.CloneElements());
            return selections;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion

        #region Conversion
        /// <summary>
        /// Returns the selections as XML. Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return this.ToXmlString();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(Selections other)
        {
            if (other == null) return false;
            if (!base.Equals(other)) return false;
            if (InterfaceUri != other.InterfaceUri) return false;
            if (Command != other.Command) return false;
            if (!Implementations.UnsequencedEquals(other.Implementations)) return false;
            return true;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Selections && Equals((Selections)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                if (InterfaceUri != null) result = (result * 397) ^ InterfaceUri.GetHashCode();
                if (Command != null) result = (result * 397) ^ Command.GetHashCode();
                result = (result * 397) ^ Implementations.GetUnsequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
