/*
 * Copyright 2010-2015 Bastian Eicher
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
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace ZeroInstall.Store
{
    /// <summary>
    /// Provides easy serialization to JSON files.
    /// </summary>
    public static class JsonStorage
    {
        /// <summary>
        /// Returns an object as an JSON string.
        /// </summary>
        /// <typeparam name="T">The type of object to be saved in an JSON string.</typeparam>
        /// <param name="data">The object to be stored.</param>
        /// <returns>A string containing the JSON code.</returns>
        [NotNull]
        public static string ToJsonString<T>([NotNull] this T data)
        {
            return JsonConvert.SerializeObject(data);
        }

        /// <summary>
        /// Loads an object from an JSON string.
        /// </summary>
        /// <typeparam name="T">The type of object the JSON string shall be converted into.</typeparam>
        /// <param name="data">The JSON string to be parsed.</param>
        /// <returns>The loaded object.</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "The type parameter is used to determine the type of returned object")]
        [NotNull]
        public static T FromJsonString<T>([NotNull] string data)
        {
            #region Sanity checks
            if (data == null) throw new ArgumentNullException("data");
            #endregion

            return JsonConvert.DeserializeObject<T>(data);
        }
    }
}
