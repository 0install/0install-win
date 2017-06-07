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

using System;
using FluentAssertions;
using Xunit;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Contains test methods for <see cref="ImplementationVersion"/>.
    /// </summary>
    public class ImplementationVersionTest
    {
        /// <summary>
        /// Ensures the <see cref="ImplementationVersion.TryCreate"/> correctly handles valid strings and rejects invalid ones.
        /// </summary>
        [Fact]
        public void TestTryCreate()
        {
            var validVersions = new[] {"0.1", "1", "1.0", "1.1", "1.1-", "1.2-pre", "1.2-pre1", "1.2-rc1", "1.2", "1.2-0", "1.2--0", "1.2-post", "1.2-post1-pre", "1.2-post1", "1.2.1-pre", "1.2.1.4", "1.2.3", "1.2.10", "3"};
            foreach (string version in validVersions)
            {
                ImplementationVersion result;
                ImplementationVersion.TryCreate(version, out result).Should().BeTrue(because: version);
                result.ToString().Should().Be(version);
            }

            var invalidVersions = new[] {"", "a", "pre-1", "1.0-1post"};
            foreach (string version in invalidVersions)
            {
                ImplementationVersion result;
                ImplementationVersion.TryCreate(version, out result).Should().BeFalse(because: version);
            }
        }

        /// <summary>
        /// Ensures the constructor correctly parses <see cref="string"/>s and <see cref="Version"/>s.
        /// </summary>
        [Fact]
        public void TestVersionConstructor()
        {
            new ImplementationVersion(new Version(1, 2)).Should().Be(new ImplementationVersion("1.2"));
            new ImplementationVersion(new Version(1, 2, 3)).Should().Be(new ImplementationVersion("1.2.3"));
            new ImplementationVersion(new Version(1, 2, 3, 4)).Should().Be(new ImplementationVersion("1.2.3.4"));
        }

        /// <summary>
        /// Ensures the <see cref="Version"/> constructor correctly handles template variables.
        /// </summary>
        [Fact]
        public void TestTemplateVariable()
        {
            var version = new ImplementationVersion("1-pre{var}");
            version.ContainsTemplateVariables.Should().BeTrue();
            version.ToString().Should().Be("1-pre{var}");

            version = new ImplementationVersion("{var}");
            version.ContainsTemplateVariables.Should().BeTrue();
            version.ToString().Should().Be("{var}");
        }

        /// <summary>
        /// Ensures <see cref="ImplementationVersion"/> objects are correctly compared.
        /// </summary>
        [Fact]
        public void TestEquals()
        {
            new ImplementationVersion("1.2-pre-3").Should().Be(new ImplementationVersion("1.2-pre-3"));
            new ImplementationVersion("1-pre-3").Should().NotBe(new ImplementationVersion("1.2-pre-3"));
            new ImplementationVersion("1.2-post-3").Should().NotBe(new ImplementationVersion("1.2-pre-3"));
            new ImplementationVersion("1.2-pre-4").Should().NotBe(new ImplementationVersion("1.2-pre-3"));
            new ImplementationVersion("1.2-pre--3").Should().NotBe(new ImplementationVersion("1.2-pre-3"));
            new ImplementationVersion("1.2-pre").Should().NotBe(new ImplementationVersion("1.2-pre-3"));
        }

        /// <summary>
        /// Ensures <see cref="ImplementationVersion"/> objects are sorted correctly.
        /// </summary>
        [Fact]
        public void TestSort()
        {
            var sortedVersions = new[] {"0.1", "1", "1.0", "1.1", "1.2-pre", "1.2-pre1", "1.2-rc1", "1.2", "1.2-0", "1.2-post", "1.2-post1-pre", "1.2-post1", "1.2.1-pre", "1.2.1.4", "1.2.3", "1.2.10", "3"};
            for (int i = 0; i < sortedVersions.Length - 1; i++)
            {
                var v1 = new ImplementationVersion(sortedVersions[i]);
                var v2 = new ImplementationVersion(sortedVersions[i + 1]);
                v1.Should().BeLessThan(v2);
                v2.Should().BeGreaterThan(v1);
            }
        }
    }
}
