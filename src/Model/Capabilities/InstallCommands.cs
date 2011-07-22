﻿/*
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
    /// Lists the commands the application registeres for use by Windows' "Set Program Access and Defaults". Will be transparently replaced with Zero Install commands at runtime.
    /// </summary>
    /// <remarks>These strings are used for registry filtering. They are never actually executed.</remarks>
    [XmlType("install-commands", Namespace = Capability.XmlNamespace)]
    public struct InstallCommands : IEquatable<InstallCommands>
    {
        #region Properties
        /// <summary>
        /// The path (relative to the installation directory) to the executable used to set an application as the default program without any arguments.
        /// </summary>
        [Description("The path (relative to the installation directory) to the executable used to set an application as the default program without any arguments.")]
        [XmlAttribute("reinstall"), DefaultValue("")]
        public string Reinstall { get; set; }

        /// <summary>
        /// Additional arguments for the executable specified in <see cref="Reinstall"/>.
        /// </summary>
        [Description("Additional arguments for the executable specified in Reinstall.")]
        [XmlAttribute("reinstall-args"), DefaultValue("")]
        public string ReinstallArgs { get; set; }

        /// <summary>
        /// The path (relative to the installation directory) to the executable used to create icons/shortcuts to the application without any arguments.
        /// </summary>
        [Description("The path (relative to the installation directory) to the executable used to create icons/shortcuts to the application without any arguments.")]
        [XmlAttribute("show-icons"), DefaultValue("")]
        public string ShowIcons { get; set; }

        /// <summary>
        /// Additional arguments for the executable specified in <see cref="ShowIcons"/>.
        /// </summary>
        [Description("Additional arguments for the executable specified in ShowIcons.")]
        [XmlAttribute("show-icons-args"), DefaultValue("")]
        public string ShowIconsArgs { get; set; }

        /// <summary>
        /// The path (relative to the installation directory) to the executable used to remove icons/shortcuts to the application without any arguments.
        /// </summary>
        [Description("The path (relative to the installation directory) to the executable used to remove icons/shortcuts to the application without any arguments.")]
        [XmlAttribute("hide-icons"), DefaultValue("")]
        public string HideIcons { get; set; }

        /// <summary>
        /// Additional arguments for the executable specified in <see cref="HideIcons"/>.
        /// </summary>
        [Description("Additional arguments for the executable specified in HideIcons.")]
        [XmlAttribute("hide-icons-args"), DefaultValue("")]
        public string HideIconsArgs { get; set; }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the install info in the form "InstallCommands: Reinstall ReinstallArgs, ShowIcons ShowIconsArgs, HideIcons HideIconsArgs". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("InstallCommands: {0} {1}, {2} {3}, {4} {5}", Reinstall, ReinstallArgs, ShowIcons, ShowIconsArgs, HideIcons, HideIconsArgs);
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(InstallCommands other)
        {
            return other.Reinstall == Reinstall && other.ReinstallArgs == ReinstallArgs &&
                other.ShowIcons == ShowIcons && other.ShowIconsArgs == ShowIconsArgs &&
                other.HideIcons == HideIcons && other.HideIconsArgs == HideIconsArgs;
        }

        /// <inheritdoc/>
        public static bool operator ==(InstallCommands left, InstallCommands right)
        {
            return left.Equals(right);
        }

        /// <inheritdoc/>
        public static bool operator !=(InstallCommands left, InstallCommands right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj.GetType() == typeof(InstallCommands) && Equals((InstallCommands)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Reinstall ?? "").GetHashCode();
                result = (result * 397) ^ (ReinstallArgs ?? "").GetHashCode();
                result = (result * 397) ^ (ShowIcons ?? "").GetHashCode();
                result = (result * 397) ^ (ShowIconsArgs ?? "").GetHashCode();
                result = (result * 397) ^ (HideIcons ?? "").GetHashCode();
                result = (result * 397) ^ (HideIconsArgs ?? "").GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
