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

namespace ZeroInstall.Store.Implementations.Manifests
{
    /// <summary>
    /// Contains test methods for <see cref="ManifestFormat"/>.
    /// </summary>
    [TestFixture]
    public class ManifestFormatTest
    {
        [Test]
        public void TestFromPrefix()
        {
            ManifestFormat.FromPrefix("sha1new=abc").Should().BeSameAs(ManifestFormat.Sha1New);
            ManifestFormat.FromPrefix("sha256=abc").Should().BeSameAs(ManifestFormat.Sha256);
            ManifestFormat.FromPrefix("sha256new_abc").Should().BeSameAs(ManifestFormat.Sha256New);
        }
    }
}
