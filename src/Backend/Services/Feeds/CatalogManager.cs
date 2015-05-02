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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Net;
using NanoByte.Common.Storage;
using ZeroInstall.Services.Properties;
using ZeroInstall.Store;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Services.Feeds
{
    /// <summary>
    /// Provides access to remote and local <see cref="Catalog"/>s. Handles downloading, signature verification and caching.
    /// </summary>
    public class CatalogManager : ICatalogManager
    {
        #region Constants
        private const string CacheMutexName = "ZeroInstall.Feeds.CatalogManager.Cache";

        /// <summary>
        /// The default <see cref="Catalog"/> source used if no other is specified.
        /// </summary>
        public static readonly FeedUri DefaultSource = new FeedUri("http://0install.de/catalog/");
        #endregion

        #region Dependencies
        private readonly ITrustManager _trustManager;

        /// <summary>
        /// Creates a new catalog manager.
        /// </summary>
        /// <param name="trustManager">Methods for verifying signatures and user trust.</param>
        public CatalogManager([NotNull] ITrustManager trustManager)
        {
            #region Sanity checks
            if (trustManager == null) throw new ArgumentNullException("trustManager");
            #endregion

            _trustManager = trustManager;
        }
        #endregion

        private readonly string _cacheFilePath = Path.Combine(Locations.GetCacheDirPath("0install.net", machineWide: false), "catalog.xml");

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "File system access")]
        public Catalog GetCached()
        {
            try
            {
                using (new MutexLock(CacheMutexName))
                    return XmlStorage.LoadXml<Catalog>(_cacheFilePath);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Performs network IO and has side-effects")]
        public Catalog GetOnline()
        {
            // Download + merge
            Catalog mergedCatalog = new Catalog();
            foreach (var source in GetSources())
            {
                foreach (var feed in DownloadCatalog(source).Feeds)
                {
                    feed.CatalogUri = source;
                    mergedCatalog.Feeds.Add(feed);
                }
            }
            mergedCatalog.Normalize();

            // Cache
            try
            {
                Log.Debug("Caching merged Catalog in: " + _cacheFilePath);
                using (new MutexLock(CacheMutexName))
                    mergedCatalog.SaveXml(_cacheFilePath);
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

            return mergedCatalog;
        }

        /// <summary>
        /// Downloads and parses a remote catalog file.
        /// </summary>
        /// <param name="source">The URL to download the catalog file from.</param>
        /// <returns>The parsed <see cref="Catalog"/>.</returns>
        /// <exception cref="WebException">A file could not be downloaded from the internet.</exception>
        /// <exception cref="SignatureException">The signature data of a remote catalog file could not be verified.</exception>
        /// <exception cref="InvalidDataException">A problem occurs while deserializing the XML data.</exception>
        [NotNull]
        private Catalog DownloadCatalog([NotNull] FeedUri source)
        {
            if (source.IsFile) return XmlStorage.LoadXml<Catalog>(source.LocalPath);

            Log.Info("Downloading catalog: " + source.ToStringRfc());
            byte[] data;
            using (var webClient = new WebClientTimeout())
                data = webClient.DownloadData(source);
            _trustManager.CheckTrust(data, source);
            return XmlStorage.LoadXml<Catalog>(new MemoryStream(data));
        }

        /// <inheritdoc/>
        public void AddSource(FeedUri uri)
        {
            var sources = GetSources().ToList();
            sources.AddIfNew(uri);
            SetSources(sources);
        }

        /// <inheritdoc/>
        public void RemoveSource(FeedUri uri)
        {
            var sources = GetSources().ToList();
            sources.Remove(uri);
            SetSources(sources);
        }

        /// <summary>
        /// Returns a list of catalog sources as defined by configuration files.
        /// </summary>
        /// <remarks>Only the top-most configuration file is processed. I.e., a user config overrides a system config.</remarks>
        /// <exception cref="IOException">There was a problem accessing a configuration file.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to a configuration file was not permitted.</exception>
        /// <exception cref="UriFormatException">An invalid catalog source is specified in the configuration file.</exception>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Reads data from a config file with no caching")]
        [NotNull, ItemNotNull]
        public static FeedUri[] GetSources()
        {
            var path = Locations.GetLoadConfigPaths("0install.net", true, "catalog-sources").FirstOrDefault();
            if (string.IsNullOrEmpty(path)) return new[] {DefaultSource};

            return File.ReadAllLines(path, Encoding.UTF8)
                .Except(string.IsNullOrEmpty)
                .Except(line => line.StartsWith("#"))
                .Select(line => new FeedUri(line))
                .ToArray();
        }

        /// <summary>
        /// Sets the list of catalog sources in a configuration file.
        /// </summary>
        /// <param name="uris">The list of catalog sources to use from now on.</param>
        /// <exception cref="IOException">There was a problem writing a configuration file.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to a configuration file was not permitted.</exception>
        public static void SetSources([NotNull, ItemNotNull, InstantHandle] IEnumerable<FeedUri> uris)
        {
            #region Sanity checks
            if (uris == null) throw new ArgumentNullException("uris");
            #endregion

            using (var atomic = new AtomicWrite(Locations.GetSaveConfigPath("0install.net", true, "catalog-sources")))
            {
                using (var configFile = new StreamWriter(atomic.WritePath, append: false, encoding: FeedUtils.Encoding) {NewLine = "\n"})
                {
                    foreach (var uri in uris)
                        configFile.WriteLine(uri.ToStringRfc());
                }
                atomic.Commit();
            }
        }
    }
}
