// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Collections.Generic;

namespace ZeroInstall.Hooking
{
    /// <summary>
    /// Applies a set of filter rules to registry access transparently replacing values (usually file paths) on-the-fly.
    /// </summary>
    [Serializable]
    public class RegistryFilter
    {
        #region Variables
        /// <summary>A list of case-insensitive values and the values they shall be mapped to when stored in the registry.</summary>
        private readonly RegistryFilterRule[] _rules;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new registry filter.
        /// </summary>
        /// <param name="rules">A list of case-insensitive values and the values they shall be mapped to when stored in the registry.</param>
        public RegistryFilter(ICollection<RegistryFilterRule> rules)
        {
            #region Sanity checks
            if (rules == null) throw new ArgumentNullException(nameof(rules));
            #endregion

            _rules = new RegistryFilterRule[rules.Count];

            int i = 0;
            foreach (var rule in rules)
                _rules[i++] = rule;

            // Sort backwards to make sure the most specific matches are selected first
            Array.Sort(_rules, (rule1, rule2) => rule2.CompareTo(rule1));
        }
        #endregion

        //--------------------//

        #region Filtering
        /// <summary>
        /// Filters a registry read request undoing <see cref="_rules"/>.
        /// </summary>
        /// <param name="fromRegistry">The raw value read from the registry.</param>
        /// <returns>The value from the registry with any required substitutions applied or <c>null</c> if nothing was changed.</returns>
        internal string ReadFilter(string fromRegistry)
        {
            if (string.IsNullOrEmpty(fromRegistry)) return fromRegistry;

            for (int i = 0; i < _rules.Length; i++)
            {
                if (fromRegistry.StartsWith(_rules[i].RegistryValue))
                    return _rules[i].ProcessValue + fromRegistry.Substring(_rules[i].RegistryValue.Length);
            }

            return null;
        }

        /// <summary>
        /// Filters a registry write request applying <see cref="_rules"/>.
        /// </summary>
        /// <param name="toRegistry">The value to be written to the registry.</param>
        /// <returns>The value for the registry with any required substitutions applied or <c>null</c> if nothing was changed.</returns>
        internal string WriteFilter(string toRegistry)
        {
            if (string.IsNullOrEmpty(toRegistry)) return toRegistry;

            for (int i = 0; i < _rules.Length; i++)
            {
                if (toRegistry.StartsWith(_rules[i].ProcessValue))
                    return _rules[i].RegistryValue + toRegistry.Substring(_rules[i].ProcessValue.Length);
            }

            return null;
        }
        #endregion
    }
}
