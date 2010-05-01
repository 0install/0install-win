using System;
using ZeroInstall.Model;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Indicates an <see cref="Implementation"/> directory does not match a <see cref="ManifestDigest"/>.
    /// </summary>
    public class DigestMismatchException : Exception
    {
    }
}
