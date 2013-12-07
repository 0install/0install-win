/*
 * Copyright 2010-2013 Bastian Eicher
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
using Common.Storage;
using Common.Tasks;
using Moq;
using NUnit.Framework;
using ZeroInstall.Injector;
using ZeroInstall.Model;
using ZeroInstall.Model.Selection;
using ZeroInstall.Store;

namespace ZeroInstall.Solvers
{
    /// <summary>
    /// Contains common code for testing specific <see cref="ISolver"/> implementations.
    /// </summary>
    public abstract class SolverTest<T> : TestWithResolver<T> where T : class, ISolver
    {
        [Test]
        public void TestNoDependency()
        {
            RunAndAssert(
                feeds: new Dictionary<string, string>
                {
                    {"http://test/app.xml", "<implementation version='1.0' id='app1'><command name='run' path='test-app' /></implementation>"}
                },
                requirements: new Requirements {InterfaceID = "http://test/app.xml"},
                expectedSelections: "<selection interface='http://test/app.xml' version='1.0' id='app1'><command name='run' path='test-app' /></selection>");
        }

        [Test]
        public void TestSimpleDependency()
        {
            RunAndAssert(
                feeds: new Dictionary<string, string>
                {
                    {"http://test/app.xml", "<implementation version='1.0' id='app1'><command name='run' path='test-app' /><requires interface='http://test/lib.xml' /></implementation>"},
                    {"http://test/lib.xml", "<implementation version='1.0' id='lib1' />"}
                },
                requirements: new Requirements {InterfaceID = "http://test/app.xml"},
                expectedSelections:
                    "<selection interface='http://test/app.xml' version='1.0' id='app1'><command name='run' path='test-app' /><requires interface='http://test/lib.xml' /></selection>" +
                    "<selection interface='http://test/lib.xml' version='1.0' id='lib1' />");
        }

        [Test]
        public void TestCyclicDependency()
        {
            RunAndAssert(
                feeds: new Dictionary<string, string>
                {
                    {"http://test/app.xml", "<implementation version='1.0' id='app1'><requires interface='http://test/lib.xml' /><command name='run' path='test-app' /></implementation>"},
                    {"http://test/lib.xml", "<implementation version='1.0' id='lib1'><requires interface='http://test/app.xml' /></implementation>"}
                },
                requirements: new Requirements {InterfaceID = "http://test/app.xml"},
                expectedSelections:
                    "<selection interface='http://test/app.xml' version='1.0' id='app1'><requires interface='http://test/lib.xml' /><command name='run' path='test-app' /></selection>" +
                    "<selection interface='http://test/lib.xml' version='1.0' id='lib1'><requires interface='http://test/app.xml' /></selection>");
        }

        [Test]
        public void TestRestriction()
        {
            // without restriction
            RunAndAssert(
                feeds: new Dictionary<string, string>
                {
                    {"http://test/app.xml", "<implementation version='1.0' id='app1'><command name='run' path='test-app' /><requires interface='http://test/liba.xml' /><requires interface='http://test/libb.xml' /></implementation>"},
                    {"http://test/liba.xml", "<implementation version='1.0' id='liba1' />"},
                    {"http://test/libb.xml", "<implementation version='1.0' id='libb1' /><implementation version='2.0' id='libb2' />"}
                },
                requirements: new Requirements {InterfaceID = "http://test/app.xml"},
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
                requirements: new Requirements {InterfaceID = "http://test/app.xml"},
                expectedSelections:
                    "<selection interface='http://test/app.xml' version='1.0' id='app1'><command name='run' path='test-app' /><requires interface='http://test/liba.xml' /><requires interface='http://test/libb.xml' /></selection>" +
                    "<selection interface='http://test/liba.xml' version='1.0' id='liba1' />" +
                    "<selection interface='http://test/libb.xml' version='1.0' id='libb1' />");
        }

        #region Helpers
        protected void RunAndAssert(IEnumerable<KeyValuePair<string, string>> feeds, Requirements requirements, string expectedSelections)
        {
            var handlerMock = Resolver.GetMock<IHandler>();
            handlerMock.SetupGet(x => x.CancellationToken).Returns(new CancellationToken());

            var feedManagerMock = Resolver.GetMock<IFeedManager>();
            var parsedFeeds = ParseFeeds(feeds);
            feedManagerMock.Setup(x => x.GetFeed(It.IsAny<string>())).Returns((string feedID) => parsedFeeds[feedID]);

            Assert.AreEqual(
                expected: ParseExpectedSelections(expectedSelections, requirements),
                actual: Target.Solve(requirements));
        }

        private static Selections ParseExpectedSelections(string expectedSelections, Requirements requirements)
        {
            var expectedSelectionsParsed = XmlStorage.FromXmlString<Selections>(string.Format(
                "<?xml version='1.0'?><selections interface='{0}' command='{1}' xmlns='http://zero-install.sourceforge.net/2004/injector/interface'>{2}</selections>",
                requirements.InterfaceID, requirements.EffectiveCommand, expectedSelections));
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
