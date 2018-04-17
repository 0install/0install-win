// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using NanoByte.Common.Storage;
using Xunit;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Contains integration tests for <see cref="Download"/>.
    /// </summary>
    public class DownloadTest : SelectionTestBase<Download>
    {
        [Fact] // Ensures all options are parsed and handled correctly.
        public void TestNormal()
        {
            var selections = ExpectSolve();

            ExpectFetchUncached(selections,
                new Implementation {ID = "id1", ManifestDigest = new ManifestDigest(sha256: "abc"), Version = new ImplementationVersion("1.0")},
                new Implementation {ID = "id2", ManifestDigest = new ManifestDigest(sha256: "xyz"), Version = new ImplementationVersion("1.0")});

            RunAndAssert(Resources.AllComponentsDownloaded, 0, selections,
                "http://0install.de/feeds/test/test1.xml", "--command=command", "--os=Windows", "--cpu=i586", "--not-before=1.0", "--before=2.0", "--version-for=http://0install.de/feeds/test/test2.xml", "2.0..!3.0");
        }

        [Fact] // Ensures local Selections XMLs are correctly detected and parsed.
        public void TestImportSelections()
        {
            var selections = Fake.Selections;

            ExpectFetchUncached(selections,
                new Implementation {ID = "id1", ManifestDigest = new ManifestDigest(sha256: "abc"), Version = new ImplementationVersion("1.0")},
                new Implementation {ID = "id2", ManifestDigest = new ManifestDigest(sha256: "xyz"), Version = new ImplementationVersion("1.0")});

            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                selections.SaveXml(tempFile);

                selections.Normalize();
                RunAndAssert(Resources.AllComponentsDownloaded, 0, selections, tempFile);
            }
        }
    }
}
