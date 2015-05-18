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

using System.Xml.Serialization;
using NanoByte.Common.Storage;
using NUnit.Framework;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Contains test methods for <see cref="XmlUnknown"/>'s equality testing logic.
    /// </summary>
    [TestFixture]
    public class XmlUnknownTest
    {
        [XmlRoot(ElementName = "root")]
        public sealed class XmlUnknownStub : XmlUnknown
        {
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj is XmlUnknownStub && Equals((XmlUnknown)obj);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        [Test]
        public void TestEquals()
        {
            var dataBase = XmlStorage.FromXmlString<XmlUnknownStub>("<root key1=\"value1\" key2=\"value2\"><element key=\"value\">text<child1 key=\"value\" /><child2 key=\"value\" /></element></root>");
            var dataAttribSwap = XmlStorage.FromXmlString<XmlUnknownStub>("<root key2=\"value2\" key1=\"value1\"><element key=\"value\">text<child1 key=\"value\" /><child2 key=\"value\" /></element></root>");
            var dataChildSwap = XmlStorage.FromXmlString<XmlUnknownStub>("<root key1=\"value1\" key2=\"value2\"><element key=\"value\">text<child2 key=\"value\" /><child1 key=\"value\" /></element></root>");
            var dataAttibChange = XmlStorage.FromXmlString<XmlUnknownStub>("<root key1=\"value1\" key2=\"valueX\"><element key=\"value\">text<child1 key=\"value\" /><child2 key=\"value\" /></element></root>");
            var dataChildAttibChange = XmlStorage.FromXmlString<XmlUnknownStub>("<root key1=\"value1\" key2=\"value2\"><element key=\"value\">text<child1 key=\"value\" /><child2 key=\"valueX\" /></element></root>");
            var dataTextChange = XmlStorage.FromXmlString<XmlUnknownStub>("<root key1=\"value1\" key2=\"value2\"><element key=\"value\">new text<child1 key=\"value\" /><child2 key=\"value\" /></element></root>");

            Assert.AreEqual(dataBase, dataBase);
            Assert.AreEqual(dataBase, dataAttribSwap);
            Assert.AreEqual(dataBase.GetHashCode(), dataAttribSwap.GetHashCode());
            Assert.AreNotEqual(dataBase, dataChildSwap);
            Assert.AreNotEqual(dataBase, dataAttibChange);
            Assert.AreNotEqual(dataBase, dataChildAttibChange);
            Assert.AreNotEqual(dataBase, dataTextChange);
        }
    }
}
