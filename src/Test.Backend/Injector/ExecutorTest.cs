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
using System.Diagnostics;
using System.IO;
using Common.Storage;
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
        #region Constants
        private const string Test1Path = "test1 path", Test2Path = "test2 path";
        #endregion

        #region Shared
        private DynamicMock _storeMock;

        private IStore TestStore { get { return (IStore)_storeMock.MockInstance; } }

        private TemporaryDirectory _tempDir;

        [SetUp]
        public void SetUp()
        {
            // Prepare mock objects that will be injected with methods in the tests
            _storeMock = new DynamicMock("MockStore", typeof(IStore));

            // Don't store generated executables settings in real user profile
            _tempDir = new TemporaryDirectory("0install-unit-tests");
            Locations.PortableBase = _tempDir.Path;
            Locations.IsPortable = true;
        }

        [TearDown]
        public void TearDown()
        {
            // Ensure no method calls were left out
            _storeMock.Verify();

            Locations.PortableBase = Locations.InstallBase;
            _tempDir.Dispose();
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
            selections.Implementations[1].Commands[0].WorkingDir = new WorkingDir();
            _storeMock.SetReturnValue("GetPath", "test path");
            var executor = new Executor(selections, TestStore);
            Assert.Throws<CommandException>(() => executor.GetStartInfo(), "Multiple WorkingDir changes should be rejected");
        }

        private void PrepareStoreMock(Selections selections, bool emptyInnerCommand)
        {
            _storeMock.ExpectAndReturn("GetPath", Test1Path, selections.Implementations[1].ManifestDigest); // Self-binding for first implementation
            _storeMock.ExpectAndReturn("GetPath", Test2Path, selections.Implementations[2].ManifestDigest); // Binding for dependency from first to second implementation
            _storeMock.ExpectAndReturn("GetPath", Test2Path, selections.Implementations[2].ManifestDigest); // Self-binding for second implementation

            // Binding for dependency from second to first implementation as executable
            for (int i = 0; i < 2; i++)
            { // For ExecutableInVar and ExecutableInPath
                _storeMock.ExpectAndReturn("GetPath", Test1Path, selections.Implementations[1].ManifestDigest); // Binding for command dependency from second to first implementation
                _storeMock.ExpectAndReturn("GetPath", Test2Path, selections.Implementations[2].ManifestDigest); // Second/outer/runner command for command-line
                _storeMock.ExpectAndReturn("GetPath", Test1Path, selections.Implementations[1].ManifestDigest); // First/inner command for command-line
            }

            _storeMock.ExpectAndReturn("GetPath", Test1Path, selections.Implementations[1].ManifestDigest); // Self-binding for first command
            _storeMock.ExpectAndReturn("GetPath", Test1Path, selections.Implementations[1].ManifestDigest); // Working dir for first/inner command
            _storeMock.ExpectAndReturn("GetPath", Test1Path, selections.Implementations[1].ManifestDigest); // Binding for command dependency from second to first implementation
            _storeMock.ExpectAndReturn("GetPath", Test2Path, selections.Implementations[2].ManifestDigest); // Second/outer/runner command for command-line
            if (!emptyInnerCommand)
                _storeMock.ExpectAndReturn("GetPath", Test1Path, selections.Implementations[1].ManifestDigest); // First/inner command for command-line
        }

        private static void CheckEnvironment(ProcessStartInfo startInfo, Selections selections)
        {
            Assert.AreEqual("default" + Path.PathSeparator + Test1Path, startInfo.EnvironmentVariables["TEST1_PATH_SELF"], "Should append implementation path");
            Assert.AreEqual("test1", startInfo.EnvironmentVariables["TEST1_VALUE"], "Should directly set value");
            Assert.AreEqual(Test2Path + Path.PathSeparator + "default", startInfo.EnvironmentVariables["TEST2_PATH_SELF"], "Should prepend implementation path");
            Assert.AreEqual("test2", startInfo.EnvironmentVariables["TEST2_VALUE"], "Should directly set value");
            Assert.AreEqual("default" + Path.PathSeparator + Path.Combine(Test2Path, "sub"), startInfo.EnvironmentVariables["TEST2_PATH_SUB_DEP"], "Should append implementation sub-path");
            Assert.AreEqual(Test1Path, startInfo.EnvironmentVariables["TEST1_PATH_COMMAND"], "Should set implementation path");
            Assert.AreEqual(Test1Path + Path.PathSeparator + Test1Path + Path.PathSeparator + Test1Path, startInfo.EnvironmentVariables["TEST1_PATH_COMMAND_DEP"], "Should set implementation path for command dependency for each reference");
            Assert.AreEqual(Path.Combine(Test1Path, "bin"), startInfo.WorkingDirectory, "Should set implementation path");

            string execFile = Path.Combine(Test2Path, FileUtils.UnifySlashes(selections.Implementations[2].Commands[0].Path));
            string execArgs = StringUtils.ConcatenateEscapeArgument(new[]
            {
                selections.Implementations[2].Commands[0].Arguments[0],
                selections.Implementations[1].Commands[1].Runner.Arguments[0],
                Path.Combine(Test1Path, FileUtils.UnifySlashes(selections.Implementations[1].Commands[1].Path)),
                selections.Implementations[1].Commands[1].Arguments[0]
            });
            Assert.AreEqual(execFile, startInfo.EnvironmentVariables["0install-runenv-file-exec-in-var"]);
            Assert.AreEqual(execArgs, startInfo.EnvironmentVariables["0install-runenv-args-exec-in-var"]);
            Assert.AreEqual(execFile, startInfo.EnvironmentVariables["0install-runenv-file-exec-in-path"]);
            Assert.AreEqual(execArgs, startInfo.EnvironmentVariables["0install-runenv-args-exec-in-path"]);
        }

        /// <summary>
        /// Ensures <see cref="Executor.GetStartInfo"/> handles complex <see cref="Selections"/>.
        /// </summary>
        [Test]
        public void TestGetStartInfo()
        {
            var selections = SelectionsTest.CreateTestSelections();
            selections.Implementations.InsertFirst(new ImplementationSelection {InterfaceID = "http://0install.de/feeds/test/dummy.xml"}); // Should be ignored by Executor

            PrepareStoreMock(selections, false);

            var executor = new Executor(selections, TestStore);
            var startInfo = executor.GetStartInfo("--custom");
            Assert.AreEqual(
                Path.Combine(Test2Path, FileUtils.UnifySlashes(selections.Implementations[2].Commands[0].Path)),
                startInfo.FileName,
                "Should combine runner implementation directory with runner command path");
            Assert.AreEqual(
                StringUtils.ConcatenateEscapeArgument(new[]
                {
                    selections.Implementations[2].Commands[0].Arguments[0],
                    selections.Implementations[1].Commands[0].Runner.Arguments[0],
                    Path.Combine(Test1Path, FileUtils.UnifySlashes(selections.Implementations[1].Commands[0].Path)),
                    selections.Implementations[1].Commands[0].Arguments[0],
                    "--custom"
                }),
                startInfo.Arguments,
                "Should combine core and additional runner arguments with application implementation directory, command path and arguments");

            CheckEnvironment(startInfo, selections);
        }

        /// <summary>
        /// Ensures <see cref="Executor.GetStartInfo"/> handles complex <see cref="Selections"/> and <see cref="Executor.Wrapper"/>.
        /// </summary>
        [Test]
        public void TestGetStartInfoWrapper()
        {
            if (!WindowsUtils.IsWindows) throw new InconclusiveException("Wrapper command-line parsing relies on a Win32 API and therefore will not work on non-Windows platforms");

            var selections = SelectionsTest.CreateTestSelections();
            selections.Implementations.InsertFirst(new ImplementationSelection {InterfaceID = "http://0install.de/feeds/test/dummy.xml"}); // Should be ignored by Executor

            PrepareStoreMock(selections, false);

            var executor = new Executor(selections, TestStore) {Wrapper = "wrapper --wrapper"};
            var startInfo = executor.GetStartInfo("--custom");
            Assert.AreEqual("wrapper", startInfo.FileName);
            Assert.AreEqual(
                StringUtils.ConcatenateEscapeArgument(new[]
                {
                    "--wrapper",
                    Path.Combine(Test2Path, FileUtils.UnifySlashes(selections.Implementations[2].Commands[0].Path)),
                    selections.Implementations[2].Commands[0].Arguments[0],
                    selections.Implementations[1].Commands[0].Runner.Arguments[0],
                    Path.Combine(Test1Path, FileUtils.UnifySlashes(selections.Implementations[1].Commands[0].Path)),
                    selections.Implementations[1].Commands[0].Arguments[0],
                    "--custom"
                }),
                startInfo.Arguments,
                "Should combine wrapper arguments, runner and application");

            CheckEnvironment(startInfo, selections);
        }

        /// <summary>
        /// Ensures <see cref="Executor.GetStartInfo"/> handles complex <see cref="Selections"/> and <see cref="Executor.Main"/> with relative paths.
        /// </summary>
        [Test]
        public void TestGetStartInfoMainRelative()
        {
            var selections = SelectionsTest.CreateTestSelections();
            selections.Implementations.InsertFirst(new ImplementationSelection {InterfaceID = "http://0install.de/feeds/test/dummy.xml"}); // Should be ignored by Executor

            PrepareStoreMock(selections, false);

            var executor = new Executor(selections, TestStore) {Main = "main"};
            var startInfo = executor.GetStartInfo("--custom");
            Assert.AreEqual(
                Path.Combine(Test2Path, FileUtils.UnifySlashes(selections.Implementations[2].Commands[0].Path)),
                startInfo.FileName,
                "Should combine runner implementation directory with runner command path");
            Assert.AreEqual(
                StringUtils.ConcatenateEscapeArgument(new[]
                {
                    selections.Implementations[2].Commands[0].Arguments[0],
                    selections.Implementations[1].Commands[0].Runner.Arguments[0],
                    FileUtils.PathCombine(Test1Path, "dir 1", "main"),
                    "--custom"
                }),
                startInfo.Arguments,
                "Should combine core and additional runner arguments with application implementation directory, command directory and main binary override");

            CheckEnvironment(startInfo, selections);
        }

        /// <summary>
        /// Ensures <see cref="Executor.GetStartInfo"/> handles complex <see cref="Selections"/> and <see cref="Executor.Main"/> with absolute paths.
        /// </summary>
        [Test]
        public void TestGetStartInfoMainAbsolute()
        {
            var selections = SelectionsTest.CreateTestSelections();
            selections.Implementations.InsertFirst(new ImplementationSelection {InterfaceID = "http://0install.de/feeds/test/dummy.xml"}); // Should be ignored by Executor

            PrepareStoreMock(selections, false);

            var executor = new Executor(selections, TestStore) {Main = "/main"};
            var startInfo = executor.GetStartInfo("--custom");
            Assert.AreEqual(
                Path.Combine(Test2Path, FileUtils.UnifySlashes(selections.Implementations[2].Commands[0].Path)),
                startInfo.FileName,
                "Should combine runner implementation directory with runner command path");
            Assert.AreEqual(
                StringUtils.ConcatenateEscapeArgument(new[]
                {
                    selections.Implementations[2].Commands[0].Arguments[0],
                    selections.Implementations[1].Commands[0].Runner.Arguments[0],
                    FileUtils.PathCombine(Test1Path, "main"),
                    "--custom"
                }),
                startInfo.Arguments,
                "Should combine core and additional runner arguments with application implementation directory and main binary override");

            CheckEnvironment(startInfo, selections);
        }

        /// <summary>
        /// Ensures <see cref="Executor.GetStartInfo"/> handles <see cref="Selections"/> with <see cref="Command.Path"/>s that are empty.
        /// </summary>
        [Test]
        public void TestGetStartInfoPathlessCommand()
        {
            var selections = SelectionsTest.CreateTestSelections();
            selections.Implementations.InsertFirst(new ImplementationSelection {InterfaceID = "http://0install.de/feeds/test/dummy.xml"}); // Should be ignored by Executor
            selections.Implementations[1].Commands[0].Path = null;

            PrepareStoreMock(selections, true);

            var executor = new Executor(selections, TestStore);
            var startInfo = executor.GetStartInfo("--custom");
            Assert.AreEqual(
                Path.Combine(Test2Path, FileUtils.UnifySlashes(selections.Implementations[2].Commands[0].Path)),
                startInfo.FileName);
            Assert.AreEqual(
                StringUtils.ConcatenateEscapeArgument(new[]
                {
                    selections.Implementations[2].Commands[0].Arguments[0],
                    selections.Implementations[1].Commands[0].Runner.Arguments[0],
                    selections.Implementations[1].Commands[0].Arguments[0],
                    "--custom"
                }),
                startInfo.Arguments);

            CheckEnvironment(startInfo, selections);
        }
    }
}
