/*
 * Copyright 2010-2015 Bastian Eicher, Roland Leopold Walkling
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

using System.Collections.Generic;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Services.Fetchers
{
    /// <summary>
    /// Compares <see cref="RetrievalMethod"/>s and sortes them from most to least preferred by <see cref="IFetcher"/>s.
    /// </summary>
    public sealed class RetrievalMethodRanker : IComparer<RetrievalMethod>
    {
        #region Singleton
        /// <summary>
        /// Singleton pattern.
        /// </summary>
        public static readonly RetrievalMethodRanker Instance = new RetrievalMethodRanker();

        private RetrievalMethodRanker()
        {}
        #endregion

        /// <inheritdoc/>
        public int Compare(RetrievalMethod x, RetrievalMethod y)
        {
            if (x == y) return 0;

            if (x is DownloadRetrievalMethod && y is Recipe) return -1;
            if (x is Recipe && y is DownloadRetrievalMethod) return 1;

            var downloadX = x as DownloadRetrievalMethod;
            var downloadY = y as DownloadRetrievalMethod;
            if (downloadX != null && downloadY != null) return downloadX.Size.CompareTo(downloadY.Size);

            var recipeX = x as Recipe;
            var recipeY = y as Recipe;
            if (recipeX != null && recipeY != null) return recipeX.Steps.Count.CompareTo(recipeY.Steps.Count);

            return 0;
        }
    }
}
