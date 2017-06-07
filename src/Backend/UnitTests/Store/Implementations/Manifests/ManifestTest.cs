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
using System.Text.RegularExpressions;
using FluentAssertions;
using JetBrains.Annotations;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NanoByte.Common.Streams;
using NanoByte.Common.Tasks;
using Xunit;
using ZeroInstall.FileSystem;
using ZeroInstall.Services;
using ZeroInstall.Store.Implementations.Build;

namespace ZeroInstall.Store.Implementations.Manifests
{
    /// <summary>
    /// Contains test methods for <see cref="Manifest"/>.
    /// </summary>
    public class ManifestTest : TestWithMocks
    {
        #region Helpers
        private static Manifest GenerateManifest(string path, ManifestFormat format, ITaskHandler handler)
        {
            var generator = new ManifestGenerator(path, format);
            handler.RunTask(generator);
            return generator.Manifest;
        }

        /// <summary>
        /// Generates a manifest for a directory in the filesystem and writes the manifest to a file named Manifest.ManifestFile in that directory.
        /// </summary>
        /// <param name="path">The path of the directory to analyze.</param>
        /// <param name="format">The format of the manifest (which file details are listed, which digest method is used, etc.).</param>
        /// <param name="handler">A callback object used when the the user is to be informed about progress.</param>
        /// <returns>The manifest digest.</returns>
        /// <exception cref="IOException">A problem occurs while writing the file.</exception>
        /// <remarks>
        /// The exact format is specified here: http://0install.net/manifest-spec.html
        /// </remarks>
        public static string CreateDotFile([NotNull] string path, [NotNull] ManifestFormat format, [NotNull] ITaskHandler handler)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
            if (format == null) throw new ArgumentNullException(nameof(format));
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            #endregion

            var generator = new ManifestGenerator(path, format);
            handler.RunTask(generator);
            return generator.Manifest.Save(Path.Combine(path, Manifest.ManifestFile));
        }
        #endregion

        [Fact] // Ensures that Manifest is correctly generated, serialized and deserialized.
        public void TestSaveLoad()
        {
            var manifest1 = new Manifest(ManifestFormat.Sha1New,
                new ManifestDirectory("subdir"),
                new ManifestNormalFile("abc123", TestFile.DefaultLastWrite, 3, "file"));
            Manifest manifest2;
            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                // Generate manifest, write it to a file and read the file again
                manifest1.Save(tempFile);
                manifest2 = Manifest.Load(tempFile, ManifestFormat.Sha1New);
            }

