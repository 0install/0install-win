/*
 * Copyright 2010-2016 Bastian Eicher
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
using JetBrains.Annotations;
using NanoByte.Common.Collections;
using ZeroInstall.Store.Model.Design;

namespace ZeroInstall.Store.Model.Capabilities
{
    /// <summary>
    /// The mapping of an action/verb (e.g. open, edit) to a <see cref="Store.Model.Command"/>.
    /// </summary>
    [Description("The mapping of an action/verb (e.g. open, edit) to a Command.")]
    [Serializable, XmlRoot("verb", Namespace = CapabilityList.XmlNamespace), XmlType("verb", Namespace = CapabilityList.XmlNamespace)]
    public sealed class Verb : XmlUnknown, IDescriptionContainer, ICloneable, IEquatable<Verb>
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
        public const string NameEdit = "edit";

        /// <summary>
        /// Canonical <see cref="Name"/> for opening a media file and starting playback immediately.
        /// </summary>
        public const string NamePlay = "play";

        /// <summary>
        /// Canonical <see cref="Name"/> for printing a file while displaying as little as necessary to complete the task.
        /// </summary>
        public const string NamePrint = "print";

        /// <summary>
        /// Canonical <see cref="Name"/> for displaying a quick, simple response that allows the user to rapidly preview and dismiss items.
        /// </summary>
        public const string NamePreview = "Preview";
        #endregion

        /// <summary>
        /// The name of the verb. Use canonical names to get automatic localization; specify <see cref="Descriptions"/> otherwise.
        /// </summary>
        [Description("The name of the verb. Use canonical names to get automatic localization; specify Descriptions otherwise.")]
        [TypeConverter(typeof(VerbNameConverter))]
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// The name of the command in the <see cref="Feed"/> to use when launching via this capability; leave <c>null</c> for <see cref="Store.Model.Command.NameRun"/>.
        /// </summary>
        [Description("The name of the command in the feed to use when launching via this capability; leave empty for 'run'.")]
        [TypeConverter(typeof(CommandNameConverter))]
        [XmlAttribute("command"), DefaultValue(""), CanBeNull]
        public string Command { get; set; }

        /// <summary>
        /// A custom arguments list to be passed to the command. %1 will be replaced with the path of the file being opened.
        /// </summary>
        [Description("A custom arguments list to be passed to the command. %1 will be replaced with the path of the file being opened.")]
        [XmlAttribute("args"), DefaultValue("")]
        public string Arguments { get; set; }

        /// <summary>
        /// Set this to <c>true</c> to hide the verb in the Windows context menu unless the Shift key is pressed when opening the menu.
        /// </summary>
        [Description("Set this to true to hide the verb in the Windows context menu unless the Shift key is pressed when opening the menu.")]
        [XmlAttribute("extended"), DefaultValue(false)]
        public bool Extended { get; set; }

        /// <inheritdoc/>
        [Browsable(false)]
        [XmlElement("description")]
        public LocalizableStringCollection Descriptions { get; } = new LocalizableStringCollection();

        #region Conversion
        /// <summary>
        /// Returns the extension in the form "Name = Command". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return $"{Name} = {Command ?? Model.Command.NameRun}";
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
            newVerb.Descriptions.AddRange(Descriptions.CloneElements());
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
            return base.Equals(other) && other.Name == Name && other.Command == Command && other.Arguments == Arguments && other.Extended == Extended && Descriptions.UnsequencedEquals(other.Descriptions);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj == this) return true;
            return obj is Verb && Equals((Verb)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ Name?.GetHashCode() ?? 0;
                result = (result * 397) ^ Command?.GetHashCode() ?? 0;
                result = (result * 397) ^ Arguments?.GetHashCode() ?? 0;
                result = (result * 397) ^ Extended.GetHashCode();
                result = (result * 397) ^ Descriptions.GetUnsequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
