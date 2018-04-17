// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using NanoByte.Common.Storage;
using Xunit;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Contains integration tests for <see cref="Selection"/>.
    /// </summary>
    public class SelectionTest : SelectionTestBase<Selection>
    {
        [Fact] // Ensures all options are parsed and handled correctly.
        public virtual void TestNormal()
        {
            var selections = ExpectSolve();

            RunAndAssert(selections.ToXmlString(), 0, selections,
                "--xml", "http://0install.de/feeds/test/test1.xml", "--command=command", "--os=Windows", "--cpu=i586", "--not-before=1.0", "--before=2.0", "--version-for=http://0install.de/feeds/test/test2.xml", "2.0..!3.0");
        }

        [Fact] // Ensures local Selections XMLs are correctly detected and parsed.
        public virtual void TestImportSelections()
        {
            var selections = Fake.Selections;
            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                selections.SaveXml(tempFile);

                selections.Normalize();
                RunAndAssert(selections.ToXmlString(), 0, selections,
                    "--xml", tempFile);
            }
        }
    }
}
