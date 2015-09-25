/*
 * Copyright 2010-2015 Bastian Eicher
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

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Contains test methods for <see cref="VersionRange"/>.
    /// </summary>
    [TestFixture]
    public class VersionRangeTest
    {
        /// <summary>
        /// Ensures <see cref="VersionRange.ToString"/> correctly serializes ranges.
        /// </summary>
        [Test]
        public void TestToString()
        {
            new VersionRange("2.6").ToString().Should().Be("2.6");
            new VersionRange("..!3").ToString().Should().Be("..!3");
            new VersionRange("!3").ToString().Should().Be("!3");
            new VersionRange("2.6..!3 | 3.2.2..").ToString().Should().Be("2.6..!3|3.2.2..");
            new VersionRange("..!3 | 3.2.2..").ToString().Should().Be("..!3|3.2.2..");
            new VersionRange("2.6..!3|3.2.2..!3.3").ToString().Should().NotBe("2.6..!3|3.2.2..");
            new VersionRange("2.6..|3.2.2..").ToString().Should().NotBe("2.6..!3|3.2.2..");
            new VersionRange("..!3|3.2.2..").ToString().Should().NotBe("2.6..!3|3.2.2..");
        }

        /// <summary>
        /// Ensures <see cref="VersionRange"/> objects are correctly compared.
        /// </summary>
        [Test]
        public void TestEquals()
        {
            new VersionRange("2.6").Should().Be(new VersionRange(new ImplementationVersion("2.6")));
            new VersionRange("..!3").Should().Be(new VersionRange(null, new ImplementationVersion("3")));
            new VersionRange("!3").Should().Be(new VersionRange("!3"));
            new VersionRange("2.6..!3 | 3.2.2..").Should().Be(new VersionRange("2.6..!3|3.2.2.."));
            new VersionRange("..!3 | 3.2.2..").Should().Be(new VersionRange("..!3|3.2.2.."));
            new VersionRange("2.6..!3|3.2.2..!3.3").Should().NotBe(new VersionRange("2.6..!3|3.2.2.."));
            new VersionRange("2.6..|3.2.2..").Should().NotBe(new VersionRange("2.6..!3|3.2.2.."));
            new VersionRange("..!3|3.2.2..").Should().NotBe(new VersionRange("2.6..!3|3.2.2.."));
        }

        /// <summary>
        /// Ensures <see cref="VersionRange.Match"/> works correctly.
        /// </summary>
        [Test]
        public void TestMatch()
        {
            new VersionRange("1.2").Match(new ImplementationVersion("1.2")).Should().BeTrue();
            new VersionRange("1.2").Match(new ImplementationVersion("1.3")).Should().BeFalse();
            new VersionRange("!1.2").Match(new ImplementationVersion("1.3")).Should().BeTrue();
            new VersionRange("!1.2").Match(new ImplementationVersion("1.2")).Should().BeFalse();
            new VersionRange("1.2..").Match(new ImplementationVersion("1.2")).Should().BeTrue();
            new VersionRange("1.2..").Match(new ImplementationVersion("1.1")).Should().BeFalse();
            new VersionRange("..!1.2").Match(new ImplementationVersion("1.1")).Should().BeTrue();
            new VersionRange("..!1.2").Match(new ImplementationVersion("1.2")).Should().BeFalse();
            new VersionRange("1.0..!1.2").Match(new ImplementationVersion("1.0")).Should().BeTrue();
            new VersionRange("1.0..!1.2").Match(new ImplementationVersion("1.1")).Should().BeTrue();
            new VersionRange("1.0..!1.2").Match(new ImplementationVersion("0.9")).Should().BeFalse();
            new VersionRange("1.0..!1.2").Match(new ImplementationVersion("1.2")).Should().BeFalse();
        }

        /// <summary>
        /// Ensures <see cref="VersionRange.Intersect"/> correctly handles <see cref="Constraint"/>s.
        /// </summary>
        [Test]
        public void TestIntersect()
        {
            var constraint = new Constraint {NotBefore = new ImplementationVersion("1"), Before = new ImplementationVersion("2")};
            new VersionRange().Intersect(constraint).Should().Be(new VersionRange("1..!2"));
            new VersionRange("..!3").Intersect(constraint).Should().Be(new VersionRange("1..!2"));
            new VersionRange("2..!3").Intersect(constraint).Should().Be(VersionRange.None);
            new VersionRange("1").Intersect(constraint).Should().Be(new VersionRange("1"));
            new VersionRange("3").Intersect(constraint).Should().Be(VersionRange.None);
            new VersionRange("2").Intersect(constraint).Should().Be(VersionRange.None);
            new VersionRange("0").Intersect(constraint).Should().Be(VersionRange.None);
            new VersionRange("!3").Intersect(constraint).Should().Be(new VersionRange("1..!2"));
            new VersionRange("!2").Intersect(constraint).Should().Be(new VersionRange("1..!2"));
            new VersionRange("!0").Intersect(constraint).Should().Be(new VersionRange("1..!2"));
            new VersionRange("!1").Intersect(constraint).Should().Be(VersionRange.None);

            constraint = new Constraint {Before = new ImplementationVersion("2")};
            new VersionRange().Intersect(constraint).Should().Be(new VersionRange("..!2"));
            new VersionRange("..!3").Intersect(constraint).Should().Be(new VersionRange("..!2"));
            new VersionRange("2..!3").Intersect(constraint).Should().Be(VersionRange.None);
            new VersionRange("1").Intersect(constraint).Should().Be(new VersionRange("1"));
            new VersionRange("3").Intersect(constraint).Should().Be(VersionRange.None);
            new VersionRange("2").Intersect(constraint).Should().Be(VersionRange.None);
            new VersionRange("!3").Intersect(constraint).Should().Be(new VersionRange("..!2"));
            new VersionRange("!2").Intersect(constraint).Should().Be(new VersionRange("..!2"));
            new VersionRange("!1").Intersect(constraint).Should().Be(VersionRange.None);

            constraint = new Constraint {NotBefore = new ImplementationVersion("1")};
            new VersionRange().Intersect(constraint).Should().Be(new VersionRange("1.."));
            new VersionRange("..!3").Intersect(constraint).Should().Be(new VersionRange("1..!3"));
            new VersionRange("2..!3").Intersect(constraint).Should().Be(new VersionRange("2..!3"));
            new VersionRange("1").Intersect(constraint).Should().Be(new VersionRange("1"));
            new VersionRange("0").Intersect(constraint).Should().Be(VersionRange.None);
            new VersionRange("!0").Intersect(constraint).Should().Be(new VersionRange("1.."));
            new VersionRange("!1").Intersect(constraint).Should().Be(VersionRange.None);
        }
    }
}
