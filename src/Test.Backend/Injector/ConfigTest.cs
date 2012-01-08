/*
 * Copyright 2010-2012 Bastian Eicher
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
using NUnit.Framework;

namespace ZeroInstall.Injector
{
    /// <summary>
    /// Contains test methods for <see cref="Config"/>.
    /// </summary>
    [TestFixture]
    public class ConfigTest
    {
        #region Helpers
        /// <summary>
        /// Creates test <see cref="Config"/>.
        /// </summary>
        public static Config CreateTestConfig()
        {
            return new Config {HelpWithTesting = true, Freshness = new TimeSpan(12, 0, 0, 0), NetworkUse = NetworkLevel.Minimal, AutoApproveKeys = false, SelfUpdateID = "invalid"};
        }
        #endregion

        [Test(Description = "Ensures that the class can be correctly cloned.")]
        public void TestClone()
        {
            var config1 = CreateTestConfig();
            var config2 = config1.Clone();

            // Ensure data stayed the same
            Assert.AreEqual(config1, config2, "Cloned objects should be equal.");
            Assert.AreEqual(config1.GetHashCode(), config2.GetHashCode(), "Cloned objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(config1, config2), "Cloning should not return the same reference.");
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
                config1.Save(tempFile.Path);
                config2 = Config.Load(tempFile.Path);
            }

            // Ensure data stayed the same
            Assert.AreEqual(config1, config2, "Serialized objects should be equal.");
            Assert.AreEqual(config1.GetHashCode(), config2.GetHashCode(), "Serialized objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(config1, config2), "Serialized objects should not return the same reference.");
        }

        /// <summary>
        /// Ensures <see cref="Config.GetOption"/> and <see cref="Config.SetOption"/> properly access the settings properties.
        /// </summary>
        [Test]
        public void TestGetSetValue()
        {
            var config = new Config();
            Assert.Throws<C5.NoSuchItemException>(() => config.SetOption("Test", "Test"));

            Assert.IsFalse(config.HelpWithTesting);
            Assert.AreEqual("False", config.GetOption("help_with_testing"));
            config.SetOption("help_with_testing", "True");
            Assert.Throws<FormatException>(() => config.SetOption("help_with_testing", "Test"));
            Assert.IsTrue(config.HelpWithTesting);
            Assert.AreEqual("True", config.GetOption("help_with_testing"));

            config.SetOption("freshness", "10");
            Assert.AreEqual(TimeSpan.FromSeconds(10), config.Freshness);
            Assert.AreEqual("10", config.GetOption("freshness"));
        }

        /// <summary>
        /// Ensures <see cref="Config.Save(string)"/> preserves unknown properties loaded in <see cref="Config.Load(string)"/>.
        /// </summary>
        [Test]
        public void TestRetainUnknownProperties()
        {
            string testIniData = "[global]" + Environment.NewLine + "test = test" + Environment.NewLine + Environment.NewLine;

            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                File.WriteAllText(tempFile.Path, testIniData);
                Config.Load(tempFile.Path).Save(tempFile.Path);
                Assert.AreEqual(testIniData, File.ReadAllText(tempFile.Path));
            }
        }
    }
}
