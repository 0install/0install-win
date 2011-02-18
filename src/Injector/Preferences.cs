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

namespace ZeroInstall.Injector
{
    #region Enumerations
    /// <summary>
    /// Controls how liberally network access is attempted.
    /// </summary>
    /// <see cref="Preferences.NetworkLevel"/>
    public enum NetworkLevel
    {
        /// <summary>Do not access network at all.</summary>
        Offline,

        /// <summary>Only access network when there are no safe implementations available.</summary>
        Minimal,

        /// <summary>Always use network to get the newest available versions.</summary>
        Full
    }
    #endregion

    /// <summary>
    /// User-preferences controlling network behaviour, etc.
    /// </summary>
    [Serializable]
    public sealed class Preferences : IEquatable<Preferences>, ICloneable
    {
        #region Properties
        private NetworkLevel _networkLevel = NetworkLevel.Full;
        /// <summary>
        /// Controls how liberally network access is attempted.
        /// </summary>
        public NetworkLevel NetworkLevel
        {
            get { return _networkLevel; }
            set
            {
                #region Sanity checks
                if (!Enum.IsDefined(typeof(NetworkLevel), value)) throw new ArgumentOutOfRangeException("value");
                #endregion

                _networkLevel = value;
            }
        }

        /// <summary>
        /// The maximum age a cached <see cref="Model.Feed"/> may have until it is considered stale (needs to be updated).
        /// </summary>
        /// <remarks>This will be ignored if <see cref="NetworkLevel"/> is set to <see cref="Injector.NetworkLevel.Offline"/>.</remarks>
        public TimeSpan Freshness { get; set; }

        /// <summary>
        /// Always prefer the newest versions, even if they havent been marked as <see cref="Model.Stability.Stable"/> yet.
        /// </summary>
        public bool HelpWithTesting { get; set; }
        #endregion

        //--------------------//

        #region Storage
        /// <summary>
        /// Loads the prefences from the default location in the user profile.
        /// </summary>
        public static Preferences LoadDefault()
        {
            // ToDo
            return new Preferences();
        }
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Preferences"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Preferences"/>.</returns>
        public Preferences ClonePreferences()
        {
            return new Preferences {NetworkLevel = NetworkLevel, Freshness = Freshness, HelpWithTesting = HelpWithTesting};
        }

        /// <summary>
        /// Creates a deep copy of this <see cref="Preferences"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Preferences"/>.</returns>
        public object Clone()
        {
            return ClonePreferences();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(Preferences other)
        {
            if (other == null) return false;

            return other.NetworkLevel == NetworkLevel && other.Freshness == Freshness && other.HelpWithTesting == HelpWithTesting;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(Preferences) && Equals((Preferences)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = NetworkLevel.GetHashCode();
                result = (result * 397) ^ Freshness.GetHashCode();
                result = (result * 397) ^ HelpWithTesting.GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
