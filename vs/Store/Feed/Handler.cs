namespace ZeroInstall.Store.Feed
{
    /// <summary>
    /// A class with abstract callback methods to be used if the the user needs to be asked any questions.
    /// </summary>
    public abstract class Handler
    {
        /// <summary>
        /// Called to ask the user whether he wishes to trust a new GPG key.
        /// </summary>
        /// <param name="information">Comprehensive information about the new key, to help the user make an informed decision.</param>
        /// <returns><see langword="true"/> if the user accepted the new key; <see langword="false"/> if he rejected it.</returns>
        public abstract bool AcceptNewKey(string information);
    }
}
