namespace ZeroInstall.Injector
{
    /// <summary>
    /// List of operational modes that can usually be selected via command-line arguments.
    /// </summary>
    public enum OperationMode
    {
        /// <summary>Launch or download an <see cref="Model.Feed"/>.</summary>
        Normal,
        /// <summary>List known <see cref="Model.Feed"/>s.</summary>
        List,
        /// <summary>Import feed files from local source.</summary>
        Import,
        /// <summary>Add feed aliases.</summary>
        Manage,
        /// <summary>Display version information.</summary>
        Version
    }
}
