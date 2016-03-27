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

using FluentAssertions;
using NUnit.Framework;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Services.Fetchers
{
    /// <summary>
    /// Ensures that <see cref="RetrievalMethodRanker"/> correctly ranks <see cref="RetrievalMethod"/>s.
    /// </summary>
    [TestFixture]
    public class RetrievalMethodRankerTest
    {
        /// <summary>
        /// Asserts that <paramref name="x"/> is ranked over <paramref name="y"/>.
        /// </summary>
        private static void AssertOver(RetrievalMethod x, RetrievalMethod y)
        {
            Assert.Less(RetrievalMethodRanker.Instance.Compare(x, y), 0);
            Assert.Greater(RetrievalMethodRanker.Instance.Compare(y, x), 0);
        }

        [Test]
        public void ArchiveOverRecipe()
        {
            AssertOver(new Archive(), new Recipe());
        }

        [Test]
        public void SingleFileOverRecipe()
        {
            AssertOver(new SingleFile(), new Recipe());
        }

        [Test]
        public void SmallOverLarge()
        {
            AssertOver(new Archive {Size = 10}, new SingleFile {Size = 20});
        }

        [Test]
        public void ArchiveAndSingleFileEqual()
        {
            RetrievalMethodRanker.Instance.Compare(new Archive(), new SingleFile()).Should().Be(0);
            RetrievalMethodRanker.Instance.Compare(new SingleFile(), new Archive()).Should().Be(0);
        }

        [Test]
        public void ShortRecipeOverLong()
        {
            AssertOver(new Recipe {Steps = {new Archive()}}, new Recipe {Steps = {new Archive(), new Archive()}});
        }
    }
}
