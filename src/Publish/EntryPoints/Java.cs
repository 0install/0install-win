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

using System.ComponentModel;
using ZeroInstall.Model;
using ZeroInstall.Publish.EntryPoints.Design;

namespace ZeroInstall.Publish.EntryPoints
{
    /// <summary>
    /// A compiled Java application.
    /// </summary>
    public abstract class Java : Candidate
    {
        /// <summary>
        /// The minimum version of the Java Runtime Environment supported by the application.
        /// </summary>
        [Description("Minimum Java Runtime Environment version")]
        [DefaultValue("")]
        [TypeConverter(typeof(JavaVersionConverter))]
        public ImplementationVersion MinimumJavaVersion { get; set; }

        /// <summary>
        /// Does this application have external dependencies that need to be injected by Zero Install?
        /// </summary>
        [Description("External dependencies to be injected by Zero Install?")]
        [DefaultValue(false)]
        public bool HasDependencies { get; set; }

        #region Equality
        protected bool Equals(Java other)
        {
            return base.Equals(other) &&
                   Equals(MinimumJavaVersion, other.MinimumJavaVersion) &&
                   HasDependencies == other.HasDependencies;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Java)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (MinimumJavaVersion != null ? MinimumJavaVersion.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ HasDependencies.GetHashCode();
                return hashCode;
            }
        }
        #endregion
    }
}
