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
using System.Linq;
using System.Xml.Serialization;
using NanoByte.Common.Collections;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Model.Selection
{
    /// <summary>
    /// Represents a set of <see cref="ImplementationBase"/>s chosen by a solver.
    /// </summary>
    [XmlRoot("selections", Namespace = Feed.XmlNamespace), XmlType("selections", Namespace = Feed.XmlNamespace)]
    public sealed class Selections : XmlUnknown, IInterfaceID, ICloneable, IEquatable<Selections>
    {
        #region Properties
        /// <summary>
        /// The URI or local path of the interface this selection is based on.
        /// </summary>
        [Description("The URI or local path of the interface this selection is based on.")]
        [XmlAttribute("interface")]
        public string InterfaceID { get; set; }

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
        [XmlElement("selection")]
        public List<ImplementationSelection> Implementations { get { return _implementations; } }

        /// <summary>
        /// The main implementation in the selection (the actual program to launch). Identified by <see cref="InterfaceID"/>.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown if no <see cref="ImplementationSelection"/> matching <see cref="InterfaceID"/> was found in <see cref="Implementations"/>.</exception>
        [XmlIgnore]
        public ImplementationSelection MainImplementation { get { return this[InterfaceID]; } }
        #endregion

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
                implementation.Normalize(implementation.FromFeed ?? implementation.InterfaceID);
        }
        #endregion

        #region Query
        /// <summary>
        /// Determines whether an <see cref="ImplementationSelection"/> for a specific interface is listed in the selection.
        /// </summary>
        /// <param name="interfaceID">The <see cref="ImplementationSelection.InterfaceID"/> to look for.</param>
        /// <returns><see langword="true"/> if an implementation was found; <see langword="false"/> otherwise.</returns>
        public bool ContainsImplementation(string interfaceID)
        {
            return _implementations.Any(implementation => implementation.InterfaceID == interfaceID);
        }

        /// <summary>
        /// Returns the <see cref="ImplementationSelection"/> for a specific interface.
        /// </summary>
        /// <param name="interfaceID">The <see cref="ImplementationSelection.InterfaceID"/> to look for.</param>
        /// <returns>The first matching implementation.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if no matching implementation was found.</exception>
        public ImplementationSelection this[string interfaceID]
        {
            get
            {
                #region Sanity checks
                if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
                #endregion

                try
                {
                    return _implementations.First(implementation => implementation.InterfaceID == interfaceID);
                }
                    #region Error handling
                catch (InvalidOperationException)
                {
                    throw new KeyNotFoundException(string.Format(Resources.ImplementationNotInSelection, interfaceID));
                }
                #endregion
            }
        }

        /// <summary>
        /// Returns the <see cref="ImplementationSelection"/> for a specific interface. Safe for missing elements.
        /// </summary>
        /// <param name="interfaceID">The <see cref="ImplementationSelection.InterfaceID"/> to look for.</param>
        /// <returns>The first matching implementation; <see langword="null"/> if no matching one was found.</returns>
        public ImplementationSelection GetImplementation(string interfaceID)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            #endregion

            return _implementations.FirstOrDefault(implementation => implementation.InterfaceID == interfaceID);
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
            var selections = new Selections {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, InterfaceID = InterfaceID, Command = Command};
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
        /// Returns the selections in the form "InterfaceID (Command): Implementations". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0} ({1}): {2}", InterfaceID, Command, Implementations);
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(Selections other)
        {
            if (other == null) return false;
            if (!base.Equals(other)) return false;
            if (InterfaceID != other.InterfaceID) return false;
            if (Command != other.Command) return false;
            if (!Implementations.SequencedEquals(other.Implementations)) return false;
            return true;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Feed && Equals((Feed)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                if (InterfaceID != null) result = (result * 397) ^ InterfaceID.GetHashCode();
                if (Command != null) result = (result * 397) ^ Command.GetHashCode();
                result = (result * 397) ^ Implementations.GetSequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
