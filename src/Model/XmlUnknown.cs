using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace ZeroInstall.Model
{
    /// <summary>
    /// Abstract base class for XML serializable classes that are intended to retain any unkown XML elements or attributes loaded from an XML file.
    /// </summary>
    /// <remarks>Inheriting from this class will prevent the <see cref="XmlSerializer.UnknownElement"/> event from being triggerd.</remarks>
    [Serializable]
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

        #region List conversion
        private static C5.IList<XmlAttribute> GetList(XmlAttribute[] attributes)
        {
            var list = new C5.ArrayList<XmlAttribute>(attributes.Length, new XmlAttributeComparer());
            list.AddAll(attributes);
            return list;
        }

        private static C5.IList<XmlAttribute> GetList(XmlAttributeCollection attributes)
        {
            var array = new XmlAttribute[attributes.Count];
            attributes.CopyTo(array, 0);
            return GetList(array);
        }

        private static C5.IList<XmlElement> GetList(XmlElement[] elements)
        {
            var list = new C5.ArrayList<XmlElement>(elements.Length, new XmlElementComparer());
            list.AddAll(elements);
            return list;
        }

        private static C5.IList<XmlElement> GetList(XmlNodeList nodes)
        {
            var list = new C5.ArrayList<XmlElement>(nodes.Count, new XmlElementComparer());
            list.AddAll(nodes.OfType<XmlElement>());
            return list;
        }
        #endregion

        #region Comparers
        private class XmlAttributeComparer : IEqualityComparer<XmlAttribute>
        {
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
            public bool Equals(XmlElement x, XmlElement y)
            {
                if (x == null || y == null) return false;
                if (x.NamespaceURI != y.NamespaceURI || x.Name != y.Name || x.InnerText != y.InnerText) return false;
                return
                    GetList(x.Attributes).UnsequencedEquals(GetList(y.Attributes)) &&
                        GetList(x.ChildNodes).SequencedEquals(GetList(y.ChildNodes));
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
            return
                GetList(UnknownAttributes ?? new XmlAttribute[0]).UnsequencedEquals(GetList(other.UnknownAttributes ?? new XmlAttribute[0])) &&
                    GetList(UnknownElements ?? new XmlElement[0]).SequencedEquals(GetList(other.UnknownElements ?? new XmlElement[0]));
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = 397;
                // ReSharper disable NonReadonlyFieldInGetHashCode
                result = (result * 397) ^ GetList(UnknownAttributes ?? new XmlAttribute[0]).GetUnsequencedHashCode();
                result = (result * 397) ^ GetList(UnknownElements ?? new XmlElement[0]).GetSequencedHashCode();
                // ReSharper restore NonReadonlyFieldInGetHashCode
                return result;
            }
        }
        #endregion
    }
}
