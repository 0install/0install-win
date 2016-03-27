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

using System.IO;
using NanoByte.Common.Storage;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Contains extension methods for <see cref="Feed"/> to simpilify testing.
    /// </summary>
    public static class FeedExtensions
    {
        /// <summary>
        /// Writes this <see cref="Feed"/> to a byte array as an XML file.
        /// </summary>
        public static byte[] ToArray(this Feed feed)
        {
            using (var stream = new MemoryStream())
            {
                feed.SaveXml(stream);
                return stream.ToArray();
            }
        }
    }
}