            // Ensure data stayed the same
            manifest2.Should().Equal(manifest1);
        }

        [Fact] // Ensures damaged manifest lines are correctly identified.
        public void TestLoadException()
        {
            Assert.Throws<FormatException>(() => Manifest.Load("test".ToStream(), ManifestFormat.Sha1New));
            Assert.Throws<FormatException>(() => Manifest.Load("test".ToStream(), ManifestFormat.Sha256));
            Assert.Throws<FormatException>(() => Manifest.Load("test".ToStream(), ManifestFormat.Sha256New));
            Manifest.Load("D /test".ToStream(), ManifestFormat.Sha1New);
            Manifest.Load("D /test".ToStream(), ManifestFormat.Sha256);
            Manifest.Load("D /test".ToStream(), ManifestFormat.Sha256New);

            Manifest.Load("F abc123 1200000000 128 test".ToStream(), ManifestFormat.Sha1New);
            Manifest.Load("F abc123 1200000000 128 test".ToStream(), ManifestFormat.Sha256);
            Manifest.Load("F abc123 1200000000 128 test".ToStream(), ManifestFormat.Sha256New);

            Assert.Throws<FormatException>(() => Manifest.Load("F abc123 128 test".ToStream(), ManifestFormat.Sha1New));
            Assert.Throws<FormatException>(() => Manifest.Load("F abc123 128 test".ToStream(), ManifestFormat.Sha256));
            Assert.Throws<FormatException>(() => Manifest.Load("F abc123 128 test".ToStream(), ManifestFormat.Sha256New));
        }

        [Fact]
        public void TestCalculateDigest()
        {
            using (var testDir = new TemporaryDirectory("0install-unit-tests"))
            {
                new TestRoot{
                    new TestDirectory("subdir") {new TestFile("file")}
                }.Build(testDir);

                GenerateManifest(testDir, ManifestFormat.Sha1New, new MockTaskHandler()).CalculateDigest()
                    .Should().Be(CreateDotFile(testDir, ManifestFormat.Sha1New, new MockTaskHandler()),
                        because: "sha1new dot file and digest should match");
                GenerateManifest(testDir, ManifestFormat.Sha256, new MockTaskHandler()).CalculateDigest()
                    .Should().Be(CreateDotFile(testDir, ManifestFormat.Sha256, new MockTaskHandler()),
                        because: "sha256 dot file and digest should match");
                GenerateManifest(testDir, ManifestFormat.Sha256New, new MockTaskHandler()).CalculateDigest()
                    .Should().Be(CreateDotFile(testDir, ManifestFormat.Sha256New, new MockTaskHandler()),
                        because: "sha256new dot file and digest should match");
            }
        }

        [Fact] // Ensures that ToXmlString() correctly outputs a serialized form of the manifest.
        public void TestToString()
        {
            using (var testDir = new TemporaryDirectory("0install-unit-tests"))
            {
                new TestRoot
                {
                    new TestDirectory("subdir") {new TestFile("file")}
                }.Build(testDir);

                var manifest = GenerateManifest(testDir, ManifestFormat.Sha1New, new MockTaskHandler());
                manifest.ToString().Replace(Environment.NewLine, "\n")
                    .Should().Be("D /subdir\nF 606ec6e9bd8a8ff2ad14e5fade3f264471e82251 946684800 3 file\n");
            }
        }

        [Fact]
        public void TestListPaths()
        {
            var normalFile = new ManifestNormalFile("123", new DateTime(), 10, "normal");
            var dir1 = new ManifestDirectory("/dir1");
            var executableFile = new ManifestExecutableFile("123", new DateTime(), 10, "executable");
            var dir2 = new ManifestDirectory("/dir2");
            var symlink = new ManifestSymlink("123", 10, "symlink");
            var manifest = new Manifest(ManifestFormat.Sha256New, normalFile, dir1, executableFile, dir2, symlink);

            manifest.ListPaths().Should().Equal(
                new KeyValuePair<string, ManifestNode>("normal", normalFile),
                new KeyValuePair<string, ManifestNode>("dir1", dir1),
                new KeyValuePair<string, ManifestNode>(Path.Combine("dir1", "executable"), executableFile),
                new KeyValuePair<string, ManifestNode>("dir2", dir2),
                new KeyValuePair<string, ManifestNode>(Path.Combine("dir2", "symlink"), symlink));
        }

        // ReSharper disable AssignNullToNotNullAttribute
        [Fact]
        public void ShouldListNormalWindowsExeWithFlagF()
        {
            using (var package = new TemporaryDirectory("0install-unit-tests"))
            {
                string filePath = Path.Combine(package, "test.exe");
                string manifestPath = Path.Combine(package, Manifest.ManifestFile);

                File.WriteAllText(filePath, @"xxxxxxx");
                CreateDotFile(package, ManifestFormat.Sha256, new MockTaskHandler());

                using (var manifest = File.OpenText(manifestPath))
                {
                    string firstLine = manifest.ReadLine();
                    Assert.True(Regex.IsMatch(firstLine, @"^F \w+ \d+ \d+ test.exe$"), "Manifest didn't match expected format");
                }
            }
        }

        [Fact]
        public void ShouldListFilesInXbitWithFlagX()
        {
            using (var package = new TemporaryDirectory("0install-unit-tests"))
            {
                string filePath = Path.Combine(package, "test.exe");
                string manifestPath = Path.Combine(package, Manifest.ManifestFile);

                File.WriteAllText(filePath, "target");
                if (WindowsUtils.IsWindows)
                {
                    string flagPath = Path.Combine(package, FlagUtils.XbitFile);
                    File.WriteAllText(flagPath, @"/test.exe");
                }
                else FileUtils.SetExecutable(filePath, true);
                CreateDotFile(package, ManifestFormat.Sha256, new MockTaskHandler());

                using (var manifest = File.OpenText(manifestPath))
                {
                    string firstLine = manifest.ReadLine();
                    Assert.True(Regex.IsMatch(firstLine, @"^X \w+ \d+ \d+ test.exe$"), "Manifest didn't match expected format");
                }
            }
        }

        [Fact]
        public void ShouldListFilesInSymlinkWithFlagS()
        {
            using (var package = new TemporaryDirectory("0install-unit-tests"))
            {
                string sourcePath = Path.Combine(package, "test");
                string manifestPath = Path.Combine(package, Manifest.ManifestFile);

                if (WindowsUtils.IsWindows)
                {
                    File.WriteAllText(sourcePath, "target");
                    string flagPath = Path.Combine(package, FlagUtils.SymlinkFile);
                    File.WriteAllText(flagPath, @"/test");
                }
                else FileUtils.CreateSymlink(sourcePath, "target");
                CreateDotFile(package, ManifestFormat.Sha256, new MockTaskHandler());

                using (var manifest = File.OpenText(manifestPath))
                {
                    string firstLine = manifest.ReadLine();
                    Assert.True(Regex.IsMatch(firstLine, @"^S \w+ \d+ test$"), "Manifest didn't match expected format");
                }
            }
        }

        [Fact]
        public void ShouldListNothingForEmptyPackage()
        {
            using (var package = new TemporaryDirectory("0install-unit-tests"))
            {
                CreateDotFile(package, ManifestFormat.Sha256, new MockTaskHandler());
                using (var manifestFile = File.OpenRead(Path.Combine(package, Manifest.ManifestFile)))
                    manifestFile.Length.Should().Be(0, because: "Empty package directory should make an empty manifest");
            }
        }

        [Fact]
        public void ShouldHandleSubdirectoriesWithExecutables()
        {
            using (var package = new TemporaryDirectory("0install-unit-tests"))
            {
                string innerPath = Path.Combine(package, "inner");
                Directory.CreateDirectory(innerPath);

                string innerExePath = Path.Combine(innerPath, "inner.exe");
                string manifestPath = Path.Combine(package, Manifest.ManifestFile);
                File.WriteAllText(innerExePath, @"xxxxxxx");
                if (WindowsUtils.IsWindows)
                {
                    string flagPath = Path.Combine(package, FlagUtils.XbitFile);
                    File.WriteAllText(flagPath, @"/inner/inner.exe");
                }
                else FileUtils.SetExecutable(innerExePath, true);
                CreateDotFile(package, ManifestFormat.Sha256, new MockTaskHandler());
                using (var manifestFile = File.OpenText(manifestPath))
                {
                    string currentLine = manifestFile.ReadLine();
                    Assert.True(Regex.IsMatch(currentLine, @"^D /inner$"), "Manifest didn't match expected format:\n" + currentLine);
                    currentLine = manifestFile.ReadLine();
                    Assert.True(Regex.IsMatch(currentLine, @"^X \w+ \w+ \d+ inner.exe$"), "Manifest didn't match expected format:\n" + currentLine);
                }
            }
        }

        // ReSharper restore AssignNullToNotNullAttribute

        [SkippableFact]
        public void ShouldNotFollowDirectorySymlinks()
        {
            Skip.IfNot(UnixUtils.IsUnix, "Can only test symlinks on Unixoid system");

            using (var package = new TemporaryDirectory("0install-unit-tests"))
            {
                Directory.CreateDirectory(Path.Combine(package, "target"));
                FileUtils.CreateSymlink(Path.Combine(package, "source"), "target");
                var manifest = GenerateManifest(package, ManifestFormat.Sha256New, new MockTaskHandler());

                (manifest[0] is ManifestSymlink).Should().BeTrue(because: "Unexpected manifest:\n" + manifest);
                ((ManifestSymlink)manifest[0]).Name.Should().Be("source", because: "Unexpected manifest:\n" + manifest);
                (manifest[1] is ManifestDirectory).Should().BeTrue(because: "Unexpected manifest:\n" + manifest);
                ((ManifestDirectory)manifest[1]).FullPath.Should().Be("/target", because: "Unexpected manifest:\n" + manifest);
            }
        }
    }
}
