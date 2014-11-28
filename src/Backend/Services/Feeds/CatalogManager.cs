/*
 * Copyright 2010-2014 Bastian Eicher
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
using System.Linq;
using System.Net;
using System.Text;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Net;
using NanoByte.Common.Storage;
using ZeroInstall.Services.Properties;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Services.Feeds
{
    /// <summary>
    /// Provides access to remote and local <see cref="Catalog"/>s. Handles downloading, signature verification and caching.
    /// </summary>
    public class CatalogManager
    {
        #region Constants
        private const string CacheMutexName = "ZeroInstall.Feeds.CatalogManager.Cache";
        #endregion

        #region Dependencies
        private readonly ITrustManager _trustManager;

        /// <summary>
        /// Creates a new catalog manager.
        /// </summary>
        /// <param name="trustManager">Methods for verifying signatures and user trust.</param>
        public CatalogManager(ITrustManager trustManager)
        {
            #region Sanity checks
            if (trustManager == null) throw new ArgumentNullException("trustManager");
            #endregion

            _trustManager = trustManager;
        }
        #endregion

        private readonly string _cacheFilePath = Path.Combine(Locations.GetCacheDirPath("0install.net", machineWide: false), "catalog.xml");

        #region Cached
        /// <summary>
        /// Loads the last result of <see cref="GetOnline"/>.
        /// </summary>
        /// <returns>A valid <see cref="Catalog"/>. Returns an empty <see cref="Catalog"/> if the cache could not be loaded.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Uncached file system access")]
        public Catalog GetCached()
        {
            try
            {
                using (new MutexLock(CacheMutexName))
                    return XmlStorage.LoadXml<Catalog>(_cacheFilePath);
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
        public static readonly FeedUri DefaultSource = new FeedUri("http://0install.de/catalog/");

        /// <summary>
        /// Downloads and merges all <see cref="Catalog"/>s specified by the configuration files.
        /// </summary>
        /// <returns>A merged <see cref="Catalog"/> view.</returns>
        /// <exception cref="IOException">A problem occured while reading a local catalog file.</exception>
        /// <exception cref="WebException">A problem occured while fetching a remote catalog file.</exception>
        /// <exception cref="InvalidDataException">A problem occurs while deserializing the XML data.</exception>
        /// <exception cref="SignatureException">The signature data of a remote catalog file could not be verified.</exception>
        /// <exception cref="UriFormatException">An invalid catalog source is specified in the configuration file.</exception>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Performs network IO and has side-effects")]
        public Catalog GetOnline()
        {
            var catalogs = GetCatalogSources().Select(source => source.IsFile
                ? XmlStorage.LoadXml<Catalog>(source.LocalPath)
                : DownloadCatalog(source));
            var catalog = Catalog.Merge(catalogs);

            // Cache the result
            try
            {
                using (new MutexLock(CacheMutexName))
                    catalog.SaveXml(_cacheFilePath);
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
        /// <returns>The parsed <see cref="Catalog"/>.</returns>
        /// <exception cref="WebException">A file could not be downloaded from the internet.</exception>
        /// <exception cref="SignatureException">The signature data of a remote catalog file could not be verified.</exception>
        /// <exception cref="InvalidDataException">A problem occurs while deserializing the XML data.</exception>
        private Catalog DownloadCatalog(FeedUri url)
        {
            byte[] data;
            using (var webClient = new WebClientTimeout())
                data = webClient.DownloadData(url);
            _trustManager.CheckTrust(data, url);
            return XmlStorage.LoadXml<Catalog>(new MemoryStream(data));
        }
        #endregion

        #region Sources
        /// <summary>
        /// Returns a list of catalog sources as defined by configuration files.
        /// </summary>
        /// <exception cref="IOException">There was a problem accessing a configuration file.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to a configuration file was not permitted.</exception>
        /// <exception cref="UriFormatException">An invalid catalog source is specified in the configuration file.</exception>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Reads data from a config file with no caching")]
        public static FeedUri[] GetCatalogSources()
        {
            var path = Locations.GetLoadConfigPaths("0install.net", true, "catalog-sources").FirstOrDefault();
            if (string.IsNullOrEmpty(path)) return new[] {DefaultSource};

            return File.ReadAllLines(path, Encoding.UTF8)
                .Except(string.IsNullOrEmpty)
                .Except(line => line.StartsWith("#"))
                .Select(line => new FeedUri(line))
                .ToArray();
        }
        #endregion
    }
}
