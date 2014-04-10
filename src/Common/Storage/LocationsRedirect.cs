/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.IO;

namespace NanoByte.Common.Storage
{
    /// <summary>
    /// Disposable class to create a temporary directory where all <see cref="Locations"/> queries are temporarily redirected to.
    /// Useful for testing. Do not use when multi-threading is involved!
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
        public LocationsRedirect(string prefix)
            : base(prefix)
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
