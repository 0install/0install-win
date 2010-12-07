using System;
using NUnit.Framework;
using ZeroInstall.Injector.Arguments;
using ZeroInstall.Model;
using ZeroInstall.Store.Feed;

namespace ZeroInstall.Injector.Cli
{
    /// <summary>
    /// Contains test methods for <see cref="Program.ParseArgs"/>.
    /// </summary>
    [TestFixture]
    public class ProgramTest
    {
        [Test]
        public void TestNormal()
        {
            var testArgs = new[] {"--not-before=2.0", "--cpu=x86_64", "--refresh", "--os=Windows", "--before=3.0", "-w", "gdb", "http://mozilla.org/firefox.xml", "-url", "http://google.com"};
            
            ParseResults results;
            Assert.AreEqual(OperationMode.Normal, Program.ParseArgs(testArgs, new SilentHandler(), out results));
            Assert.AreEqual(new ImplementationVersion("2.0"), results.Policy.Constraint.NotBeforeVersion);
            Assert.AreEqual(new ImplementationVersion("3.0"), results.Policy.Constraint.BeforeVersion);
            Assert.AreEqual(OS.Windows, results.Policy.Architecture.OS);
            Assert.AreEqual(Cpu.X64, results.Policy.Architecture.Cpu);
            Assert.IsTrue(results.Policy.FeedProvider.Refresh);
            Assert.IsFalse(results.DownloadOnly);
            Assert.IsFalse(results.DryRun);
            Assert.IsFalse(results.GetSelections);
            Assert.IsFalse(results.SelectOnly);
            Assert.IsNull(results.SelectionsFile);
            Assert.IsNull(results.Main);
            Assert.AreEqual("gdb", results.Wrapper);
            Assert.AreEqual("http://mozilla.org/firefox.xml", results.Feed);
            CollectionAssert.AreEqual(new[] {"-url", "http://google.com"}, results.AdditionalArgs);
        }

        [Test]
        public void TestSetSelections()
        {
            var testArgs = new[] {"--offline", "--source", "-D", "--main=app", "--set-selections=/local/selections.xml", "/local/feed.xml"};

            ParseResults results;
            Assert.AreEqual(OperationMode.Normal, Program.ParseArgs(testArgs, new SilentHandler(), out results));
            Assert.AreEqual(NetworkLevel.Offline, results.Policy.FeedProvider.NetworkLevel);
            Assert.AreEqual(Cpu.Source, results.Policy.Architecture.Cpu);
            Assert.IsFalse(results.DownloadOnly);
            Assert.IsTrue(results.DryRun);
            Assert.IsFalse(results.GetSelections);
            Assert.IsFalse(results.SelectOnly);
            Assert.AreEqual("/local/selections.xml", results.SelectionsFile);
            Assert.AreEqual("app", results.Main);
            Assert.IsNull(results.Wrapper);
            Assert.AreEqual("/local/feed.xml", results.Feed);
            CollectionAssert.AreEqual(new string[0], results.AdditionalArgs);
        }

        [Test]
        public void TestGetSelections()
        {
            var testArgs = new[] {"-d", "--get-selections", "--select-only", "/local/feed.xml"};

            ParseResults results;
            Assert.AreEqual(OperationMode.Normal, Program.ParseArgs(testArgs, new SilentHandler(), out results));
            Assert.IsTrue(results.DownloadOnly);
            Assert.IsFalse(results.DryRun);
            Assert.IsTrue(results.GetSelections);
            Assert.IsTrue(results.SelectOnly);
            Assert.IsNull(results.SelectionsFile);
            Assert.IsNull(results.Main);
            Assert.IsNull(results.Wrapper);
            Assert.AreEqual("/local/feed.xml", results.Feed);
            CollectionAssert.AreEqual(new string[0], results.AdditionalArgs);
        }

        [Test]
        public void TestListMode()
        {
            var testArgs = new[] {"--list", "some", "search", "terms"};

            ParseResults results;
            Assert.AreEqual(OperationMode.List, Program.ParseArgs(testArgs, new SilentHandler(), out results));
        }
        
        [Test]
        public void TestManageMode()
        {
            var testArgs = new[] {"--feed", "http://signed/feed.xml", "http://another/signed/feed.xml"};

            ParseResults results;
            Assert.AreEqual(OperationMode.Manage, Program.ParseArgs(testArgs, new SilentHandler(), out results));
        }

        [Test]
        public void TestVersionMode()
        {
            var testArgs = new[] {"--version"};

            ParseResults results;
            Assert.AreEqual(OperationMode.Version, Program.ParseArgs(testArgs, new SilentHandler(), out results));
        }

        [Test]
        public void TestException()
        {
            var testArgs = new[] { "--refresh", "--invalid-argument", "--offline", "/local/feed.xml" };

            ParseResults results;
            Assert.Throws<ArgumentException>(() => Program.ParseArgs(testArgs, new SilentHandler(), out results));
        }
    }
}
