namespace ZeroInstall.Store
{
    /// <summary>
    /// Available operation modes for the Zero Install Bootstrapper.
    /// </summary>
    public enum BootstrapMode
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