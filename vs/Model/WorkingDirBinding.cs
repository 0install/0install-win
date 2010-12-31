/*
 * Copyright 2010 Bastian Eicher
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

namespace ZeroInstall.Model
{
    /// <summary>
    /// Make a chosen <see cref="Model.Implementation"/> available by overlaying it onto another part of the file-system.
    /// </summary>
    /// <remarks>
    /// <para>This is to support legacy programs which can't properly locate their installation directory.</para>
    /// <para>Only the once instance of this binding type in a selection of <see cref="Model.Implementation"/>s (the last one to be processed) is effective. All others are ignored.</para>
    /// </remarks>
    [Serializable]
    [XmlType("working-dir", Namespace = Feed.XmlNamespace)]
    public sealed class WorkingDirBinding : Binding, IEquatable<WorkingDirBinding>
    {
        #region Properties
        /// <summary>
        /// The relative path of the directory in the implementation to publish. The default is to publish everything.
        /// </summary>
        [Description("The name of the environment variable.")]
        [XmlAttribute("src"), DefaultValue("")]
        public string Source { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new default working binding that switches to the implementation's root.
        /// </summary>
        public WorkingDirBinding()
        {
            Source = ".";
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the binding in the form "WorkingDirBinding: Source". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("WorkingDirBinding: {0}", Source);
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="WorkingDirBinding"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="WorkingDirBinding"/>.</returns>
        public override Binding CloneBinding()
        {
            return new WorkingDirBinding { Source = Source };
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(WorkingDirBinding other)
        {
            if (ReferenceEquals(null, other)) return false;

            return other.Source == Source;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(WorkingDirBinding) && Equals((WorkingDirBinding)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return (Source ?? "").GetHashCode();
        }
        #endregion
    }
}
