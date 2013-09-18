/*
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
using System.IO;
using System.Net;
using Common.Collections;
using Common.Tasks;
using ZeroInstall.Model;

namespace ZeroInstall.Publish.Cli
{
    public static partial class Program
    {
        private static readonly ITaskHandler _handler = new CliTaskHandler();

        /// <summary>
        /// Applies user-selected modifications to a feed.
        /// </summary>
        /// <param name="feed">The feed to modify.</param>
        /// <param name="options">The modifications to apply.</param>
        /// <exception cref="OperationCanceledException">Thrown if the user canceled the operation.</exception>
        /// <exception cref="WebException">Thrown if a file could not be downloaded from the internet.</exception>
        /// <exception cref="IOException">Thrown if there is a problem access a temporary file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read or write access to a temporary file is not permitted.</exception>
        private static void HandleModify(Feed feed, ParseResults options)
        {
            if (options.AddMissing)
                AddMissing(feed.Elements, options.StoreDownloads);
        }

        private static void AddMissing(IEnumerable<Element> elements, bool store = false)
        {
            new PerTypeDispatcher<Element>(true)
            {
                (Implementation implementation) => ImplementationUtils.AddMissing(implementation, _handler, store: store),
                (Group group) => AddMissing(group.Elements, store) // recursion
            }.Dispatch(elements);
        }
    }
}
