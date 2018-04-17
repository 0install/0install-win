// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;

namespace ZeroInstall.Hooking
{
    /// <summary>
    /// Represents a filtering/replacement rule in <see cref="RegistryFilter"/>.
    /// </summary>
    [Serializable]
    public struct RegistryFilterRule : IEquatable<RegistryFilterRule>, IComparable<RegistryFilterRule>
    {
        #region Variables
        /// <summary>
        /// The value as it is seen by the process.
        /// </summary>
        public readonly string ProcessValue;

        /// <summary>
        /// The value as it is actually stored in the registry.
        /// </summary>
        public readonly string RegistryValue;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new filter rule.
        /// </summary>
        /// <param name="processValue">The value as it is seen by the process; may not be <c>null</c>!</param>
        /// <param name="registryValue">The value as it is actually stored in the registry; may not be <c>null</c>!</param>
        public RegistryFilterRule(string processValue, string registryValue)
        {
            ProcessValue = processValue ?? throw new ArgumentNullException(nameof(processValue));
            RegistryValue = registryValue ?? throw new ArgumentNullException(nameof(registryValue));
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the rule in the form "ProcessValue = RegistryValue". Not safe for parsing!
        /// </summary>
        public override string ToString() => ProcessValue + " = " + RegistryValue;
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(RegistryFilterRule other) => Equals(ProcessValue, other.ProcessValue) && Equals(RegistryValue, other.RegistryValue);

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj != null && obj is RegistryFilterRule rule && Equals(rule);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = ProcessValue.GetHashCode();
                result = (result * 397) ^ RegistryValue.GetHashCode();
                return result;
            }
        }

        /// <inheritdoc/>
        public static bool operator ==(RegistryFilterRule left, RegistryFilterRule right) => left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(RegistryFilterRule left, RegistryFilterRule right) => !left.Equals(right);
        #endregion

        #region Comparison
        /// <inheritdoc/>
        public int CompareTo(RegistryFilterRule other)
        {
            // Compare by ProcessValue first, then by RegistryValue if that was equal
            int processCompare = string.CompareOrdinal(ProcessValue, other.ProcessValue);
            return (processCompare == 0) ? string.CompareOrdinal(RegistryValue, other.RegistryValue) : processCompare;
        }

        /// <inheritdoc/>
        public static bool operator <(RegistryFilterRule left, RegistryFilterRule right) => left.CompareTo(right) < 0;

        /// <inheritdoc/>
        public static bool operator >(RegistryFilterRule left, RegistryFilterRule right) => left.CompareTo(right) > 0;

        /// <inheritdoc/>
        public static bool operator <=(RegistryFilterRule left, RegistryFilterRule right) => left.CompareTo(right) <= 0;

        /// <inheritdoc/>
        public static bool operator >=(RegistryFilterRule left, RegistryFilterRule right) => left.CompareTo(right) >= 0;
        #endregion
    }
}
