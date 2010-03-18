using System.ComponentModel;

namespace Common
{
    /// <summary>
    /// An object with a unique name, suitable for identification in lists.
    /// </summary>
    /// <see cref="Collections.INamedCollection{T}"/>
    public interface INamed
    {
        /// <summary>
        /// A unique name for the object.
        /// </summary>
        [ReadOnly(true)]
        string Name { get; }
    }
}
