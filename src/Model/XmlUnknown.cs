using System.Xml;
using System.Xml.Serialization;

namespace ZeroInstall.Model
{
    /// <summary>
    /// Abstract base class for XML serializable classes that are intended to retain any unkown XML elements or attributes loaded from an XML file.
    /// </summary>
    /// <remarks>Inheriting from this class will prevent the <see cref="XmlSerializer.UnknownElement"/> event from being triggerd.</remarks>
    public abstract class XmlUnknown
    {
        /// <summary>
        /// Contains any unknown additional XML elements.
        /// </summary>
        [XmlAnyElement]
        public XmlElement[] UnknownElements;

        /// <summary>
        /// Contains any unknown additional XML attributes.
        /// </summary>
        [XmlAnyAttribute]
        public XmlAttribute[] UnknownAttributes;

        #region Equality
        /// <inheritdoc/>
        protected bool Equals(XmlUnknown other)
        {
            if (other == null) return false;

            // Convert arrays to list for easier comparison
            var elements = new C5.LinkedList<XmlElement>();
            if (UnknownElements != null) elements.AddAll(UnknownElements);
            var attributes = new C5.LinkedList<XmlAttribute>();
            if (UnknownAttributes != null) attributes.AddAll(UnknownAttributes);
            var otherElements = new C5.LinkedList<XmlElement>();
            if (other.UnknownElements != null) otherElements.AddAll(other.UnknownElements);
            var otherAttributes = new C5.LinkedList<XmlAttribute>();
            if (other.UnknownAttributes != null) otherAttributes.AddAll(other.UnknownAttributes);

            return elements.SequencedEquals(otherElements) && attributes.SequencedEquals(otherAttributes);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            // Convert arrays to list for easier comparison
            var elements = new C5.LinkedList<XmlElement>();
            if (UnknownElements != null) elements.AddAll(UnknownElements);
            var attributes = new C5.LinkedList<XmlAttribute>();
            if (UnknownAttributes != null) attributes.AddAll(UnknownAttributes);

            unchecked
            {
                return (elements.GetSequencedHashCode() * 397) ^ attributes.GetSequencedHashCode();
            }
        }
        #endregion
    }
}
