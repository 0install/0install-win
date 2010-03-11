namespace ZeroInstall.Model
{
    public interface ISimplifyable
    {
        /// <summary>
        /// Sets missing default values, flattens the inheritance structure, etc.
        /// </summary>
        /// <remarks>This should be called to prepare an interface for launch.
        /// It should not be called if you plan on serializing the <see cref="Interface"/> again since it will may some of its structure.</remarks>
        void Simplify();
    }
}
