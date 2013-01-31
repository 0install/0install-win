﻿/*
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
using System.Xml.Serialization;
using Common.Collections;

namespace ZeroInstall.Model.Capabilities
{
    /// <summary>
    /// Describes the mapping of an action/verb (e.g. open, edit) to a <see cref="Model.Command"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [XmlType("verb", Namespace = Capability.XmlNamespace)]
    public sealed class Verb : XmlUnknown, ICloneable, IEquatable<Verb>
    {
        #region Constants
        /// <summary>
        /// Canonical <see cref="Name"/> for opening a file.
        /// </summary>
        public const string NameOpen = "open";

        /// <summary>
        /// Canonical <see cref="Name"/> for opening a file in a new window.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
        public const string NameOpenNew = "opennew";

        /// <summary>
        /// Canonical <see cref="Name"/> for opening a file in an application of the user's choice.
        /// </summary>
        public const string NameOpenAs = "openas";

        /// <summary>
        /// Canonical <see cref="Name"/> for opening a file in editing mode.
        /// </summary>
        public const string NameOpenEdit = "edit";

        /// <summary>
        /// Canonical <see cref="Name"/> for opening a media file and starting playback immediately.
        /// </summary>
        public const string NameOpenPlay = "play";

        /// <summary>
        /// Canonical <see cref="Name"/> for printing a file while displaying as little as necessary to complete the task.
        /// </summary>
        public const string NameOpenPrint = "print";

        /// <summary>
        /// Canonical <see cref="Name"/> for displaying a quick, simple response that allows the user to rapidly preview and dismiss items.
        /// </summary>
        public const string NameOpenPreview = "Preview";
        #endregion

        #region Properties
        /// <summary>
        /// The name of the action to perform. Use canonical names whenever possible.
        /// </summary>
        [Description("The name of the action. Use canonical names whenever possible.")]
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// The name of the command in the <see cref="Feed"/> to use when launching via this capability; leave <see langword="null"/> for <see cref="Model.Command.NameRun"/>.
        /// </summary>
        [Description("The name of the command in the feed to use when launching via this capability; leave null for 'run'.")]
        [XmlAttribute("command")]
        public string Command { get; set; }

        /// <summary>
        /// A custom arguments list to be passed to the command. %1 will be replaced with the path of the file being opened.
        /// </summary>
        [Description("A custom arguments list to be passed to the command. %1 will be replaced with the path of the file being opened.")]
        [XmlAttribute("args"), DefaultValue("")]
        public string Arguments { get; set; }

        /// <summary>
        /// Set this to <see langword="true"/> to hide the verb in the Windows context menu unless the Shift key is pressed when opening the menu.
        /// </summary>
        [Description("Set this to true to hide the verb in the Windows context menu unless the Shift key is pressed when opening the menu.")]
        [XmlAttribute("extended"), DefaultValue(false)]
        public bool Extended { get; set; }

        private readonly LocalizableStringCollection _descriptions = new LocalizableStringCollection();

        /// <summary>
        /// Localized human-readable descriptions of the verb as an alternative to <see cref="Name"/>.
        /// </summary>
        [Description("Localized human-readable descriptions of the verb as an alternative to Name.")]
        [XmlElement("description")]
        public LocalizableStringCollection Descriptions { get { return _descriptions; } }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the extension in the form "Name = Command". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0} = {1}", Name, Command);
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Verb"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Verb"/>.</returns>
        public Verb Clone()
        {
            var newVerb = new Verb {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, Name = Name, Command = Command, Arguments = Arguments, Extended = Extended};
            foreach (var description in Descriptions) newVerb.Descriptions.Add(description.Clone());

            return newVerb;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(Verb other)
        {
            if (other == null) return false;
            return base.Equals(other) && other.Name == Name && other.Command == Command && other.Arguments == Arguments && other.Extended == Extended && Descriptions.SequencedEquals(other.Descriptions);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Verb && Equals((Verb)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ (Name ?? "").GetHashCode();
                result = (result * 397) ^ (Command ?? "").GetHashCode();
                result = (result * 397) ^ (Arguments ?? "").GetHashCode();
                result = (result * 397) ^ Extended.GetHashCode();
                result = (result * 397) ^ Descriptions.GetSequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
