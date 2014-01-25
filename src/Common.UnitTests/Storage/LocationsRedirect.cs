/*
 * Copyright 2010-2014 Bastian Eicher
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
using System.IO;

namespace Common.Storage
{
    /// <summary>
    /// Disposable class to create a temporary directory where all <see cref="Locations"/> queries are temporarily redirected to.
    /// </summary>
    public class LocationsRedirect : TemporaryDirectory
    {
        private readonly string _previousPortableBase;
        private readonly bool _previousIsPortable;

        /// <summary>
        /// Creates a uniquely named, empty temporary directory on disk and starts redirecting all <see cref="Locations"/> queries there.
        /// </summary>
        /// <param name="prefix">A short string the directory name should start with.</param>
        /// <exception cref="IOException">Thrown if a problem occurred while creating a directory in <see cref="System.IO.Path.GetTempPath"/>.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a directory in <see cref="System.IO.Path.GetTempPath"/> is not permitted.</exception>
        public LocationsRedirect(string prefix) : base(prefix)
        {
            _previousPortableBase = Locations.PortableBase;
            _previousIsPortable = Locations.IsPortable;

            Locations.PortableBase = Path;
            Locations.IsPortable = true;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    Locations.IsPortable = _previousIsPortable;
                    Locations.PortableBase = _previousPortableBase;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}
