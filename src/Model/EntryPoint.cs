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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using Common.Collections;

namespace ZeroInstall.Model
{
    /// <summary>
    /// Associates a <see cref="Command"/> with a user-friendly name and description.
    /// </summary>
    /// <seealso cref="Feed.EntryPoints"/>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [Serializable]
    [XmlType("entry-point", Namespace = Feed.XmlNamespace)]
    public sealed class EntryPoint : XmlUnknown, ICloneable, IEquatable<EntryPoint>
    {
        #region Properties
        /// <summary>
        /// The name of the <see cref="Command"/>.
        /// </summary>
        [Description("The name of the command.")]
        [XmlAttribute("command")]
        public string Command { get; set; }

        /// <summary>
        /// The canonical name of the binary supplying the command (without file extensions). Used by desktop integration to better name stubs.
        /// </summary>
        [Description("The canonical name of the binary supplying the command (without file extensions). Used by desktop integration to better name stubs.")]
        [XmlAttribute("binary-name")]
        public string BinaryName { get; set; }

        /// <summary>
        /// If <see langword="true"/>, this element indicates that the entry point requires a terminal in order to run. Graphical launchers should therefore run this program in a suitable terminal emulator.
        /// </summary>
        [Category("Interface"), Description("If true, this element indicates that the entry point requires a terminal in order to run. Graphical launchers should therefore run this program in a suitable terminal emulator.")]
        [XmlIgnore, DefaultValue(false)]
        public bool NeedsTerminal { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="NeedsTerminal"/>
        [XmlElement("needs-terminal"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string NeedsTerminalString
        {
            get { return (NeedsTerminal ? "" : null); }
            set { NeedsTerminal = (value != null); }
        }

        private readonly LocalizableStringCollection _names = new LocalizableStringCollection();
        /// <summary>
        /// User-friendly names for the command in different languages.
        /// </summary>
        [Description("User-friendly names for the command in different languages.")]
        [XmlElement("name")]
        // Note: Can not use ICollection<T> interface because of XML Serialization
        public LocalizableStringCollection Names { get { return _names; } }

        private readonly LocalizableStringCollection _summaries = new LocalizableStringCollection();
        /// <summary>
        /// Short one-line descriptions for different languages; the first word should not be upper-case unless it is a proper noun (e.g. "cures all ills").
        /// </summary>
        [Description("Short one-line descriptions for different languages; the first word should not be upper-case unless it is a proper noun (e.g. \"cures all ills\").")]
        [XmlElement("summary")]
        // Note: Can not use ICollection<T> interface because of XML Serialization
        public LocalizableStringCollection Summaries { get { return _summaries; } }

        private readonly LocalizableStringCollection _descriptions = new LocalizableStringCollection();
        /// <summary>
        /// Full descriptions for different languages, which can be several paragraphs long.
        /// </summary>
        [Description("Full descriptions for different languages, which can be several paragraphs long.")]
        [XmlElement("description")]
        // Note: Can not use ICollection<T> interface because of XML Serialization
        public LocalizableStringCollection Descriptions { get { return _descriptions; } }

        // Preserve order
        private readonly C5.LinkedList<Icon> _icons = new C5.LinkedList<Icon>();
        /// <summary>
        /// Zero or more icons to use for the command.
        /// </summary>
        [Description("Zero or more icons to use for the command.")]
        [XmlElement("icon")]
        // Note: Can not use ICollection<T> interface because of XML Serialization
        public C5.LinkedList<Icon> Icons { get { return _icons; } }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the EntryPoint in the form "EntryPoint: Command". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return "EntryPoint: " + Command;
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="EntryPoint"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="EntryPoint"/>.</returns>
        public EntryPoint CloneEntryPoint()
        {
            var newEntryPoint = new EntryPoint {Command = Command, BinaryName = BinaryName, NeedsTerminal = NeedsTerminal};
            foreach (var name in Names) newEntryPoint.Names.Add(name.CloneString());
            foreach (var summary in Summaries) newEntryPoint.Summaries.Add(summary.CloneString());
            foreach (var description in Descriptions) newEntryPoint.Descriptions.Add(description.CloneString());
            newEntryPoint.Icons.AddAll(Icons);

            return newEntryPoint;
        }

        /// <summary>
        /// Creates a deep copy of this <see cref="EntryPoint"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="EntryPoint"/> casted to a generic <see cref="object"/>.</returns>
        public object Clone()
        {
            return CloneEntryPoint();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(EntryPoint other)
        {
            if (other == null) return false;

            return Command == other.Command && BinaryName == other.BinaryName && NeedsTerminal == other.NeedsTerminal &&
                Names.SequencedEquals(other.Names) && Summaries.SequencedEquals(other.Summaries) && Descriptions.SequencedEquals(other.Descriptions) && Icons.SequencedEquals(other.Icons);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(EntryPoint) && Equals((EntryPoint)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Command ?? "").GetHashCode();
                result = (result * 397) ^ (BinaryName ?? "").GetHashCode();
                result = (result * 397) ^ NeedsTerminal.GetHashCode();
                result = (result * 397) ^ Names.GetSequencedHashCode();
                result = (result * 397) ^ Summaries.GetSequencedHashCode();
                result = (result * 397) ^ Descriptions.GetSequencedHashCode();
                result = (result * 397) ^ Icons.GetSequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
