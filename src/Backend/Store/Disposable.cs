/*
 * Copyright 2010-2017 Bastian Eicher
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

namespace ZeroInstall.Store
{
    /// <summary>
    /// Invokes a callback on <see cref="Dispose"/>.
    /// </summary>
    public sealed class Disposable : IDisposable
    {
        [NotNull]
        private readonly Action _callback;

        /// <summary>
        /// Creates a new disposable.
        /// </summary>
        /// <param name="callback">The callback to invoke on <see cref="Dispose"/>.</param>
        public Disposable([NotNull] Action callback) => _callback = callback ?? throw new ArgumentNullException(nameof(callback));

        /// <summary>
        /// Invokes the callback.
        /// </summary>
        public void Dispose() => _callback();
    }
}
