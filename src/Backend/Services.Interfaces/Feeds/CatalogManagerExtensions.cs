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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using JetBrains.Annotations;
using NanoByte.Common;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Services.Feeds
{
    /// <summary>
    /// Provides extension methods for <see cref="ICatalogManager"/>.
    /// </summary>
    public static class CatalogManagerExtensions
    {
        /// <summary>
        /// Loads the last result of <see cref="ICatalogManager.GetOnline"/>.
        /// </summary>
        /// <returns>A <see cref="Catalog"/>; an empty <see cref="Catalog"/> if there was a problem.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "File system access")]
        [NotNull]
        public static Catalog GetCachedSafe([NotNull] this ICatalogManager manager)
        {
            #region Sanity checks
            if (manager == null) throw new ArgumentNullException("manager");
            #endregion

            try
            {
                return manager.GetCached() ?? new Catalog();
            }
                #region Error handling
            catch (IOException ex)
            {
                Log.Warn(ex.Message);
                return new Catalog();
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Warn(ex.Message);
                return new Catalog();
            }
            catch (InvalidDataException ex)
            {
                Log.Warn(ex.Message);
                return new Catalog();
            }
            #endregion
        }

        /// <summary>
        /// Downloads and merges all <see cref="Catalog"/>s specified by the configuration files.
        /// </summary>
        /// <returns>A <see cref="Catalog"/>; an empty <see cref="Catalog"/> if there was a problem.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Performs network IO and has side-effects")]
        [NotNull]
        public static Catalog GetOnlineSafe([NotNull] this ICatalogManager manager)
        {
            #region Sanity checks
            if (manager == null) throw new ArgumentNullException("manager");
            #endregion

            try
            {
                return manager.GetOnline();
            }
                #region Error handling
            catch (IOException ex)
            {
                Log.Warn(ex);
                return new Catalog();
            }
            catch (WebException ex)
            {
                Log.Warn(ex);
                return new Catalog();
            }
            catch (InvalidDataException ex)
            {
                Log.Warn(ex);
                return new Catalog();
            }
            catch (SignatureException ex)
            {
                Log.Warn(ex);
                return new Catalog();
            }
            catch (UriFormatException ex)
            {
                Log.Warn(ex);
                return new Catalog();
            }
            #endregion
        }
    }
}
