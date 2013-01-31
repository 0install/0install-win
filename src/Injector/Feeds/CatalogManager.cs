﻿/*
 * Copyright 2010-2013 Bastian Eicher
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Common;
using Common.Storage;
using ZeroInstall.Injector.Properties;
using ZeroInstall.Model;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Injector.Feeds
{
    /// <summary>
    /// Provides access to remote and local <see cref="Catalog"/>s. Handles downloading, signature verification and caching.
    /// </summary>
    public static class CatalogManager
    {
        #region Cached
        private const string CacheMutexName = "ZeroInstall.Injector.Feeds.CatalogManager.Cache";

        /// <summary>The file used to cache a merged view of all used catalogs.</summary>
        private static string CacheFilePath { get { return Path.Combine(Locations.GetCacheDirPath("0install.net", false), "catalog.xml"); } }

        /// <summary>
        /// Loads the last result of <see cref="GetOnline"/>.
        /// </summary>
        /// <returns>A valid <see cref="Catalog"/>. Returns an empty <see cref="Catalog"/> if the cache could not be loaded.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Uncached file system access")]
        public static Catalog GetCached()
        {
            try
            {
                using (new MutexLock(CacheMutexName))
                    return Catalog.Load(CacheFilePath);
            }
                #region Error handling
            catch (FileNotFoundException)
            {}
            catch (IOException ex)
            {
                Log.Warn(Resources.UnableToLoadCachedCatalog);
                Log.Warn(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Warn(Resources.UnableToLoadCachedCatalog);
                Log.Warn(ex.Message);
            }
            catch (InvalidDataException ex)
            {
                Log.Warn(Resources.UnableToLoadCachedCatalog);
                Log.Warn(ex.Message);
            }
            #endregion

            // Transparently handle errors
            return new Catalog();
        }
        #endregion

        #region Online
        /// <summary>
        /// The default <see cref="Catalog"/> source used if no other is specified.
        /// </summary>
        public const string DefaultSource = "http://0install.de/catalog/";

        /// <summary>
        /// Downloads and merges all <see cref="Catalog"/>s specified by the configuration files.
        /// </summary>
        /// <param name="policy">Provides additional class dependencies.</param>
        /// <returns>A merged <see cref="Catalog"/> view.</returns>
        /// <exception cref="WebException">Thrown if a file could not be downloaded from the internet.</exception>
        /// <exception cref="SignatureException">Thrown if the signature data of a remote catalog file could not be verified.</exception>
        /// <exception cref="InvalidDataException">Thrown if a problem occurs while deserializing the XML data.</exception>
        public static Catalog GetOnline(Policy policy)
        {
            var catalogs = GetCatalogSources().Select(delegate(string source)
            {
                // Download remote catalogs and open local catalogs
                Uri catalogUrl;
                return ModelUtils.TryParseUri(source, out catalogUrl)
                    ? DownloadCatalog(catalogUrl, policy)
                    : Catalog.Load(source);
            });
            var catalog = Catalog.Merge(catalogs);

            // Cache the result
            try
            {
                using (new MutexLock(CacheMutexName))
                    catalog.Save(CacheFilePath);
            }
                #region Error handling
            catch (IOException ex)
            {
                Log.Warn(Resources.UnableToCacheCatalog);
                Log.Warn(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Warn(Resources.UnableToCacheCatalog);
                Log.Warn(ex.Message);
            }
            #endregion

            return catalog;
        }

        /// <summary>
        /// Downloads and parses a remote catalog file.
        /// </summary>
        /// <param name="url">The URL to download the catalog file from.</param>
        /// <param name="policy">Provides additional class dependencies.</param>
        /// <returns>The parsed <see cref="Catalog"/>.</returns>
        /// <exception cref="WebException">Thrown if a file could not be downloaded from the internet.</exception>
        /// <exception cref="SignatureException">Thrown if the signature data of a remote catalog file could not be verified.</exception>
        /// <exception cref="InvalidDataException">Thrown if a problem occurs while deserializing the XML data.</exception>
        private static Catalog DownloadCatalog(Uri url, Policy policy)
        {
            var data = new WebClientTimeout().DownloadData(url);
            TrustUtils.CheckTrust(url, null, data, policy);
            return Catalog.Load(new MemoryStream(data));
        }
        #endregion

        #region Sources
        /// <summary>
        /// Returns a list of catalog sources as defined by configuration files.
        /// </summary>
        /// <exception cref="IOException">Thrown if there was a problem accessing a configuration file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if access to a configuration file was not permitted.</exception>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Reads data from a config file with no caching")]
        public static string[] GetCatalogSources()
        {
            var path = Locations.GetLoadConfigPaths("0install.net", true, "catalog-sources").FirstOrDefault();
            if (string.IsNullOrEmpty(path)) return new[] {DefaultSource};

            var result = new List<string>();
            foreach (string line in File.ReadAllLines(path, Encoding.UTF8).Where(line => !line.StartsWith("#") && !string.IsNullOrEmpty(line)))
            {
                Uri catalogUrl;
                result.Add(ModelUtils.TryParseUri(line, out catalogUrl)
                    ? catalogUrl.ToString()
                    : Environment.ExpandEnvironmentVariables(line));
            }
            return result.ToArray();
        }
        #endregion
    }
}
