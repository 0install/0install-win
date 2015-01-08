using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using NanoByte.Common.Collections;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Abstract base class for XML serializable classes that are intended to retain any unknown XML elements or attributes loaded from an XML file.
    /// </summary>
    /// <remarks>Inheriting from this class will prevent the <see cref="XmlSerializer.UnknownElement"/> event from being triggerd.</remarks>
    public abstract class XmlUnknown
    {
        /// <summary>
        /// Contains any unknown additional XML attributes.
        /// </summary>
        [XmlAnyAttribute]
        public XmlAttribute[] UnknownAttributes;

        /// <summary>
        /// Contains any unknown additional XML elements.
        /// </summary>
        [XmlAnyElement]
        public XmlElement[] UnknownElements;

        #region Comparers
        private class XmlAttributeComparer : IEqualityComparer<XmlAttribute>
        {
            public static readonly XmlAttributeComparer Instance = new XmlAttributeComparer();

            public bool Equals(XmlAttribute x, XmlAttribute y)
            {
                if (x == null || y == null) return false;
                return x.NamespaceURI == y.NamespaceURI && x.Name == y.Name && x.Value == y.Value;
            }

            [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
            public int GetHashCode(XmlAttribute obj)
            {
                return (obj.Name + obj.Value).GetHashCode();
            }
        }

        private class XmlElementComparer : IEqualityComparer<XmlElement>
        {
            public static readonly XmlElementComparer Instance = new XmlElementComparer();

            public bool Equals(XmlElement x, XmlElement y)
            {
                if (x == null || y == null) return false;
                if (x.NamespaceURI != y.NamespaceURI || x.Name != y.Name || x.InnerText != y.InnerText) return false;

                // ReSharper disable once InvokeAsExtensionMethod
                bool attributesEqual = EnumerableExtensions.UnsequencedEquals(
                    x.Attributes.OfType<XmlAttribute>().ToArray(),
                    y.Attributes.OfType<XmlAttribute>().ToArray(),
                    comparer: XmlAttributeComparer.Instance);
                bool elementsEqual = EnumerableExtensions.SequencedEquals(
                    x.ChildNodes.OfType<XmlElement>().ToArray(),
                    y.ChildNodes.OfType<XmlElement>().ToArray(),
                    comparer: Instance);
                return attributesEqual && elementsEqual;
            }

            [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
            public int GetHashCode(XmlElement obj)
            {
                return (obj.Name + obj.Value).GetHashCode();
            }
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        protected bool Equals(XmlUnknown other)
        {
            if (other == null) return false;
            // ReSharper disable once InvokeAsExtensionMethod
            bool attributesEqual = EnumerableExtensions.UnsequencedEquals(
                UnknownAttributes ?? new XmlAttribute[0],
                other.UnknownAttributes ?? new XmlAttribute[0],
                comparer: XmlAttributeComparer.Instance);
            bool elementsEqual = EnumerableExtensions.SequencedEquals(
                UnknownElements ?? new XmlElement[0],
                other.UnknownElements ?? new XmlElement[0],
                comparer: XmlElementComparer.Instance);
            return attributesEqual && elementsEqual;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = 397;
                result = (result * 397) ^ (UnknownAttributes ?? new XmlAttribute[0]).GetUnsequencedHashCode(XmlAttributeComparer.Instance);
                result = (result * 397) ^ (UnknownElements ?? new XmlElement[0]).GetSequencedHashCode(XmlElementComparer.Instance);
                return result;
            }
        }
        #endregion
    }
}
