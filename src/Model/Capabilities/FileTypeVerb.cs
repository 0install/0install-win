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
using System.Xml.Serialization;

namespace ZeroInstall.Model.Capabilities
{
    /// <summary>
    /// Describes an action that can be performed on a file type.
    /// </summary>
    [XmlType("verb", Namespace = Capability.XmlNamespace)]
    public struct FileTypeVerb : IEquatable<FileTypeVerb>
    {
        #region Constants
        /// <summary>
        /// Canonical <see cref="Name"/> for opening a file.
        /// </summary>
        public const string NameOpen = "open";

        /// <summary>
        /// Canonical <see cref="Name"/> for opening a file in a new window.
        /// </summary>
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
        /// The name of the command in the <see cref="Feed"/> to use when launching via this capability.
        /// </summary>
        [Description("The name of the command in the feed to use when launching via this capability.")]
        [XmlAttribute("command")]
        public string Command { get; set; }

        /// <summary>
        /// A custom arguments list to be passed to the command. %1 will be replaced with the path of the file being opened.
        /// </summary>
        /// <remarks>Leaving this empty will pass in the path of the file being opened directly.</remarks>
        [Description("A custom arguments list to be passed to the command. %1 will be replaced with the path of the file being opened.")]
        [XmlAttribute("arguments")]
        public string Arguments { get; set; }

        /// <summary>
        /// Set this to <see langword="true"/> to hide the verb in the Windows context menu unless the Shift key is pressed when opening the menu.
        /// </summary>
        [Description("Set this to true to hide the verb in the Windows context menu unless the Shift key is pressed when opening the menu.")]
        [XmlAttribute("extended"), DefaultValue(false)]
        public bool Extended { get; set; }
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

        #region Equality
        /// <inheritdoc/>
        public bool Equals(FileTypeVerb other)
        {
            return other.Name == Name && other.Command == Command && other.Arguments == Arguments && other.Extended == Extended;
        }

        /// <inheritdoc/>
        public static bool operator ==(FileTypeVerb left, FileTypeVerb right)
        {
            return left.Equals(right);
        }

        /// <inheritdoc/>
        public static bool operator !=(FileTypeVerb left, FileTypeVerb right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj.GetType() == typeof(FileTypeVerb) && Equals((FileTypeVerb)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Name ?? "").GetHashCode();
                result = (result * 397) ^ (Command ?? "").GetHashCode();
                result = (result * 397) ^ (Arguments ?? "").GetHashCode();
                result = (result * 397) ^ Extended.GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
