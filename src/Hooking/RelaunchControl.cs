// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Collections.Generic;
using System.IO;

namespace ZeroInstall.Hooking
{
    /// <summary>
    /// Stores information about how commands within an implementation can be relaunched. Used for Windows 7 taskbar pinning.
    /// </summary>
    [Serializable]
    public class RelaunchControl
    {
        #region Variables
        /// <summary>A list of case-insensitive values and the values they shall be mapped to when stored in the registry.</summary>
        private readonly List<RelaunchEntry> _entries;

        /// <summary>
        /// The fully qualified path of the Zero Install Command GUI executable.
        /// </summary>
        public readonly string CommandPathGui;

        /// <summary>
        /// The fully qualified path of the Zero Install Command Cli executable.
        /// </summary>
        public readonly string CommandPathCli;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new relaunch control object.
        /// </summary>
        /// <param name="entries">A list of entries describing how to relaunch individual commands.</param>
        /// <param name="commandPathGui">The fully qualified path of the Zero Install Command GUI executable.</param>
        /// <param name="commandPathCli">The fully qualified path of the Zero Install Command CLI executable.</param>
        public RelaunchControl(IEnumerable<RelaunchEntry> entries, string commandPathGui, string commandPathCli)
        {
            #region Sanity checks
            if (entries == null) throw new ArgumentNullException(nameof(entries));
            if (string.IsNullOrEmpty(commandPathGui)) throw new ArgumentNullException(nameof(commandPathGui));
            if (string.IsNullOrEmpty(commandPathCli)) throw new ArgumentNullException(nameof(commandPathCli));
            #endregion

            _entries = new List<RelaunchEntry>(entries);
            CommandPathGui = commandPathGui;
            CommandPathCli = commandPathCli;
        }
        #endregion

        //--------------------//

        #region Access
        /// <summary>
        /// Locates the correct entry for the current process.
        /// </summary>
        /// <returns>The first entry matching the binary name of the current process; the first entry if none matches; <c>null</c> if there are no entries at all.</returns>
        internal RelaunchEntry GetCurrentEntry()
        {
            if (_entries.Count == 0) return null;

            // Determine the name of the binary without leading directories or trailing file extensions
            string binaryName = Path.GetFileName(WindowsUtils.CurrentProcessPath);
            if (binaryName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) binaryName = binaryName.Substring(0, binaryName.Length - 4);

            // Try to find a match and fall back to first entry if none is found
            var result = _entries.Find(entry => entry.BinaryName == binaryName);
            if (result == null && _entries.Count != 0) result = _entries[0];
            return result;
        }
        #endregion
    }
}
