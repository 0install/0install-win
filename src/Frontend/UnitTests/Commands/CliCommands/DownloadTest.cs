/*
 * Copyright 2010-2015 Bastian Eicher
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

using NanoByte.Common.Storage;
using NUnit.Framework;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Contains integration tests for <see cref="Download"/>.
    /// </summary>
    [TestFixture]
    public class DownloadTest : SelectionTestBase<Download>
    {
        [Test(Description = "Ensures all options are parsed and handled correctly.")]
        public override void TestNormal()
        {
            var testImplementation1 = new Implementation {ID = "id1", ManifestDigest = new ManifestDigest(sha256: "abc"), Version = new ImplementationVersion("1.0")};
            var testImplementation2 = new Implementation {ID = "id2", ManifestDigest = new ManifestDigest(sha256: "xyz"), Version = new ImplementationVersion("1.0")};

            var requirements = RequirementsTest.CreateTestRequirements();
            var selections = SelectionsTest.CreateTestSelections();

            SolverMock.Setup(x => x.Solve(requirements)).Returns(selections);

            // Download uncached implementations
            SelectionsManagerMock.Setup(x => x.GetUncachedSelections(selections)).Returns(selections.Implementations);
            SelectionsManagerMock.Setup(x => x.GetImplementations(selections.Implementations)).Returns(new[] {testImplementation1, testImplementation2});
            FetcherMock.Setup(x => x.Fetch(new[] {testImplementation1, testImplementation2}));

            RunAndAssert(Resources.AllComponentsDownloaded, 0, selections,
                "http://0install.de/feeds/test/test1.xml", "--command=command", "--os=Windows", "--cpu=i586", "--not-before=1.0", "--before=2.0", "--version-for=http://0install.de/feeds/test/test2.xml", "2.0..!3.0");
        }

        [Test(Description = "Ensures local Selections XMLs are correctly detected and parsed.")]
        public override void TestImportSelections()
        {
            var testImplementation1 = new Implementation {ID = "id1", ManifestDigest = new ManifestDigest(sha256: "abc"), Version = new ImplementationVersion("1.0")};
            var testImplementation2 = new Implementation {ID = "id2", ManifestDigest = new ManifestDigest(sha256: "xyz"), Version = new ImplementationVersion("1.0")};

            var selections = SelectionsTest.CreateTestSelections();

            // Download uncached implementations
            SelectionsManagerMock.Setup(x => x.GetUncachedSelections(selections)).Returns(selections.Implementations);
            SelectionsManagerMock.Setup(x => x.GetImplementations(selections.Implementations)).Returns(new[] {testImplementation1, testImplementation2});
            FetcherMock.Setup(x => x.Fetch(new[] {testImplementation1, testImplementation2}));

            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                selections.SaveXml(tempFile);
                RunAndAssert(Resources.AllComponentsDownloaded, 0, selections, tempFile);
            }
        }
    }
}
