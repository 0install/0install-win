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
using System.Collections.Generic;
using System.IO;
using System.Threading;
using FluentAssertions;
using NanoByte.Common.Storage;
using NUnit.Framework;

namespace ZeroInstall.Store
{
    /// <summary>
    /// Contains test methods for <see cref="Config"/>.
    /// </summary>
    [TestFixture]
    public class ConfigTest
    {
        /// <summary>
        /// Creates test <see cref="Config"/>.
        /// </summary>
        public static Config CreateTestConfig() => new Config
        {
            HelpWithTesting = true,
            Freshness = new TimeSpan(12, 0, 0, 0),
            NetworkUse = NetworkLevel.Minimal,
            AutoApproveKeys = false,
            SyncServerPassword = "pw123"
        };

        [Test(Description = "Ensures that the class can be correctly cloned.")]
        public void TestClone()
        {
            var config1 = CreateTestConfig();
            var config2 = config1.Clone();

            // Ensure data stayed the same
            config2.Should().Be(config1, because: "Cloned objects should be equal.");
            config2.GetHashCode().Should().Be(config1.GetHashCode(), because: "Cloned objects' hashes should be equal.");
            config2.Should().NotBeSameAs(config1, because: "Cloning should not return the same reference.");
        }

        /// <summary>
        /// Ensures that the class is correctly serialized and deserialized.
        /// </summary>
        [Test]
        public void TestSaveLoad()
        {
            Config config1, config2;
            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                // Write and read file
                config1 = CreateTestConfig();
                config1.Save(tempFile);
                config2 = Config.Load(tempFile);
            }

            // Ensure data stayed the same
            config2.Should().Be(config1, because: "Serialized objects should be equal.");
            config2.GetHashCode().Should().Be(config1.GetHashCode(), because: "Serialized objects' hashes should be equal.");
            config2.Should().NotBeSameAs(config1, because: "Serialized objects should not return the same reference.");
        }

        /// <summary>
        /// Ensures <see cref="Config.GetOption"/> and <see cref="Config.SetOption"/> properly access the settings properties.
        /// </summary>
        [Test]
        public void TestGetSetValue()
        {
            var config = new Config();
            config.Invoking(x => x.SetOption("Test", "Test")).ShouldThrow<KeyNotFoundException>();

            config.HelpWithTesting.Should().BeFalse();
            config.GetOption("help_with_testing").Should().Be("False");
            config.SetOption("help_with_testing", "True");
            config.Invoking(x => x.SetOption("help_with_testing", "Test")).ShouldThrow<FormatException>();
            config.HelpWithTesting.Should().BeTrue();
            config.GetOption("help_with_testing").Should().Be("True");

            config.SetOption("freshness", "10");
            config.Freshness.Should().Be(TimeSpan.FromSeconds(10));
            config.GetOption("freshness").Should().Be("10");
        }

        /// <summary>
        /// Ensures <see cref="Config.Save(string)"/> preserves unknown properties loaded in <see cref="Config.Load(string)"/>.
        /// </summary>
        [Test]
        public void TestRetainUnknownProperties()
        {
            string testIniData = "[global]" + Environment.NewLine + "test = test" + Environment.NewLine;

            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                File.WriteAllText(tempFile, testIniData);
                Config.Load(tempFile).Save(tempFile);
                File.ReadAllText(tempFile).Should().Be(testIniData);
            }
        }

        [Test]
        public void StressTest()
        {
            using (new LocationsRedirect("0install-unit-tests"))
            {
                new Config().Save();

                Exception exception = null;
                var threads = new Thread[100];
                for (int i = 0; i < threads.Length; i++)
                {
                    threads[i] = new Thread(() =>
                    {
                        try
                        {
                            Config.Load();
                        }
                        catch (Exception ex)
                        {
                            exception = ex;
                        }
                    });
                    threads[i].Start();
                }

                foreach (var thread in threads)
                    thread.Join();
                if (exception != null)
                    Assert.Fail(exception.ToString());
            }
        }
    }
}
