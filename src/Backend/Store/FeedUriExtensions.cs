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
using JetBrains.Annotations;
using NanoByte.Common;

namespace ZeroInstall.Store
{
    /// <summary>
    /// Provides extensions methods for <see cref="FeedUri"/>-related types.
    /// </summary>
    public static class FeedUriExtensions
    {
        /// <summary>
        /// Wraps a <see cref="FeedUri"/> pointer in a <see cref="string"/> pointer.
        /// </summary>
        public static PropertyPointer<string> ToStringPointer([NotNull] this PropertyPointer<FeedUri> pointer)
        {
            #region Sanity checks
            if (pointer == null) throw new ArgumentNullException("pointer");
            #endregion

            return new PropertyPointer<string>(
                getValue: () => (pointer.Value == null) ? null : pointer.Value.ToStringRfc(),
                setValue: value => pointer.Value = (value == null) ? null : new FeedUri(value),
                defaultValue: (pointer.DefaultValue == null) ? null : pointer.DefaultValue.ToStringRfc());
        }
    }
}
