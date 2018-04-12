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
using Moq;
using NanoByte.Common.Storage;
using Xunit;
using ZeroInstall.Services;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;
using ZeroInstall.Store.ViewModel;

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
            var requirements = CreateTestRequirements();
            var selectionsOld = Fake.Selections;
            var selectionsNew = Fake.Selections;
            selectionsNew.Implementations[1].Version = new ImplementationVersion("2.0");
            selectionsNew.Implementations.Add(new ImplementationSelection {InterfaceUri = Fake.SubFeed3Uri, ID = "id3", Version = new ImplementationVersion("0.1")});

            GetMock<ISolver>().SetupSequence(x => x.Solve(requirements))
                .Returns(selectionsOld)
                .Returns(selectionsNew);
            GetMock<IFeedCache>().Setup(x => x.GetFeed(Fake.Feed1Uri)).Returns(Fake.Feed);

            // Download uncached implementations
            ExpectFetchUncached(selectionsNew,
                new Implementation {ID = "id1"},
                new Implementation {ID = "id2"},
                new Implementation {ID = "id3"});

            var diffNodes = new[] {new SelectionsDiffNode(Fake.Feed2Uri)};
            GetMock<ISelectionsManager>().Setup(x => x.GetDiff(selectionsOld, selectionsNew)).Returns(diffNodes);

            RunAndAssert(diffNodes, 0,
                "http://0install.de/feeds/test/test1.xml", "--command=command", "--os=Windows", "--cpu=i586", "--not-before=1.0", "--before=2.0", "--version-for=http://0install.de/feeds/test/test2.xml", "2.0..!3.0");
        }

        [Fact] // Ensures local Selections XMLs are rejected.
        public void TestRejectImportSelections()
        {
            var selections = Fake.Selections;
            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                selections.SaveXml(tempFile);
                Sut.Parse(new string[] {tempFile});
                Assert.Throws<NotSupportedException>(() => Sut.Execute());
            }
        }
    }
}
