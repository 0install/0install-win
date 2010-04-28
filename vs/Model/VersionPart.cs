using System;

namespace ZeroInstall.Model
{
    /// <summary>
    /// Represents a part of a <see cref="ImplementationVersion"/>.
    /// </summary>
    internal abstract class VersionPart : IEquatable<VersionPart>, IComparable<VersionPart>
    {
        public abstract bool Equals(VersionPart other);

        public abstract int CompareTo(VersionPart other);
    }
}
