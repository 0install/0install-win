/*
 * Copyright 2010 Bastian Eicher
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
using System.Security.Cryptography;
using Common.Helpers;
using Common.Storage;
using NUnit.Framework;
using ZeroInstall.Model;
using System.IO;

namespace ZeroInstall.Store.Implementation
{
    static class DirectoryHelper
    {
        /// <summary>
        /// Finds a free folder or file name by optionally appending a number
        /// to a path string.
        /// </summary>
        /// <returns>the modified preferred path</returns>
        public static string FindInexistantPath(string preferredPath)
        {
            if (!System.IO.Directory.Exists(preferredPath))
                return preferredPath;

            int suffix = 1;
            while (System.IO.Directory.Exists(preferredPath + suffix))
            {
                preferredPath = preferredPath + "_";
                ++suffix;
            }
            return preferredPath + suffix;
        }
    }

    /// <summary>
    /// Helper class to allow operating on a temporary directory from within a
    /// using statement block.
    /// </summary>
    public class TemporaryDirectory : IDisposable
    {
        private readonly string _path;

        public string Path
        {
            get { return _path; }
        }

        public TemporaryDirectory(string path)
        {
            string inexistantPath = DirectoryHelper.FindInexistantPath(path);
            System.IO.Directory.CreateDirectory(inexistantPath);
            _path = inexistantPath;
        }

        public void Dispose()
        {
            System.IO.Directory.Delete(_path, recursive: true);
        }
    }

    public class DirectoryReplacement : IDisposable
    {
        private readonly string _path, _backup;

        public string Path
        {
            get { return _path; }
        }

        public DirectoryReplacement(string path)
        {
            if (System.IO.Directory.Exists(path))
            {
                string inexistantPath = DirectoryHelper.FindInexistantPath(path);
                System.IO.Directory.Move(path, inexistantPath);
                _backup = inexistantPath;
            }
            System.IO.Directory.CreateDirectory(path);
            _path = path;
        }

        public void Dispose()
        {
            System.IO.Directory.Delete(_path, recursive: true);
            if(!String.IsNullOrEmpty(_backup))
            {
                System.IO.Directory.Move(_backup, _path);
            }
        }
    }

    /// <summary>
    /// Helper class to move an existing directory to a temporary directory only within a
    /// using statement block.
    /// </summary>
    public class TemporaryMove : IDisposable
    {
        private readonly string _originalPath, _movedPath;

        public string MovedPath
        {
            get { return _movedPath; }
        }

        public TemporaryMove(string path)
        {
            if (System.IO.Directory.Exists(path))
            {
                string inexistantPath = DirectoryHelper.FindInexistantPath(path);
                System.IO.Directory.Move(path, inexistantPath);
                _originalPath = path;
                _movedPath = inexistantPath;
            }
        }

        public void Dispose()
        {
            if (!String.IsNullOrEmpty(_originalPath))
            {
                System.IO.Directory.Delete(_originalPath, recursive: true);
                System.IO.Directory.Move(_movedPath, _originalPath);
            }
        }
    }

    [TestFixture]
    public class StoreCreation
    {
        [Test]
        public void ShouldAcceptAnExistingPath()
        {
            using (var dir = new TemporaryDirectory(Path.GetFullPath("test-store")))
            {
                Assert.DoesNotThrow(delegate { new Store(dir.Path); }, "Store must instantiate given an existing path");
            }
        }

        [Test]
        public void ShouldRejectInexistantPath()
        {
            var path = DirectoryHelper.FindInexistantPath(Path.GetFullPath("test-store"));
            Assert.Throws<DirectoryNotFoundException>(delegate { new Store(path); }, "Store must throw DirectoryNotFoundException created with non-existing path");
        }

        [Test]
        public void ShouldAcceptRelativePath()
        {
            using (var dir = new TemporaryDirectory("relative-path"))
            {
                Assert.False(Path.IsPathRooted(dir.Path), "Internal assertion: Test path must be relative");
                Assert.DoesNotThrow(delegate { new Store(dir.Path); }, "Store must accept relative paths");
            }
        }

        [Test]
        public void ShouldProvideDefaultConstructor()
        {
            string cachePath = Locations.GetUserCacheDir("0install");
            using (var cache = new DirectoryReplacement(cachePath))
            {
                Assert.DoesNotThrow(delegate { new Store(); }, "Store must be default constructible");
            }
        }

        [Test]
        public void DefaultConstructorShouldFailIfCacheDirInexistant()
        {
            string cache = Locations.GetUserCacheDir("0install");
            using (new TemporaryMove(cache))
            {
                Assert.Throws<DirectoryNotFoundException>(delegate { new Store(); }, "Store must throw DirectoryNotFoundException created with non-existing path");
            }
        }
    }

    [TestFixture]
    public class StoreFunctionality
    {
        TemporaryDirectory _cache;
        Store _store;

        [TestFixtureSetUp]
        public void SetUp()
        {
            _cache = new TemporaryDirectory(Path.GetFullPath("test-store"));
            System.IO.Directory.CreateDirectory(_cache.Path);
            _store = new Store(_cache.Path);
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            System.IO.Directory.Delete(_cache.Path, recursive: true);
        }

        [Test]
        public void ShouldTellIfItContainsAnImplementation()
        {
            using ( var temporaryDir = new TemporaryDirectory("temp"))
            {
                string packageDir = Path.Combine(temporaryDir.Path, "package");
                System.IO.Directory.CreateDirectory(packageDir);

                string contentFilePath = Path.Combine(packageDir, "content");
                using (var contentFile = System.IO.File.Create(contentFilePath))
                {
                    for(int i = 0; i < 1000; ++i)
                        contentFile.WriteByte((byte)'A');
                }

                string manifestPath = Path.Combine(packageDir, ".manifest");
                var manifest = NewManifest.Generate(packageDir, SHA256.Create());
                manifest.Save(manifestPath);
                string hash = FileHelper.ComputeHash(manifestPath, SHA256.Create());

                System.IO.Directory.Move(packageDir, Path.Combine(_cache.Path, "sha256=" + hash));

                Assert.True(_store.Contains(new ManifestDigest(null, null, hash)));
            }
        }
    }
}
