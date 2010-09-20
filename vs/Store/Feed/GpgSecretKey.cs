using System;

namespace ZeroInstall.Store.Feed
{
    public enum KeyType
    {
        Dsa,
        Elgamal,
        Rsa
    } ;

    public struct GpgSecretKey
    {
        public int BitLength;
        public KeyType KeyType;
        public string MainSigningKey;
        public DateTime CreationDate;
        public string Owner;
        public string EmailAdress;

        public override string ToString()
        {
            return Owner;
        }
    }
}
