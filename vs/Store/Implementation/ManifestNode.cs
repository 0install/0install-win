namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// An entry in <see cref="Manifest.Nodes"/>.
    /// </summary>
    public abstract class ManifestNode
    {
        /// <summary>
        /// Returns the string representation of this node for the old manifest format.
        /// </summary>
        public virtual string ToStringOld()
        {
            return ToString();
        }
    }
}
