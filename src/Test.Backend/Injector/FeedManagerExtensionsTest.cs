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
using NUnit.Framework;
using ZeroInstall.Model;
using ZeroInstall.Model.Preferences;

namespace ZeroInstall.Injector
{
    /// <summary>
    /// Contains test methods for <see cref="FeedManagerExtensions"/>.
    /// </summary>
    [TestFixture]
    public class FeedManagerExtensionsTest
    {
        /// <summary>
        /// Ensures <see cref="FeedManagerExtensions.GetFeedFresh"/> correctly handles fresh <see cref="Feed"/>s (no refresh).
        /// </summary>
        [Test]
        public void FreshHelperIsFresh()
        {
            // TODO
        }

        /// <summary>
        /// Ensures <see cref="FeedManagerExtensions.GetFeedFresh"/> correctly handles stale <see cref="Feed"/>s (refresh).
        /// </summary>
        [Test]
        public void FreshHelperIsStale()
        {
            // TODO
        }
    }
}
