/*
 * Copyright 2010-2012 Bastian Eicher
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
using System.Text;
using Common;
using Common.Collections;
using Common.Storage;
using ZeroInstall.Model;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Injector.Feeds
{
    public static class CatalogProvider
    {
        public const string DefaultSource = "http://0install.de/catalog/";

        private static string CacheFilePath { get { return Path.Combine(Locations.GetCacheDirPath("0install.net"), "catalog.xml"); } }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Uncached file system access")]
        public static Catalog GetCached()
        {
            try
            {
                return Catalog.Load(CacheFilePath);
            }
                #region Error handling
            catch (FileNotFoundException)
            {}
            catch (IOException ex)
            {
                Log.Warn("Unable to load cached application catalog from disk:\n" + ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Warn("Unable to load cached application catalog from disk:\n" + ex.Message);
            }
            catch (InvalidDataException ex)
            {
                Log.Warn("Unable to parse cached application catalog:\n" + ex.Message);
            }
            #endregion

            // Transparently handle errors
            return new Catalog();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="policy"></param>
        /// <returns></returns>
        /// <exception cref="WebException"></exception>
        /// <exception cref="SignatureException"></exception>
        /// <exception cref="InvalidDataException">Thrown if a problem occurs while deserializing the XML data.</exception>
        public static Catalog GetOnline(Policy policy)
        {
            // Load and megre all catalogs
            var catalog = Catalog.Merge(Array.ConvertAll(GetCatalogSources(), delegate(string source)
            {
                Uri catalogUrl;
                return ModelUtils.TryParseUri(source, out catalogUrl) ? DownloadCatalog(catalogUrl, policy) : Catalog.Load(source);
            }));

            // Cache the result
            try
            {
                catalog.Save(CacheFilePath);
            }
                #region Error handling
            catch (IOException ex)
            {
                Log.Warn("Unable to cache downloaded application catalog:\n" + ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Warn("Unable to cache downloaded application catalog:\n" + ex.Message);
            }
            #endregion

            return catalog;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="policy"></param>
        /// <returns></returns>
        /// <exception cref="WebException"></exception>
        /// <exception cref="SignatureException"></exception>
        /// <exception cref="InvalidDataException">Thrown if a problem occurs while deserializing the XML data.</exception>
        private static Catalog DownloadCatalog(Uri url, Policy policy)
        {
            var data = new WebClientTimeout().DownloadData(url);
            TrustUtils.CheckTrust(url, null, data, policy);
            return Catalog.Load(new MemoryStream(data));
        }

        /// <summary>
        /// Returns a list of catalog sources as defined by configuration files.
        /// </summary>
        /// <exception cref="IOException">Thrown if there was a problem accessing a configuration file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if access to a configuration file was not permitted.</exception>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Reads data from a config file with no caching")]
        public static string[] GetCatalogSources()
        {
            var path = EnumerableUtils.First(Locations.GetLoadConfigPaths("0install.net", true, "catalog-sources"));
            if (string.IsNullOrEmpty(path)) return new[] {DefaultSource};

            var result = new C5.LinkedList<string>();
            foreach (string line in File.ReadAllLines(path, Encoding.UTF8))
            {
                if (line.StartsWith("#") || string.IsNullOrEmpty(line)) continue;
                Uri catalogUrl;
                result.Add(ModelUtils.TryParseUri(line, out catalogUrl)
                    ? catalogUrl.ToString()
                    : Environment.ExpandEnvironmentVariables(line));
            }
            return result.ToArray();
        }
    }
}
