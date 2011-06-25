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
using System.Collections.Generic;
using System.Xml.Serialization;
using Common.Tasks;
using Common.Utils;
using Capabilities = ZeroInstall.Model.Capabilities;

namespace ZeroInstall.DesktopIntegration.AccessPoints
{
    /// <summary>
    /// Makes an application the default AutoPlay handler for a specific event.
    /// </summary>
    /// <seealso cref="ZeroInstall.Model.Capabilities.AutoPlay"/>
    [XmlType("auto-play", Namespace = AppList.XmlNamespace)]
    public class AutoPlay : DefaultAccessPoint, IEquatable<AutoPlay>
    {
        #region Conflict ID
        /// <inheritdoc/>
        public override IEnumerable<string> GetConflictIDs(AppEntry appEntry)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            #endregion

            var capability = appEntry.GetCapability<Capabilities.AutoPlay>(Capability);
            return capability.Events.Map(@event => "autoplay-event:" + @event.Name);
        }
        #endregion

        #region Apply
        /// <inheritdoc/>
        public override void Apply(AppEntry appEntry, InterfaceFeed target, bool systemWide, ITaskHandler handler)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            var capability = appEntry.GetCapability<Capabilities.AutoPlay>(Capability);
            if (capability == null) return;

            if (WindowsUtils.IsWindows)
                Windows.AutoPlay.Register(target, capability, true, systemWide, handler);
        }

        /// <inheritdoc/>
        public override void Unapply(AppEntry appEntry, bool systemWide)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            #endregion

            var capability = appEntry.GetCapability<Capabilities.AutoPlay>(Capability);
            if (capability == null) return;

            if (WindowsUtils.IsWindows)
                Windows.AutoPlay.Unregister(capability, systemWide);
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the access point in the form "AutoPlay". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("AutoPlay");
        }
        #endregion

        #region Clone
        /// <inheritdoc/>
        public override AccessPoint CloneAccessPoint()
        {
            return new AutoPlay {Capability = Capability};
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(AutoPlay other)
        {
            if (other == null) return false;

            return base.Equals(other);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(AutoPlay) && Equals((AutoPlay)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
