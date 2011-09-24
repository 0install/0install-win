/*
 * Copyright 2010-2011 Bastian Eicher
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
using Common.Storage;
using NDesk.Options;
using ZeroInstall.Model;
using ZeroInstall.Publish.Cli.Properties;

namespace ZeroInstall.Publish.Cli
{
    public static partial class Program
    {
        /// <summary>
        /// Creates a <see cref="Catalog"/> from the <see cref="Feed"/>s specified in the command-line arguments.
        /// </summary>
        /// <param name="results">The parser results to be executed.</param>
        /// <exception cref="OptionException">Thrown if the specified feed file paths were invalid.</exception>
        /// <exception cref="InvalidDataException">Thrown if a feed file is damaged.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the a files could not be found.</exception>
        /// <exception cref="IOException">Thrown if a file could not be read or written.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read or write access to a feed file or the catalog file is not permitted.</exception>
        public static void CreateCatalog(ParseResults results)
        {
            var catalog = new Catalog();

            foreach (var feedFile in results.Feeds)
            {
                var feed = Feed.Load(feedFile.FullName);
                feed.Strip();
                catalog.Feeds.Add(feed);
            }

            if (catalog.Feeds.IsEmpty) throw new FileNotFoundException(Resources.NoFeedFilesFound);

            catalog.Save(results.CatalogFile);
            XmlStorage.AddStylesheet(results.CatalogFile, "catalog.xsl");
        }
    }
}
