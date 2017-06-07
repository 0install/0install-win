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
using FluentAssertions;
using Moq;
using NanoByte.Common.Storage;
using Xunit;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Contains integration tests for <see cref="Update"/>.
    /// </summary>
    public class UpdateTest : SelectionTestBase<Update>
    {
        [Fact] // Ensures local Selections XMLs are correctly detected and parsed.
        public void TestNormal()
        {
            var requirements = RequirementsTest.CreateTestRequirements();
            var selectionsOld = SelectionsTest.CreateTestSelections();
            var selectionsNew = SelectionsTest.CreateTestSelections();
            selectionsNew.Implementations[1].Version = new ImplementationVersion("2.0");
            selectionsNew.Implementations.Add(new ImplementationSelection {InterfaceUri = FeedTest.Sub3Uri, ID = "id3", Version = new ImplementationVersion("0.1")});

            GetMock<ISolver>().SetupSequence(x => x.Solve(requirements))
                .Returns(selectionsOld)
                .Returns(selectionsNew);

            // Download uncached implementations
            ExpectFetchUncached(selectionsNew,
                new Implementation {ID = "id1"},
                new Implementation {ID = "id2"},
                new Implementation {ID = "id3"});

            // Check for <replaced-by>
            GetMock<IFeedCache>().Setup(x => x.GetFeed(FeedTest.Test1Uri)).Returns(FeedTest.CreateTestFeed());

            RunAndAssert("http://0install.de/feeds/test/test2.xml: 1.0 -> 2.0" + Environment.NewLine + "http://0install.de/feeds/test/sub3.xml: new -> 0.1" + Environment.NewLine, 0, selectionsNew,
                "http://0install.de/feeds/test/test1.xml", "--command=command", "--os=Windows", "--cpu=i586", "--not-before=1.0", "--before=2.0", "--version-for=http://0install.de/feeds/test/test2.xml", "2.0..!3.0");
        }

        [Fact] // Ensures local Selections XMLs are rejected.
        public void TestRejectImportSelections()
        {
            var selections = SelectionsTest.CreateTestSelections();
            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                selections.SaveXml(tempFile);
                Sut.Parse(new string[] {tempFile});
                Assert.Throws<NotSupportedException>(() => Sut.Execute());
            }
        }
    }
}
