/*
 * Copyright 2010-2011 Bastian Eicher
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
using Common.Storage;
using NUnit.Framework;

namespace ZeroInstall.Model.Capabilities
{
    /// <summary>
    /// Contains test methods for <see cref="CapabilityList"/>.
    /// </summary>
    [TestFixture]
    public sealed class CapabilityListTest
    {
        #region Helpers
        /// <summary>
        /// Creates a fictive test <see cref="CapabilityList"/>.
        /// </summary>
        public static CapabilityList CreateTestCapabilityList()
        {
            var testIcon = new Icon(new Uri("http://0install.de/feeds/icons/test.ico"), "image/vnd.microsoft.icon");
            var testVerb = new Verb {Name = Verb.NameOpen, Command = Command.NameRun, Arguments = "--open"};
            return new CapabilityList
            {
                Architecture = new Architecture(OS.Windows, Cpu.All),
                Entries =
                {
                    new AppRegistration {ID = "myapp", CapabilityRegPath = @"SOFTWARE\MyApp\Capabilities", X64 = true},
                    new AutoPlay {ID = "autoplay", Description = "Do somthing", Icons = {testIcon}, Provider = "MyApp", ProgID = "MyApp.Burn", Verb = testVerb, Events = {new AutoPlayEvent {Name = AutoPlayEvent.NameBurnCD}}},
                    new ComServer {ID = "com-server"},
                    new ContextMenu {ID = "context-menu", AllObjects = true, Verb = testVerb},
                    new DefaultProgram {ID = "default-program", Description = "My mail client", Icons = {testIcon}, Verbs = {testVerb}, Service = "Mail"},
                    new FileType {ID = "my_ext", Description = "Text file", Icons = {testIcon}, Extensions = {new FileTypeExtension {Value = "txt", MimeType = "text/plain"}}, Verbs = {testVerb}},
                    new GamesExplorer {ID = "games-explorer"},
                    new UrlProtocol {ID = "my_protocol", Description = "My protocol", Icons = {testIcon}, Verbs = {testVerb}, Prefix = "my-protocol"}
                }
            };
        }
        #endregion

        [Test(Description = "Ensures that the class is correctly serialized and deserialized.")]
        public void TestSaveLoad()
        {
            CapabilityList capabilityList1, capabilityList2;
            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                // Write and read file
                capabilityList1 = CreateTestCapabilityList();
                capabilityList1.Save(tempFile.Path);
                capabilityList2 = CapabilityList.Load(tempFile.Path);
            }

            // Ensure data stayed the same
            Assert.AreEqual(capabilityList1, capabilityList2, "Serialized objects should be equal.");
            Assert.AreEqual(capabilityList1.GetHashCode(), capabilityList2.GetHashCode(), "Serialized objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(capabilityList1, capabilityList2), "Serialized objects should not return the same reference.");
        }

        [Test(Description = "Ensures that the class can be correctly cloned.")]
        public void TestClone()
        {
            var capabilityList1 = CreateTestCapabilityList();
            var capabilityList2 = capabilityList1.CloneCapabilityList();

            // Ensure data stayed the same
            Assert.AreEqual(capabilityList1, capabilityList2, "Cloned objects should be equal.");
            Assert.AreEqual(capabilityList1.GetHashCode(), capabilityList2.GetHashCode(), "Cloned objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(capabilityList1, capabilityList2), "Cloning should not return the same reference.");
        }
    }
}