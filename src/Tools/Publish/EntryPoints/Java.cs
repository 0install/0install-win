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

using System.ComponentModel;
using JetBrains.Annotations;
using ZeroInstall.Publish.EntryPoints.Design;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Publish.EntryPoints
{
    /// <summary>
    /// A compiled Java application.
    /// </summary>
    public abstract class Java : Candidate
    {
        /// <summary>
        /// The minimum version of the Java Runtime Environment required by the application.
        /// </summary>
        [Category("Details (Java)"), DisplayName(@"Minimum Java version"), Description("The minimum version of the Java Runtime Environment required by the application.")]
        [DefaultValue("")]
        [TypeConverter(typeof(JavaVersionConverter))]
        [UsedImplicitly, CanBeNull]
        public ImplementationVersion MinimumRuntimeVersion { get; set; }

        /// <summary>
        /// Does this application have external dependencies that need to be injected by Zero Install? Only enable if you are sure!
        /// </summary>
        [Category("Details (Java)"), DisplayName(@"External dependencies"), Description("Does this application have external dependencies that need to be injected by Zero Install? Only enable if you are sure!")]
        [DefaultValue(false)]
        [UsedImplicitly]
        public bool ExternalDependencies { get; set; }

        /// <summary>
        /// Does this application have a graphical interface an no terminal output? Only enable if you are sure!
        /// </summary>
        [Category("Details (Java)"), DisplayName(@"GUI only"), Description("Does this application have a graphical interface an no terminal output? Only enable if you are sure!")]
        [UsedImplicitly]
        public bool GuiOnly { get => !NeedsTerminal; set => NeedsTerminal = !value; }

        #region Equality
        protected bool Equals(Java other)
        {
            if (other == null) return false;
            return base.Equals(other) &&
                   Equals(MinimumRuntimeVersion, other.MinimumRuntimeVersion) &&
                   ExternalDependencies == other.ExternalDependencies;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj == this) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Java)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (MinimumRuntimeVersion?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ ExternalDependencies.GetHashCode();
                return hashCode;
            }
        }
        #endregion
    }
}
