namespace Common
{
    /// <summary>
    /// An object with a read/write <see cref="Name"/> property.
    /// </summary>
    public interface INamed
    {
        /// <summary>
        /// The name of the object.
        /// </summary>
        string Name { get; set; }
    }
}
