using System;
using System.Diagnostics.CodeAnalysis;
using ZeroInstall.Model;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Indicates an <see cref="Implementation"/> directory does not match a <see cref="ManifestDigest"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "This exception type has a specific signaling purpose and doesn't need to carry extra info like Messages")]
    public class DigestMismatchException : Exception
    {
        #region Properties
        /// <summary>
        /// The hash value the <see cref="Implementation"/> was supposed to have.
        /// </summary>
        public string ExpectedHash { get; private set; }

        /// <summary>
        /// The hash value that was actually calculated.
        /// </summary>
        public string ActualHash { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new digest mismatch exception.
        /// </summary>
        /// <param name="expectedHash">The hash value the <see cref="Implementation"/> was supposed to have.</param>
        /// <param name="actualHash">The hash value that was actually calculated.</param>
        public DigestMismatchException(string expectedHash, string actualHash)
        {
            ExpectedHash = expectedHash;
            ActualHash = actualHash;
        }
        #endregion
    }
}
