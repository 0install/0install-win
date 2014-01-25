/*
 * Copyright 2010-2014 Bastian Eicher, Roland Leopold Walkling
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

using System.IO;
using System.Reflection;

namespace ZeroInstall.Store.Trust
{
    /// <summary>
    /// Utility class that provides access to the test data contained in the assembly as resources.
    /// </summary>
    public static class TestData
    {
        private static readonly Assembly _testDataAssembly = Assembly.GetAssembly(typeof(TestData));

        public static Stream GetResource(string name)
        {
            return _testDataAssembly.GetManifestResourceStream(typeof(TestData), name);
        }
    }
}
