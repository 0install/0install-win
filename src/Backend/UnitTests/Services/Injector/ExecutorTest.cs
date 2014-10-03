/*
 * Copyright 2010-2014 Bastian Eicher
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
using System.Linq;
using Moq;
using NanoByte.Common;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NUnit.Framework;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Services.Injector
{
    /// <summary>
    /// Contains test methods for <see cref="Executor"/>.
    /// </summary>
    [TestFixture]
    public class ExecutorTest
    {
        private const string Test1Path = "test1 path", Test2Path = "test2 path";

        private LocationsRedirect _redirect;

        [SetUp]
        public void SetUp()
        {
            // Don't store generated executables settings in real user profile
            _redirect = new LocationsRedirect("0install-unit-tests");
        }

        [TearDown]
        public void TearDown()
        {
            _redirect.Dispose();
        }

        [Test]
        public void TestExceptionEmpty()
        {
            Assert.Throws<ArgumentException>(() => new Executor(new Mock<IStore>(MockBehavior.Loose).Object).Start(new Selections()), "Empty selections should be rejected");
        }

        [Test]
        public void TestExceptionMultipleInvalidBindings()
        {
            var selections = SelectionsTest.CreateTestSelections();
            selections.Implementations[1].Commands[0].Bindings.Add(new EnvironmentBinding()); // Missing name
            ExpectCommandException(selections);

            selections = SelectionsTest.CreateTestSelections();
            selections.Implementations[1].Commands[0].Bindings.Add(new EnvironmentBinding {Name = "test", Insert = "test1", Value = "test2"}); // Conflicting insert and value
            ExpectCommandException(selections);

            selections = SelectionsTest.CreateTestSelections();
            selections.Implementations[1].Commands[0].Bindings.Add(new ExecutableInVar()); // Missing name
            ExpectCommandException(selections);

            selections = SelectionsTest.CreateTestSelections();
            selections.Implementations[1].Commands[0].Bindings.Add(new ExecutableInPath()); // Missing name
            ExpectCommandException(selections);
        }

        [Test]
        public void TestExceptionMultipleWorkingDirs()
        {
            var selections = SelectionsTest.CreateTestSelections();
            selections.Implementations[1].Commands[0].WorkingDir = new WorkingDir();
            ExpectCommandException(selections);
        }

        private static void ExpectCommandException(Selections selections)
        {
            var storeMock = new Mock<IStore>(MockBehavior.Loose);
            storeMock.Setup(x => x.GetPath(It.IsAny<ManifestDigest>())).Returns("test path");
            var executor = new Executor(storeMock.Object);
            Assert.Throws<ExecutorException>(() => executor.GetStartInfo(selections), "Invalid Selections should be rejected");
        }

        private static IStore GetMockStore(Selections selections)
        {
            var storeMock = new Mock<IStore>(MockBehavior.Loose);
            storeMock.Setup(x => x.GetPath(selections.Implementations[1].ManifestDigest)).Returns(Test1Path);
            storeMock.Setup(x => x.GetPath(selections.Implementations[2].ManifestDigest)).Returns(Test2Path);
            return storeMock.Object;
        }

        private static void VerifyEnvironment(ProcessStartInfo startInfo, Selections selections)
        {
            Assert.AreEqual("default" + Path.PathSeparator + Test1Path, startInfo.EnvironmentVariables["TEST1_PATH_SELF"], "Should append implementation path");
            Assert.AreEqual("test1", startInfo.EnvironmentVariables["TEST1_VALUE"], "Should directly set value");
            Assert.AreEqual("", startInfo.EnvironmentVariables["TEST1_EMPTY"], "Should set empty environment variables");
            Assert.AreEqual(Test2Path + Path.PathSeparator + "default", startInfo.EnvironmentVariables["TEST2_PATH_SELF"], "Should prepend implementation path");
            Assert.AreEqual("test2", startInfo.EnvironmentVariables["TEST2_VALUE"], "Should directly set value");
            Assert.AreEqual("default" + Path.PathSeparator + Path.Combine(Test2Path, "sub"), startInfo.EnvironmentVariables["TEST2_PATH_SUB_DEP"], "Should append implementation sub-path");
            Assert.AreEqual(Test1Path, startInfo.EnvironmentVariables["TEST1_PATH_COMMAND"], "Should set implementation path");
            Assert.AreEqual(Test1Path + Path.PathSeparator + Test1Path, startInfo.EnvironmentVariables["TEST1_PATH_COMMAND_DEP"], "Should set implementation path for command dependency for each reference");
            Assert.AreEqual(Path.Combine(Test1Path, "bin"), startInfo.WorkingDirectory, "Should set implementation path");

            string execFile = Path.Combine(Test2Path, FileUtils.UnifySlashes(selections.Implementations[2].Commands[0].Path));
            string execArgs = new[]
            {
                selections.Implementations[2].Commands[0].Arguments[0].ToString(),
                selections.Implementations[1].Commands[1].Runner.Arguments[0].ToString(),
                Path.Combine(Test1Path, FileUtils.UnifySlashes(selections.Implementations[1].Commands[1].Path)),
                selections.Implementations[1].Commands[1].Arguments[0].ToString()
            }.JoinEscapeArguments();
            Assert.AreEqual(execFile, startInfo.EnvironmentVariables["ZEROINSTALL_RUNENV_FILE_exec-in-var"]);
            Assert.AreEqual(execArgs, startInfo.EnvironmentVariables["ZEROINSTALL_RUNENV_ARGS_exec-in-var"]);
            Assert.AreEqual(execFile, startInfo.EnvironmentVariables["ZEROINSTALL_RUNENV_FILE_exec-in-path"]);
            Assert.AreEqual(execArgs, startInfo.EnvironmentVariables["ZEROINSTALL_RUNENV_ARGS_exec-in-path"]);
            Assert.AreEqual(Locations.GetCacheDirPath("0install.net", false, "injector", "executables", "exec-in-path") + Path.PathSeparator + new ProcessStartInfo().EnvironmentVariables["PATH"], startInfo.EnvironmentVariables["PATH"]);
        }

        /// <summary>
        /// Ensures <see cref="Executor.GetStartInfo"/> handles complex <see cref="Selections"/>.
        /// </summary>
        [Test]
        public void TestGetStartInfo()
        {
            var selections = SelectionsTest.CreateTestSelections();
            selections.Implementations.Insert(0, new ImplementationSelection {InterfaceID = "http://0install.de/feeds/test/dummy.xml"}); // Should be ignored by Executor

            var executor = new Executor(GetMockStore(selections));
            var startInfo = executor.GetStartInfo(selections, "--custom");
            Assert.AreEqual(
                Path.Combine(Test2Path, FileUtils.UnifySlashes(selections.Implementations[2].Commands[0].Path)),
                startInfo.FileName,
                "Should combine runner implementation directory with runner command path");
            Assert.AreEqual(
                new[]
                {
                    selections.Implementations[2].Commands[0].Arguments[0].ToString(),
                    selections.Implementations[1].Commands[0].Runner.Arguments[0].ToString(),
                    Path.Combine(Test1Path, FileUtils.UnifySlashes(selections.Implementations[1].Commands[0].Path)),
                    selections.Implementations[1].Commands[0].Arguments[0].ToString(),
                    "--custom"
                }.JoinEscapeArguments(),
                startInfo.Arguments,
                "Should combine core and additional runner arguments with application implementation directory, command path and arguments");

            VerifyEnvironment(startInfo, selections);
        }

        /// <summary>
        /// Ensures <see cref="Executor.GetStartInfo"/> handles complex <see cref="Selections"/> and <see cref="Executor.Wrapper"/>.
        /// </summary>
        [Test]
        public void TestGetStartInfoWrapper()
        {
            if (!WindowsUtils.IsWindows) Assert.Ignore("Wrapper command-line parsing relies on a Win32 API and therefore will not work on non-Windows platforms");

            var selections = SelectionsTest.CreateTestSelections();
            selections.Implementations.Insert(0, new ImplementationSelection {InterfaceID = "http://0install.de/feeds/test/dummy.xml"}); // Should be ignored by Executor

            var executor = new Executor(GetMockStore(selections)) {Wrapper = "wrapper --wrapper"};
            var startInfo = executor.GetStartInfo(selections, "--custom");
            Assert.AreEqual("wrapper", startInfo.FileName);
            Assert.AreEqual(
                new[]
                {
                    "--wrapper",
                    Path.Combine(Test2Path, FileUtils.UnifySlashes(selections.Implementations[2].Commands[0].Path)),
                    selections.Implementations[2].Commands[0].Arguments[0].ToString(),
                    selections.Implementations[1].Commands[0].Runner.Arguments[0].ToString(),
                    Path.Combine(Test1Path, FileUtils.UnifySlashes(selections.Implementations[1].Commands[0].Path)),
                    selections.Implementations[1].Commands[0].Arguments[0].ToString(),
                    "--custom"
                }.JoinEscapeArguments(),
                startInfo.Arguments,
                "Should combine wrapper arguments, runner and application");

            VerifyEnvironment(startInfo, selections);
        }

        /// <summary>
        /// Ensures <see cref="Executor.GetStartInfo"/> handles complex <see cref="Selections"/> and <see cref="Executor.Main"/> with relative paths.
        /// </summary>
        [Test]
        public void TestGetStartInfoMainRelative()
        {
            var selections = SelectionsTest.CreateTestSelections();
            selections.Implementations.Insert(0, new ImplementationSelection {InterfaceID = "http://0install.de/feeds/test/dummy.xml"}); // Should be ignored by Executor

            var executor = new Executor(GetMockStore(selections)) {Main = "main"};
            var startInfo = executor.GetStartInfo(selections, "--custom");
            Assert.AreEqual(
                Path.Combine(Test2Path, FileUtils.UnifySlashes(selections.Implementations[2].Commands[0].Path)),
                startInfo.FileName,
                "Should combine runner implementation directory with runner command path");
            Assert.AreEqual(
                new[]
                {
                    selections.Implementations[2].Commands[0].Arguments[0].ToString(),
                    selections.Implementations[1].Commands[0].Runner.Arguments[0].ToString(),
                    new[] {Test1Path, "dir 1", "main"}.Aggregate(Path.Combine),
                    "--custom"
                }.JoinEscapeArguments(),
                startInfo.Arguments,
                "Should combine core and additional runner arguments with application implementation directory, command directory and main binary override");

            VerifyEnvironment(startInfo, selections);
        }

        /// <summary>
        /// Ensures <see cref="Executor.GetStartInfo"/> handles complex <see cref="Selections"/> and <see cref="Executor.Main"/> with absolute paths.
        /// </summary>
        [Test]
        public void TestGetStartInfoMainAbsolute()
        {
            var selections = SelectionsTest.CreateTestSelections();
            selections.Implementations.Insert(0, new ImplementationSelection {InterfaceID = "http://0install.de/feeds/test/dummy.xml"}); // Should be ignored by Executor

            var executor = new Executor(GetMockStore(selections)) {Main = "/main"};
            var startInfo = executor.GetStartInfo(selections, "--custom");
            Assert.AreEqual(
                Path.Combine(Test2Path, FileUtils.UnifySlashes(selections.Implementations[2].Commands[0].Path)),
                startInfo.FileName,
                "Should combine runner implementation directory with runner command path");
            Assert.AreEqual(
                new[]
                {
                    selections.Implementations[2].Commands[0].Arguments[0].ToString(),
                    selections.Implementations[1].Commands[0].Runner.Arguments[0].ToString(),
                    Path.Combine(Test1Path, "main"),
                    "--custom"
                }.JoinEscapeArguments(),
                startInfo.Arguments,
                "Should combine core and additional runner arguments with application implementation directory and main binary override");

            VerifyEnvironment(startInfo, selections);
        }

        /// <summary>
        /// Ensures <see cref="Executor.GetStartInfo"/> handles <see cref="Selections"/> with <see cref="Command.Path"/>s that are empty.
        /// </summary>
        [Test]
        public void TestGetStartInfoPathlessCommand()
        {
            var selections = SelectionsTest.CreateTestSelections();
            selections.Implementations.Insert(0, new ImplementationSelection {InterfaceID = "http://0install.de/feeds/test/dummy.xml"}); // Should be ignored by Executor
            selections.Implementations[1].Commands[0].Path = null;

            var executor = new Executor(GetMockStore(selections));
            var startInfo = executor.GetStartInfo(selections, "--custom");
            Assert.AreEqual(
                Path.Combine(Test2Path, FileUtils.UnifySlashes(selections.Implementations[2].Commands[0].Path)),
                startInfo.FileName);
            Assert.AreEqual(
                new[]
                {
                    selections.Implementations[2].Commands[0].Arguments[0].ToString(),
                    selections.Implementations[1].Commands[0].Runner.Arguments[0].ToString(),
                    selections.Implementations[1].Commands[0].Arguments[0].ToString(),
                    "--custom"
                }.JoinEscapeArguments(),
                startInfo.Arguments);

            VerifyEnvironment(startInfo, selections);
        }

        /// <summary>
        /// Ensures <see cref="Executor.GetStartInfo"/> handles <see cref="Selections"/> with <see cref="ForEachArgs"/>.
        /// </summary>
        [Test]
        public void TestGetStartInfoForEachArgs()
        {
            var selections = SelectionsTest.CreateTestSelections();
            selections.Implementations.Insert(0, new ImplementationSelection {InterfaceID = "http://0install.de/feeds/test/dummy.xml"}); // Should be ignored by Executor

            selections.Implementations[1].Commands[0].Arguments.Add(new ForEachArgs
            {
                ItemFrom = "SPLIT_ARG",
                Arguments = {"pre1 $item post1", "pre2 $item post2"}
            });
            selections.Implementations[2].Bindings.Add(new EnvironmentBinding {Name = "SPLIT_ARG", Value = "split1" + Path.PathSeparator + "split2"});

            var executor = new Executor(GetMockStore(selections));
            var startInfo = executor.GetStartInfo(selections);
            Assert.AreEqual(
                Path.Combine(Test2Path, FileUtils.UnifySlashes(selections.Implementations[2].Commands[0].Path)),
                startInfo.FileName,
                "Should combine runner implementation directory with runner command path");
            Assert.AreEqual(
                new[]
                {
                    selections.Implementations[2].Commands[0].Arguments[0].ToString(),
                    selections.Implementations[1].Commands[0].Runner.Arguments[0].ToString(),
                    Path.Combine(Test1Path, FileUtils.UnifySlashes(selections.Implementations[1].Commands[0].Path)),
                    selections.Implementations[1].Commands[0].Arguments[0].ToString(),
                    "pre1 split1 post1", "pre2 split1 post2", "pre1 split2 post1", "pre2 split2 post2"
                }.JoinEscapeArguments(),
                startInfo.Arguments,
                "Should combine core and additional runner arguments with application implementation directory and command path");

            VerifyEnvironment(startInfo, selections);
        }
    }
}
