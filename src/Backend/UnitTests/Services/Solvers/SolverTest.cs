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

using System.Collections.Generic;
using Moq;
using NanoByte.Common.Storage;
using NUnit.Framework;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Preferences;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Services.Solvers
{
    /// <summary>
    /// Contains common code for testing specific <see cref="ISolver"/> implementations.
    /// </summary>
    public abstract class SolverTest<T> : TestWithContainer<T> where T : class, ISolver
    {
        [Test]
        public void Minimal()
        {
            RunAndAssert(
                feeds: new Dictionary<string, string>
                {
                    {"http://test/app.xml", "<implementation version='1.0' id='app1'><command name='run' path='test-app' /></implementation>"}
                },
                requirements: new Requirements {InterfaceID = "http://test/app.xml", Command = Command.NameRun},
                expectedSelections: "<selection interface='http://test/app.xml' version='1.0' id='app1'><command name='run' path='test-app' /></selection>");
        }

        [Test]
        public void SimpleDependency()
        {
            RunAndAssert(
                feeds: new Dictionary<string, string>
                {
                    {"http://test/app.xml", "<implementation version='1.0' id='app1'><command name='run' path='test-app' /><requires interface='http://test/lib.xml' /></implementation>"},
                    {"http://test/lib.xml", "<implementation version='1.0' id='lib1' />"}
                },
                requirements: new Requirements {InterfaceID = "http://test/app.xml", Command = Command.NameRun},
                expectedSelections:
                    "<selection interface='http://test/app.xml' version='1.0' id='app1'><command name='run' path='test-app' /><requires interface='http://test/lib.xml' /></selection>" +
                    "<selection interface='http://test/lib.xml' version='1.0' id='lib1' />");
        }

        [Test]
        public void OptionalDependency()
        {
            // satisfiable
            RunAndAssert(
                feeds: new Dictionary<string, string>
                {
                    {"http://test/app.xml", "<implementation version='1.0' id='app1'><command name='run' path='test-app' /><requires interface='http://test/lib.xml' importance='recommended' /></implementation>"},
                    {"http://test/lib.xml", "<implementation version='1.0' id='lib1' />"}
                },
                requirements: new Requirements {InterfaceID = "http://test/app.xml", Command = Command.NameRun},
                expectedSelections:
                    "<selection interface='http://test/app.xml' version='1.0' id='app1'><command name='run' path='test-app' /><requires interface='http://test/lib.xml' importance='recommended' /></selection>" +
                    "<selection interface='http://test/lib.xml' version='1.0' id='lib1' />");

            // not satisfiable
            RunAndAssert(
                feeds: new Dictionary<string, string>
                {
                    {"http://test/app.xml", "<implementation version='1.0' id='app1'><command name='run' path='test-app' /><requires interface='http://test/lib.xml' importance='recommended' /></implementation>"},
                    {"http://test/lib.xml", ""}
                },
                requirements: new Requirements {InterfaceID = "http://test/app.xml", Command = Command.NameRun},
                expectedSelections:
                    "<selection interface='http://test/app.xml' version='1.0' id='app1'><command name='run' path='test-app' /></selection>");
        }

        [Test]
        public void OSSpecificDependency()
        {
            var feeds = new Dictionary<string, string>
            {
                {"http://test/app.xml", "<implementation version='1.0' id='app1'><command name='run' path='test-app' /><requires interface='http://test/lib.xml' os='Windows' /></implementation>"},
                {"http://test/lib.xml", "<implementation version='1.0' id='lib1' />"}
            };

            // applicable
            RunAndAssert(feeds,
                requirements: new Requirements {InterfaceID = "http://test/app.xml", Command = Command.NameRun, Architecture = new Architecture(OS.Windows, Cpu.All)},
                expectedSelections:
                    "<selection interface='http://test/app.xml' version='1.0' id='app1'><command name='run' path='test-app' /><requires interface='http://test/lib.xml' os='Windows' /></selection>" +
                    "<selection interface='http://test/lib.xml' version='1.0' id='lib1' />");

            // not applicable
            RunAndAssert(feeds,
                requirements: new Requirements {InterfaceID = "http://test/app.xml", Command = Command.NameRun, Architecture = new Architecture(OS.Linux, Cpu.All)},
                expectedSelections:
                    "<selection interface='http://test/app.xml' version='1.0' id='app1'><command name='run' path='test-app' /></selection>");
        }

        [Test]
        public void CyclicDependency()
        {
            RunAndAssert(
                feeds: new Dictionary<string, string>
                {
                    {"http://test/app.xml", "<implementation version='1.0' id='app1'><requires interface='http://test/lib.xml' /><command name='run' path='test-app' /></implementation>"},
                    {"http://test/lib.xml", "<implementation version='1.0' id='lib1'><requires interface='http://test/app.xml' /></implementation>"}
                },
                requirements: new Requirements {InterfaceID = "http://test/app.xml", Command = Command.NameRun},
                expectedSelections:
                    "<selection interface='http://test/app.xml' version='1.0' id='app1'><requires interface='http://test/lib.xml' /><command name='run' path='test-app' /></selection>" +
                    "<selection interface='http://test/lib.xml' version='1.0' id='lib1'><requires interface='http://test/app.xml' /></selection>");
        }

        [Test]
        public void RunnerDependency()
        {
            RunAndAssert(
                feeds: new Dictionary<string, string>
                {
                    {"http://test/app.xml", "<implementation version='1.0' id='app1'><command name='run' path='test-app'><runner interface='http://test/runner.xml' /></command></implementation>"},
                    {"http://test/runner.xml", "<implementation version='1.0' id='runner1'><command name='run' path='test-runner' /></implementation>"}
                },
                requirements: new Requirements {InterfaceID = "http://test/app.xml", Command = Command.NameRun},
                expectedSelections:
                    "<selection interface='http://test/app.xml' version='1.0' id='app1'><command name='run' path='test-app'><runner interface='http://test/runner.xml' /></command></selection>" +
                    "<selection interface='http://test/runner.xml' version='1.0' id='runner1'><command name='run' path='test-runner' /></selection>");
        }

        [Test]
        public void DependencyWithBinding()
        {
            RunAndAssert(
                feeds: new Dictionary<string, string>
                {
                    {"http://test/app.xml", "<implementation version='1.0' id='app1'><command name='run' path='test-app' /><requires interface='http://test/lib.xml'><environment name='var1' insert='.' /></requires></implementation>"},
                    {"http://test/lib.xml", "<implementation version='1.0' id='lib1' />"}
                },
                requirements: new Requirements {InterfaceID = "http://test/app.xml", Command = Command.NameRun},
                expectedSelections:
                    "<selection interface='http://test/app.xml' version='1.0' id='app1'><command name='run' path='test-app' /><requires interface='http://test/lib.xml'><environment name='var1' insert='.' /></requires></selection>" +
                    "<selection interface='http://test/lib.xml' version='1.0' id='lib1' />");
        }

        [Test]
        public void MultipleCommandDependencies()
        {
            RunAndAssert(
                feeds: new Dictionary<string, string>
                {
                    {"http://test/app.xml", "<implementation version='1.0' id='app1'><command name='run' path='test-app' /><requires interface='http://test/helper.xml'><executable-in-path name='helperA' command='commandA'/><executable-in-path name='helperB' command='commandB'/></requires></implementation>"},
                    {
                        "http://test/helper.xml",
                        "<implementation version='1.0' id='helper1'>" +
                        "  <command name='commandA' path='helperA' />" +
                        "  <command name='commandB' path='helperB'><runner interface='http://test/runner.xml' /></command>" +
                        "  <command name='commandC' path='helperC' />" +
                        "</implementation>"
                    },
                    {"http://test/runner.xml", "<implementation version='1.0' id='runner1'><command name='run' path='test-runner' /></implementation>"}
                },
                requirements: new Requirements {InterfaceID = "http://test/app.xml", Command = Command.NameRun},
                expectedSelections:
                    "<selection interface='http://test/app.xml' version='1.0' id='app1'><command name='run' path='test-app' /><requires interface='http://test/helper.xml'><executable-in-path name='helperA' command='commandA'/><executable-in-path name='helperB' command='commandB'/></requires></selection>" +
                    "<selection interface='http://test/helper.xml' version='1.0' id='helper1'><command name='commandA' path='helperA' /><command name='commandB' path='helperB'><runner interface='http://test/runner.xml' /></command></selection>" +
                    "<selection interface='http://test/runner.xml' version='1.0' id='runner1'><command name='run' path='test-runner' /></selection>");
        }

        [Test]
        public void ExecutableBindingInDependency()
        {
            RunAndAssert(
                feeds: new Dictionary<string, string>
                {
                    {"http://test/app.xml", "<implementation version='1.0' id='app1'><command name='run' path='test-app' /><requires interface='http://test/helper.xml'><executable-in-path/></requires></implementation>"},
                    {"http://test/helper.xml", "<implementation version='1.0' id='helper1'><command name='run' path='test-helper' /></implementation>"}
                },
                requirements: new Requirements {InterfaceID = "http://test/app.xml", Command = Command.NameRun},
                expectedSelections:
                    "<selection interface='http://test/app.xml' version='1.0' id='app1'><command name='run' path='test-app' /><requires interface='http://test/helper.xml'><executable-in-path/></requires></selection>" +
                    "<selection interface='http://test/helper.xml' version='1.0' id='helper1'><command name='run' path='test-helper' /></selection>");
        }

        [Test]
        public void ExecutableBindingInImplementationTriggeringAdditionalRequirements()
        {
            RunAndAssert(
                feeds: new Dictionary<string, string>
                {
                    {"http://test/app.xml", "<implementation version='1.0' id='app1'><command name='run' path='test-app' /><command name='helper' path='helper-app'><requires interface='http://test/helper.xml' /></command><executable-in-path command='helper'/></implementation>"},
                    {"http://test/helper.xml", "<implementation version='1.0' id='helper1'/>"}
                },
                requirements: new Requirements {InterfaceID = "http://test/app.xml", Command = Command.NameRun},
                expectedSelections:
                    "<selection interface='http://test/app.xml' version='1.0' id='app1'><command name='run' path='test-app' /><command name='helper' path='helper-app'><requires interface='http://test/helper.xml' /></command><executable-in-path command='helper'/></selection>" +
                    "<selection interface='http://test/helper.xml' version='1.0' id='helper1' />");
        }

        [Test]
        public void ExecutableBindingInDependencyTriggeringAdditionalRequirements()
        {
            RunAndAssert(
                feeds: new Dictionary<string, string>
                {
                    {"http://test/app.xml", "<implementation version='1.0' id='app1'><command name='run' path='test-app' /><requires interface='http://test/helper.xml'><executable-in-path command='helper'/></requires></implementation>"},
                    {"http://test/helper.xml", "<implementation version='1.0' id='helper1'><command name='helper' path='helper-app' /></implementation>"}
                },
                requirements: new Requirements {InterfaceID = "http://test/app.xml", Command = Command.NameRun},
                expectedSelections:
                    "<selection interface='http://test/app.xml' version='1.0' id='app1'><command name='run' path='test-app' /><requires interface='http://test/helper.xml'><executable-in-path command='helper'/></requires></selection>" +
                    "<selection interface='http://test/helper.xml' version='1.0' id='helper1'><command name='helper' path='helper-app' /></selection>");
        }

        [Test]
        public void SimpleRestriction()
        {
            // without restriction
            RunAndAssert(
                feeds: new Dictionary<string, string>
                {
                    {"http://test/app.xml", "<implementation version='1.0' id='app1'><command name='run' path='test-app' /><requires interface='http://test/liba.xml' /><requires interface='http://test/libb.xml' /></implementation>"},
                    {"http://test/liba.xml", "<implementation version='1.0' id='liba1' />"},
                    {"http://test/libb.xml", "<implementation version='1.0' id='libb1' /><implementation version='2.0' id='libb2' />"}
                },
                requirements: new Requirements {InterfaceID = "http://test/app.xml", Command = Command.NameRun},
                expectedSelections:
                    "<selection interface='http://test/app.xml' version='1.0' id='app1'><command name='run' path='test-app' /><requires interface='http://test/liba.xml' /><requires interface='http://test/libb.xml' /></selection>" +
                    "<selection interface='http://test/liba.xml' version='1.0' id='liba1' />" +
                    "<selection interface='http://test/libb.xml' version='2.0' id='libb2' />");

            // with restriction
            RunAndAssert(
                feeds: new Dictionary<string, string>
                {
                    {"http://test/app.xml", "<implementation version='1.0' id='app1'><command name='run' path='test-app' /><requires interface='http://test/liba.xml' /><requires interface='http://test/libb.xml' /></implementation>"},
                    {"http://test/liba.xml", "<implementation version='1.0' id='liba1'><restricts interface='http://test/libb.xml' version='1.0' /></implementation>"},
                    {"http://test/libb.xml", "<implementation version='1.0' id='libb1' /><implementation version='2.0' id='libb2' />"}
                },
                requirements: new Requirements {InterfaceID = "http://test/app.xml", Command = Command.NameRun},
                expectedSelections:
                    "<selection interface='http://test/app.xml' version='1.0' id='app1'><command name='run' path='test-app' /><requires interface='http://test/liba.xml' /><requires interface='http://test/libb.xml' /></selection>" +
                    "<selection interface='http://test/liba.xml' version='1.0' id='liba1' />" +
                    "<selection interface='http://test/libb.xml' version='1.0' id='libb1' />");
        }

        [Test]
        public void OSSpecificRestriction()
        {
            var feeds = new Dictionary<string, string>
            {
                {"http://test/app.xml", "<implementation version='1.0' id='app1'><command name='run' path='test-app' /><requires interface='http://test/liba.xml' /><requires interface='http://test/libb.xml' /></implementation>"},
                {"http://test/liba.xml", "<implementation version='1.0' id='liba1'><restricts interface='http://test/libb.xml' version='1.0' os='Windows' /></implementation>"},
                {"http://test/libb.xml", "<implementation version='1.0' id='libb1' /><implementation version='2.0' id='libb2' />"}
            };

            // applicable
            RunAndAssert(feeds,
                requirements: new Requirements {InterfaceID = "http://test/app.xml", Command = Command.NameRun, Architecture = new Architecture(OS.Windows, Cpu.All)},
                expectedSelections:
                    "<selection interface='http://test/app.xml' version='1.0' id='app1'><command name='run' path='test-app' /><requires interface='http://test/liba.xml' /><requires interface='http://test/libb.xml' /></selection>" +
                    "<selection interface='http://test/liba.xml' version='1.0' id='liba1' />" +
                    "<selection interface='http://test/libb.xml' version='1.0' id='libb1' />");

            // not applicable
            RunAndAssert(feeds,
                requirements: new Requirements {InterfaceID = "http://test/app.xml", Command = Command.NameRun, Architecture = new Architecture(OS.Linux, Cpu.All)},
                expectedSelections:
                    "<selection interface='http://test/app.xml' version='1.0' id='app1'><command name='run' path='test-app' /><requires interface='http://test/liba.xml' /><requires interface='http://test/libb.xml' /></selection>" +
                    "<selection interface='http://test/liba.xml' version='1.0' id='liba1' />" +
                    "<selection interface='http://test/libb.xml' version='2.0' id='libb2' />");
        }

        [Test]
        public void ExtraRestrictions()
        {
            RunAndAssert(
                feeds: new Dictionary<string, string>
                {
                    {"http://test/app.xml", "<implementation version='1.0' id='app1'><command name='run' path='test-app' /></implementation><implementation version='2.0' id='app2'><command name='run' path='test-app' /></implementation>"}
                },
                requirements: new Requirements
                {
                    InterfaceID = "http://test/app.xml", Command = Command.NameRun,
                    ExtraRestrictions = {{"http://test/app.xml", new VersionRange("..!2.0")}}
                },
                expectedSelections: "<selection interface='http://test/app.xml' version='1.0' id='app1'><command name='run' path='test-app' /></selection>");
        }

        [Test]
        public void SimpleFeedReference()
        {
            RunAndAssert(
                feeds: new Dictionary<string, string>
                {
                    {"http://test/app1.xml", "<implementation version='1.0' id='app1'><command name='run' path='test-app1' /></implementation><feed src='http://test/app2.xml' />"},
                    {"http://test/app2.xml", "<implementation version='2.0' id='app2'><command name='run' path='test-app2' /></implementation>"}
                },
                requirements: new Requirements {InterfaceID = "http://test/app1.xml", Command = Command.NameRun},
                expectedSelections: "<selection interface='http://test/app1.xml' from-feed='http://test/app2.xml' version='2.0' id='app2'><command name='run' path='test-app2' /></selection>");
        }

        [Test]
        public void CyclicFeedReference()
        {
            RunAndAssert(
                feeds: new Dictionary<string, string>
                {
                    {"http://test/app1.xml", "<implementation version='1.0' id='app1'><command name='run' path='test-app1' /></implementation><feed src='http://test/app2.xml' />"},
                    {"http://test/app2.xml", "<implementation version='2.0' id='app2'><command name='run' path='test-app2' /></implementation><feed src='http://test/app1.xml' />"}
                },
                requirements: new Requirements {InterfaceID = "http://test/app1.xml", Command = Command.NameRun},
                expectedSelections: "<selection interface='http://test/app1.xml' from-feed='http://test/app2.xml' version='2.0' id='app2'><command name='run' path='test-app2' /></selection>");
        }

        [Test]
        public void CustomFeedReference()
        {
            new InterfacePreferences {Feeds = {new FeedReference {Source = "http://test/app2.xml"}}}.SaveFor("http://test/app1.xml");
            RunAndAssert(
                feeds: new Dictionary<string, string>
                {
                    {"http://test/app1.xml", "<implementation version='1.0' id='app1'><command name='run' path='test-app1' /></implementation>"},
                    {"http://test/app2.xml", "<implementation version='2.0' id='app2'><command name='run' path='test-app2' /></implementation>"}
                },
                requirements: new Requirements {InterfaceID = "http://test/app1.xml", Command = Command.NameRun},
                expectedSelections: "<selection interface='http://test/app1.xml' from-feed='http://test/app2.xml' version='2.0' id='app2'><command name='run' path='test-app2' /></selection>");
        }

        [Test]
        public void X86OnX64()
        {
            if (Architecture.CurrentSystem.Cpu != Cpu.X64) Assert.Ignore("Can only test on X64 systems");

            // Prefer x64 when possible
            RunAndAssert(
                feeds: new Dictionary<string, string>
                {
                    {
                        "http://test/app.xml",
                        "<group version='1.0'><command name='run' path='test-app' /><requires interface='http://test/lib.xml' />" +
                        "<implementation arch='*-i686' id='app32'/><implementation arch='*-x86_64' id='app64'/>" +
                        "</group>"
                    },
                    {"http://test/lib.xml", "<implementation version='1.0' id='lib' />"}
                },
                requirements: new Requirements {InterfaceID = "http://test/app.xml", Command = Command.NameRun},
                expectedSelections:
                    "<selection interface='http://test/app.xml' version='1.0' arch='*-x86_64' id='app64'><command name='run' path='test-app' /><requires interface='http://test/lib.xml' /></selection>" +
                    "<selection interface='http://test/lib.xml' version='1.0' id='lib' />");

            // Fall back to x86 to avoid 32bit/64bit mixing
            RunAndAssert(
                feeds: new Dictionary<string, string>
                {
                    {
                        "http://test/app.xml",
                        "<group version='1.0'><command name='run' path='test-app' /><requires interface='http://test/lib.xml' />" +
                        "<implementation arch='*-i686' id='app32'/><implementation arch='*-x86_64' id='app64'/>" +
                        "</group>"
                    },
                    {"http://test/lib.xml", "<implementation version='1.0' arch='*-i486' id='lib' />"}
                },
                requirements: new Requirements {InterfaceID = "http://test/app.xml", Command = Command.NameRun},
                expectedSelections:
                    "<selection interface='http://test/app.xml' version='1.0' arch='*-i686' id='app32'><command name='run' path='test-app' /><requires interface='http://test/lib.xml' /></selection>" +
                    "<selection interface='http://test/lib.xml' version='1.0' arch='*-i486' id='lib' />");
        }

        #region Helpers
        protected void RunAndAssert(IEnumerable<KeyValuePair<string, string>> feeds, Requirements requirements, string expectedSelections)
        {
            var feedManagerMock = Container.GetMock<IFeedManager>();
            var parsedFeeds = ParseFeeds(feeds);
            feedManagerMock.Setup(x => x.GetFeed(It.IsAny<string>())).Returns((string feedID) => parsedFeeds[feedID]);

            var expected = ParseExpectedSelections(expectedSelections, requirements);
            var actual = Target.Solve(requirements);
            Assert.AreEqual(expected, actual,
                message: string.Format("Selections mismatch.\nExpected: {0}\nActual: {1}", expected.ToXmlString(), actual.ToXmlString()));
        }

        private static Selections ParseExpectedSelections(string expectedSelections, Requirements requirements)
        {
            var expectedSelectionsParsed = XmlStorage.FromXmlString<Selections>(string.Format(
                "<?xml version='1.0'?><selections interface='{0}' command='{1}' xmlns='http://zero-install.sourceforge.net/2004/injector/interface'>{2}</selections>",
                requirements.InterfaceID, requirements.Command, expectedSelections));
            return expectedSelectionsParsed;
        }

        private static IDictionary<string, Feed> ParseFeeds(IEnumerable<KeyValuePair<string, string>> feeds)
        {
            var feedsParsed = new Dictionary<string, Feed>();
            foreach (var feedXml in feeds)
            {
                var feed = XmlStorage.FromXmlString<Feed>(string.Format(
                    "<?xml version='1.0'?><interface xmlns='http://zero-install.sourceforge.net/2004/injector/interface' uri='{0}'>{1}</interface>",
                    feedXml.Key, feedXml.Value));
                feed.Normalize(feedXml.Key);
                feedsParsed.Add(feedXml.Key, feed);
            }
            return feedsParsed;
        }
        #endregion
    }
}
