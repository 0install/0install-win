namespace ZeroInstall.Bootstrap
{
    /// <summary>
    /// Application bootstrapping mode.
    /// </summary>
    public enum AppMode
    {
        /// <summary>
        /// Perform no application bootstrapping.
        /// </summary>
        None,

        /// <summary>
        /// Run the target application.
        /// </summary>
        Run,

        /// <summary>
        /// Perform desktop integration for the target application.
        /// </summary>
        Integrate
    }
}