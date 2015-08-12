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

using System;
using System.IO;
using System.Net;
using JetBrains.Annotations;
using NanoByte.Common;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Services.Solvers
{
    /// <summary>
    /// Provides extension methods for <see cref="ISolver"/>.
    /// </summary>
    public static class SolverExtensions
    {
        /// <summary>
        /// Provides a set of <see cref="Selections"/> that satisfy a set of <see cref="Requirements"/>. Catches most exceptions and <see cref="Log"/>s them.
        /// </summary>
        /// <param name="solver">The <see cref="ISolver"/> implementation.</param>
        /// <param name="requirements">A set of requirements/restrictions imposed by the user on the implementation selection process.</param>
        /// <returns>The <see cref="ImplementationSelection"/>s chosen for the feed; <c>null</c> if there was a problem.</returns>
        /// <remarks>Feed files may be downloaded, signature validation is performed, implementations are not downloaded.</remarks>
        /// <exception cref="OperationCanceledException">The user canceled the task.</exception>
        /// <exception cref="ArgumentException"><paramref name="requirements"/> is incomplete.</exception>
        [CanBeNull]
        public static Selections TrySolve(this ISolver solver, [NotNull] Requirements requirements)
        {
            #region Sanity checks
            if (solver == null) throw new ArgumentNullException(nameof(solver));
            #endregion

            try
            {
                return solver.Solve(requirements);
            }
                #region Error handling
            catch (IOException ex)
            {
                Log.Warn(ex);
                return null;
            }
            catch (WebException ex)
            {
                Log.Warn(ex);
                return null;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Warn(ex);
                return null;
            }
            catch (SignatureException ex)
            {
                Log.Warn(ex);
                return null;
            }
            catch (SolverException ex)
            {
                Log.Warn(ex);
                return null;
            }
            #endregion
        }
    }
}
