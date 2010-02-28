using System;
using System.Diagnostics.CodeAnalysis;

namespace Common
{
    /// <summary>
    /// To be thrown when an action was cancelled because the user wished it to be.
    /// </summary>
    [Serializable]
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "This exception type has only a signaling purpose and doesn't need to carry extra info like Messages")]
    public class UserCancelException : Exception
    {}
}
