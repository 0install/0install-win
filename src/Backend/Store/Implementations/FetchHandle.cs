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
using NanoByte.Common;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Store.Implementations
{
    /// <summary>
    /// Provides a way to share an <see cref="Implementation"/> fetch callback as per-thread ambient state.
    /// </summary>
    /// <remarks>This is useful for making the high-level Fetcher service available to low-level systems such as a Recipe step.</remarks>
    public static class FetchHandle
    {
        [ThreadStatic]
        private static Func<Implementation, string> _callback;

        /// <summary>
        /// Registers an <see cref="Implementation"/> fetch callback for the current thread.
        /// </summary>
        /// <param name="callback">A callback that downloads an implementation to a local cache if missing and returns its path.</param>
        /// <returns>A handle that can be used to remove the registration.</returns>
        [NotNull]
        public static IDisposable Register([NotNull] Func<Implementation, string> callback)
        {
            #region Sanity checks
            if (callback == null) throw new ArgumentNullException(nameof(callback));
            #endregion

            var previousValue = _callback;
            _callback = callback;
            return new Disposable(() => _callback = previousValue);
        }

        /// <summary>
        /// Downloads an <see cref="Implementation"/> to a local cache if missing and returns its path. <see cref="Register"/> must be called first on the same thread.
        /// </summary>
        /// <param name="implementation">The implementation to be downloaded.</param>
        /// <returns>A fully qualified path to the directory containing the implementation.</returns>
        /// <exception cref="InvalidOperationException"><see cref="Register"/> was not called first.</exception>
        [NotNull]
        public static string Use([NotNull] Implementation implementation)
        {
            #region Sanity checks
            if (implementation == null) throw new ArgumentNullException(nameof(implementation));
            #endregion

            if (_callback == null) throw new InvalidOperationException("Implementation provider must be registered first on the same thread.");

            return _callback(implementation);
        }
    }
}
