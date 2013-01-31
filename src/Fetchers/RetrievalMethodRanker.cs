/*
 * Copyright 2010-2013 Bastian Eicher, Roland Leopold Walkling
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
using ZeroInstall.Model;
using System.Diagnostics;

namespace ZeroInstall.Fetchers
{
    internal class RetrievalMethodRanker : IComparer<RetrievalMethod>
    {
        #region Declaration of Priorities
        private enum Category
        {
            Null,
            Format,
            Simplicity
        }

        private enum Valuation
        {
            Recipe = 1 * Category.Simplicity,
            Archive = 0 * Category.Simplicity,
            ZipFormat = 0 * Category.Format,
            GzFormat = 1 * Category.Format,
            UnknownFormat = 2 * Category.Format
        }
        #endregion

        private abstract class Ranking : IComparable<Ranking>
        {
            protected abstract int Value { get; }

            public int CompareTo(Ranking other)
            {
                #region Sanity checks
                if (other == null) throw new ArgumentNullException("other");
                #endregion

                return Value - other.Value;
            }
        }

        #region Archive Ranking
        private class ArchiveRanking : Ranking
        {
            private readonly Archive _subject;

            public ArchiveRanking(Archive subject)
            {
                _subject = subject;
            }

            protected override int Value
            {
                get
                {
                    int result = 0;
                    result += (int)Valuation.Archive;

                    if (_subject.MimeType == "application/zip")
                        result += (int)Valuation.ZipFormat;
                    else
                        result += (int)Valuation.UnknownFormat;

                    return result;
                }
            }
        }
        #endregion

        #region Recipe Ranking
        private class RecipeRanking : Ranking
        {
            public readonly Recipe Subject;

            public RecipeRanking(Recipe subject)
            {
                Subject = subject;
            }

            protected override int Value
            {
                get
                {
                    int result = 0;
                    result += (int)Valuation.Archive;

                    return result;
                }
            }
        }
        #endregion

        #region Dispatching Creation of Ranking objects
        private static Ranking Rank(RetrievalMethod subject)
        {
            #region Sanity checks
            if (subject == null) throw new ArgumentNullException("subject");
            #endregion

            Ranking result = null;

            var archive = subject as Archive;
            if (archive != null) result = new ArchiveRanking(archive);
            else
            {
                var recipe = subject as Recipe;
                if (recipe != null) result = new RecipeRanking(recipe);
                else Debug.Fail("subject (RetrievalMethod) has unknown type");
            }
            return result;
        }
        #endregion

        public int Compare(RetrievalMethod x, RetrievalMethod y)
        {
            var rankingX = Rank(x);
            var rankingY = Rank(y);
            return rankingX.CompareTo(rankingY);
        }
    }
}
