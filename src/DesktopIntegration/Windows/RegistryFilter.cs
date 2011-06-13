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
using Common.Collections;

namespace ZeroInstall.DesktopIntegration.Windows
{
    /// <summary>
    /// Applies a filter to registry access transparently replacing file paths on-the-fly.
    /// </summary>
    [Serializable]
    public class RegistryFilter
    {
        #region Variables
        /// <summary>A list of case-insensitive paths and the values they shall be mapped to when stored in the registry.</summary>
        private readonly ComparableTuple<string>[] _mappings;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new registry filter.
        /// </summary>
        /// <param name="mappings">A list of case-insensitive paths and the values they shall be mapped to when stored in the registry.</param>
        public RegistryFilter(ICollection<ComparableTuple<string>> mappings)
        {
            #region Sanity checks
            if (mappings == null) throw new ArgumentNullException("mappings");
            #endregion

            _mappings = new ComparableTuple<string>[mappings.Count];

            int i = 0;
            foreach (var mapping in mappings)
                _mappings[i++] = mapping;

            // Sort backwards to make sure the most specific matches are selected first
            Array.Sort(_mappings, (mapping1, mapping2) => mapping2.CompareTo(mapping1));
        }
        #endregion

        //--------------------//

        #region Filtering
        /// <summary>
        /// Filters a registry read request undoing <see cref="_mappings"/>.
        /// </summary>
        /// <param name="fromRegistry">The raw value read from the registry.</param>
        /// <returns>The value from the registry with any required substitutions applied.</returns>
        private string ReadFilter(string fromRegistry)
        {
            if (string.IsNullOrEmpty(fromRegistry)) return fromRegistry;

            for (int i = 0; i < _mappings.Length; i++)
            {
                if (fromRegistry.StartsWith(_mappings[i].Key))
                    return _mappings[i].Value + fromRegistry.Substring(_mappings[i].Key.Length);
            }

            return fromRegistry;
        }

        /// <summary>
        /// Filters a registry write request applying <see cref="_mappings"/>.
        /// </summary>
        /// <param name="toRegistry">The value to be written to the registry.</param>
        /// <returns>The value for the registry with any required substitutions applied.</returns>
        private string WriteFilter(string toRegistry)
        {
            if (string.IsNullOrEmpty(toRegistry)) return toRegistry;

            for (int i = 0; i < _mappings.Length; i++)
            {
                if (toRegistry.StartsWith(_mappings[i].Value))
                    return _mappings[i].Key + toRegistry.Substring(_mappings[i].Value.Length);
            }

            return toRegistry;
        }
        #endregion
    }
}
