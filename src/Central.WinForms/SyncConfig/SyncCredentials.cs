namespace ZeroInstall.Central.WinForms.SyncConfig
{
    internal struct SyncCredentials
    {
        public string Username { get; private set; }

        public string Password { get; private set; }

        public SyncCredentials(string username, string password) : this()
        {
            Username = username;
            Password = password;
        }
    }
}
