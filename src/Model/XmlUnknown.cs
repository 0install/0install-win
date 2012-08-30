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
        /// Contains any unknown additional XML attributes.
        /// </summary>
        [XmlAnyAttribute]
        public XmlAttribute[] UnknownAttributes;

        /// <summary>
        /// Contains any unknown additional XML elements.
        /// </summary>
        [XmlAnyElement]
        public XmlElement[] UnknownElements;

        #region Equality
        /// <inheritdoc/>
        protected bool Equals(XmlUnknown other)
        {
            if (other == null) return false;

            if (UnknownAttributes == null)
            {
                if (other.UnknownAttributes != null) return false;
            }
            else
            {
                if (other.UnknownAttributes == null || UnknownAttributes.Length != other.UnknownAttributes.Length) return false;
                else
                {
                    // ToDo: Compare UnknownAttributes
                }
            }

            if (UnknownElements == null)
            {
                if (other.UnknownElements != null) return false;
            }
            else
            {
                if (other.UnknownElements == null || UnknownElements.Length != other.UnknownElements.Length) return false;
                else
                {
                    // ToDo: Compare UnknownElements
                }
            }

            return true;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = 397;
                // ReSharper disable NonReadonlyFieldInGetHashCode
                if (UnknownAttributes != null)
                {
                    foreach (var attribute in UnknownAttributes)
                        result = (result * 397) ^ attribute.Name.GetHashCode();
                }
                if (UnknownElements != null)
                {
                    foreach (var element in UnknownElements)
                        result = (result * 397) ^ element.Name.GetHashCode();
                }
                // ReSharper restore NonReadonlyFieldInGetHashCode
                return result;
            }
        }
        #endregion
    }
}
