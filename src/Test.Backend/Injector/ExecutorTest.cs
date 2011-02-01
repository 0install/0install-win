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
using System.IO;
using Common.Utils;
using NUnit.Framework;
using NUnit.Mocks;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Injector
{
    /// <summary>
    /// Contains test methods for <see cref="Executor"/>.
    /// </summary>
    [TestFixture]
    public class ExecutorTest
    {
        #region Shared
        private DynamicMock _storeMock;

        private IStore TestStore
        {
            get { return (IStore)_storeMock.MockInstance; }
        }

        [SetUp]
        public void SetUp()
        {
            // Prepare mock objects that will be injected with methods in the tests
            _storeMock = new DynamicMock("MockStore", typeof(IStore));
        }

        [TearDown]
        public void TearDown()
        {
            // Ensure no method calls were left out
            _storeMock.Verify();
        }
        #endregion

        /// <summary>
        /// Ensures the <see cref="Executor"/> constructor throws the correct exceptions.
        /// </summary>
        [Test]
        public void TestExceptions()
        {
            Assert.Throws<ArgumentException>(() => new Executor(new Selections(), TestStore), "Empty selections should be rejected");

            var selections = SelectionsTest.CreateTestSelections();
            selections.Commands[1].WorkingDir = new WorkingDir();
            _storeMock.SetReturnValue("GetPath", "test path");
            var executor = new Executor(selections, TestStore);
            Assert.Throws<CommandException>(() => executor.GetStartInfo(""), "Multiple WorkingDir changes should be rejected");
        }

        /// <summary>
        /// Ensures <see cref="Executor.GetStartInfo"/> handles complex <see cref="Selections"/>.
        /// </summary>
        [Test]
        public void TestGetStartInfo()
        {
            var selections = SelectionsTest.CreateTestSelections();

            _storeMock.ExpectAndReturn("GetPath", "test1 path", selections.Implementations[0].ManifestDigest);
            _storeMock.ExpectAndReturn("GetPath", "test2 path", selections.Implementations[1].ManifestDigest);
            _storeMock.ExpectAndReturn("GetPath", "test1 path", selections.Implementations[0].ManifestDigest);
            _storeMock.ExpectAndReturn("GetPath", "test2 path", selections.Implementations[1].ManifestDigest);
            _storeMock.ExpectAndReturn("GetPath", "test1 path", selections.Implementations[0].ManifestDigest);

            var executor = new Executor(selections, TestStore);
            var startInfo = executor.GetStartInfo("--custom");
            Assert.AreEqual(
                Path.Combine("test2 path", StringUtils.UnifySlashes(selections.Commands[1].Path)),
                startInfo.FileName);
            Assert.AreEqual(
                selections.Commands[1].Arguments[0] + " " + selections.Commands[0].Runner.Arguments[0] + " \"" + Path.Combine("test1 path", StringUtils.UnifySlashes(selections.Commands[0].Path)) + "\" " + selections.Commands[0].Arguments[0] + " --custom",
                startInfo.Arguments);
            Assert.AreEqual(Path.Combine("test1 path", "bin"), startInfo.WorkingDirectory);
            Assert.AreEqual("test1 path", startInfo.EnvironmentVariables["TEST1_PATH"]);
            Assert.AreEqual("test1", startInfo.EnvironmentVariables["TEST1_VALUE"]);
            Assert.AreEqual("test2 path", startInfo.EnvironmentVariables["TEST2_PATH"]);
            Assert.AreEqual("test2", startInfo.EnvironmentVariables["TEST2_VALUE"]);
        }

        /// <summary>
        /// Ensures <see cref="Executor.GetStartInfo"/> handles <see cref="Selections"/> with <see cref="Command.Path"/>s that are empty.
        /// </summary>
        [Test]
        public void TestGetStartInfoPathlessCommand()
        {
            var selections = SelectionsTest.CreateTestSelections();
            selections.Commands[0].Path = null;

            _storeMock.ExpectAndReturn("GetPath", "test2 path", selections.Implementations[1].ManifestDigest);
            _storeMock.ExpectAndReturn("GetPath", "test1 path", selections.Implementations[0].ManifestDigest);
            _storeMock.ExpectAndReturn("GetPath", "test2 path", selections.Implementations[1].ManifestDigest);
            _storeMock.ExpectAndReturn("GetPath", "test1 path", selections.Implementations[0].ManifestDigest);

            var executor = new Executor(selections, TestStore);
            var startInfo = executor.GetStartInfo("--custom");
            Assert.AreEqual(
                Path.Combine("test2 path", StringUtils.UnifySlashes(selections.Commands[1].Path)),
                startInfo.FileName);
            Assert.AreEqual(
                selections.Commands[1].Arguments[0] + " " + selections.Commands[0].Runner.Arguments[0] + " --custom",
                startInfo.Arguments);
            Assert.AreEqual(Path.Combine("test1 path", "bin"), startInfo.WorkingDirectory);
            Assert.AreEqual("test1 path", startInfo.EnvironmentVariables["TEST1_PATH"]);
            Assert.AreEqual("test1", startInfo.EnvironmentVariables["TEST1_VALUE"]);
            Assert.AreEqual("test2 path", startInfo.EnvironmentVariables["TEST2_PATH"]);
            Assert.AreEqual("test2", startInfo.EnvironmentVariables["TEST2_VALUE"]);
        }

        /// <summary>
        /// Ensures <see cref="Executor.GetStartInfo"/> handles complex <see cref="Selections"/> and <see cref="Executor.Wrapper"/>.
        /// </summary>
        [Test]
        public void TestGetStartInfoWrapper()
        {
            var selections = SelectionsTest.CreateTestSelections();

            _storeMock.ExpectAndReturn("GetPath", "test1 path", selections.Implementations[0].ManifestDigest);
            _storeMock.ExpectAndReturn("GetPath", "test2 path", selections.Implementations[1].ManifestDigest);
            _storeMock.ExpectAndReturn("GetPath", "test1 path", selections.Implementations[0].ManifestDigest);
            _storeMock.ExpectAndReturn("GetPath", "test2 path", selections.Implementations[1].ManifestDigest);
            _storeMock.ExpectAndReturn("GetPath", "test1 path", selections.Implementations[0].ManifestDigest);

            var executor = new Executor(selections, TestStore) {Wrapper = "wrapper --wrapper"};
            var startInfo = executor.GetStartInfo("--custom");
            Assert.AreEqual("wrapper", startInfo.FileName);
            Assert.AreEqual(
                "--wrapper \"" + Path.Combine("test2 path", StringUtils.UnifySlashes(selections.Commands[1].Path)) + "\" " + selections.Commands[1].Arguments[0] + " " + selections.Commands[0].Runner.Arguments[0] + " \"" + Path.Combine("test1 path", StringUtils.UnifySlashes(selections.Commands[0].Path)) + "\" " + selections.Commands[0].Arguments[0] + " --custom",
                startInfo.Arguments);
            Assert.AreEqual(Path.Combine("test1 path", "bin"), startInfo.WorkingDirectory);
            Assert.AreEqual("test1 path", startInfo.EnvironmentVariables["TEST1_PATH"]);
            Assert.AreEqual("test1", startInfo.EnvironmentVariables["TEST1_VALUE"]);
            Assert.AreEqual("test2 path", startInfo.EnvironmentVariables["TEST2_PATH"]);
            Assert.AreEqual("test2", startInfo.EnvironmentVariables["TEST2_VALUE"]);
        }

        /// <summary>
        /// Ensures <see cref="Executor.GetStartInfo"/> handles complex <see cref="Selections"/> and <see cref="Executor.Main"/> with relative paths.
        /// </summary>
        [Test]
        public void TestGetStartInfoMainRelative()
        {
            var selections = SelectionsTest.CreateTestSelections();

            _storeMock.ExpectAndReturn("GetPath", "test1 path", selections.Implementations[0].ManifestDigest);
            _storeMock.ExpectAndReturn("GetPath", "test2 path", selections.Implementations[1].ManifestDigest);
            _storeMock.ExpectAndReturn("GetPath", "test1 path", selections.Implementations[0].ManifestDigest);
            _storeMock.ExpectAndReturn("GetPath", "test2 path", selections.Implementations[1].ManifestDigest);
            _storeMock.ExpectAndReturn("GetPath", "test1 path", selections.Implementations[0].ManifestDigest);

            var executor = new Executor(selections, TestStore) {Main = "main"};
            var startInfo = executor.GetStartInfo("--custom");
            Assert.AreEqual(Path.Combine("test2 path", StringUtils.UnifySlashes(selections.Commands[1].Path)), startInfo.FileName);
            Assert.AreEqual(selections.Commands[1].Arguments[0] + " " + selections.Commands[0].Runner.Arguments[0] + " \"" + StringUtils.PathCombine("test1 path", "dir 1", "main") + "\" --custom", startInfo.Arguments);
            Assert.AreEqual(Path.Combine("test1 path", "bin"), startInfo.WorkingDirectory);
            Assert.AreEqual("test1 path", startInfo.EnvironmentVariables["TEST1_PATH"]);
            Assert.AreEqual("test1", startInfo.EnvironmentVariables["TEST1_VALUE"]);
            Assert.AreEqual("test2 path", startInfo.EnvironmentVariables["TEST2_PATH"]);
            Assert.AreEqual("test2", startInfo.EnvironmentVariables["TEST2_VALUE"]);
        }

        /// <summary>
        /// Ensures <see cref="Executor.GetStartInfo"/> handles complex <see cref="Selections"/> and <see cref="Executor.Main"/> with absolute paths.
        /// </summary>
        [Test]
        public void TestGetStartInfoMainAbsolute()
        {
            var selections = SelectionsTest.CreateTestSelections();

            _storeMock.ExpectAndReturn("GetPath", "test1 path", selections.Implementations[0].ManifestDigest);
            _storeMock.ExpectAndReturn("GetPath", "test2 path", selections.Implementations[1].ManifestDigest);
            _storeMock.ExpectAndReturn("GetPath", "test1 path", selections.Implementations[0].ManifestDigest);
            _storeMock.ExpectAndReturn("GetPath", "test2 path", selections.Implementations[1].ManifestDigest);
            _storeMock.ExpectAndReturn("GetPath", "test1 path", selections.Implementations[0].ManifestDigest);

            var executor = new Executor(selections, TestStore) {Main = "/main"};
            var startInfo = executor.GetStartInfo("--custom");
            Assert.AreEqual(
                Path.Combine("test2 path", StringUtils.UnifySlashes(selections.Commands[1].Path)),
                startInfo.FileName);
            Assert.AreEqual(
                selections.Commands[1].Arguments[0] + " " + selections.Commands[0].Runner.Arguments[0] + " \"" + StringUtils.PathCombine("test1 path", "main") + "\" --custom",
                startInfo.Arguments);
            Assert.AreEqual(Path.Combine("test1 path", "bin"), startInfo.WorkingDirectory);
            Assert.AreEqual("test1 path", startInfo.EnvironmentVariables["TEST1_PATH"]);
            Assert.AreEqual("test1", startInfo.EnvironmentVariables["TEST1_VALUE"]);
            Assert.AreEqual("test2 path", startInfo.EnvironmentVariables["TEST2_PATH"]);
            Assert.AreEqual("test2", startInfo.EnvironmentVariables["TEST2_VALUE"]);
        }
    }
}
