namespace ZeroInstall.Store.Feed
{
    /// <summary>
    /// Silently ignores all requests and answers them with "No".
    /// </summary>
    public sealed class SilentHandler : Handler
    {
        public override bool AcceptNewKey(string information)
        {
            return false;
        }
    }
}
