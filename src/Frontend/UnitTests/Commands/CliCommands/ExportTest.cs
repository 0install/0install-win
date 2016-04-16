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

using System.IO;
using FluentAssertions;
using NDesk.Options;
using NUnit.Framework;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Contains integration tests for <see cref="Export"/>.
    /// </summary>
    [TestFixture]
    public class ExportTest : SelectionTestBase<Export>
    {
        [Test(Description = "Ensures all options are parsed and handled correctly.")]
        public void TestNormal()
        {
            var selections = ExpectSolve();

            ExpectFetchUncached(selections,
                new Implementation {ID = "id1", ManifestDigest = new ManifestDigest(sha256: "123"), Version = new ImplementationVersion("1.0")},
                new Implementation {ID = "id2", ManifestDigest = new ManifestDigest(sha256: "abc"), Version = new ImplementationVersion("1.0")});

            string outputDir = Path.GetTempPath();

            GetMock<IExporter>().Setup(x => x.ExportFeeds(selections, outputDir));
            GetMock<IExporter>().Setup(x => x.ExportImplementations(selections, outputDir, Handler));
            GetMock<IExporter>().Setup(x => x.DeployBootstrap(outputDir));

            RunAndAssert(Resources.AllComponentsExported, 0, selections,
                "http://0install.de/feeds/test/test1.xml", outputDir, "--command=command", "--os=Windows", "--cpu=i586", "--not-before=1.0", "--before=2.0", "--version-for=http://0install.de/feeds/test/test2.xml", "2.0..!3.0");
        }

        [Test(Description = "Ensures exporting without implementations works.")]
        public void TestNoImplementations()
        {
            var selections = ExpectSolve();

            // Note: Result is never used in this execution path
            ExpectListUncached(selections);

            string outputDir = Path.GetTempPath();

            GetMock<IExporter>().Setup(x => x.ExportFeeds(selections, outputDir));
            GetMock<IExporter>().Setup(x => x.DeployBootstrap(outputDir));

            RunAndAssert(Resources.AllComponentsExported, 0, null,
                "http://0install.de/feeds/test/test1.xml", outputDir, "--no-implementations", "--command=command", "--os=Windows", "--cpu=i586", "--not-before=1.0", "--before=2.0", "--version-for=http://0install.de/feeds/test/test2.xml", "2.0..!3.0");
        }

        [Test(Description = "Ensures calling with too many arguments raises an exception.")]
        public override void TestTooManyArgs()
        {
            Target.Invoking(x => x.Parse(new[] {"http://0install.de/feeds/test/test1.xml", "output-path", "arg1"}))
                .ShouldThrow<OptionException>(because: "Should reject more than one argument");
        }
    }
}
