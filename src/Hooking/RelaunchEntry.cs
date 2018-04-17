// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Security.Cryptography;
using System.Text;

namespace ZeroInstall.Hooking
{
    /// <summary>
    /// Describes how a specific command within an implementation can be relaunched.
    /// </summary>
    /// <seealso cref="RelaunchControl"/>
    [Serializable]
    public class RelaunchEntry
    {
        #region Variables
        /// <summary>
        /// The canonical name of the binary supplying the command (without file extensions); can be <c>null</c>.
        /// </summary>
        public readonly string BinaryName;

        /// <summary>
        /// The name to use for pinning this specific window to the taskbar.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The Zero Install target (appended to a "0install run" command, escape accordingly).
        /// </summary>
        public readonly string Target;

        /// <summary>
        /// Indicates whether this command requires a terminal to run.
        /// </summary>
        public readonly bool NeedsTerminal;

        /// <summary>
        /// The path to the icon to use for pinning this specific window to the taskbar; can be <c>null</c>.
        /// </summary>
        public readonly string IconPath;

        /// <summary>
        /// The application user model ID used by Windows to identify the entry.
        /// </summary>
        public readonly string AppID;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new relaunch entry.
        /// </summary>
        /// <param name="binaryName">The canonical name of the binary supplying the command (without file extensions); can be <c>null</c>.</param>
        /// <param name="name">The name to use for pinning this specific window to the taskbar.</param>
        /// <param name="target">The Zero Install target. This will be used as a command-line argument, escape accordingly.</param>
        /// <param name="needsTerminal">Indicates whether this command requires a terminal to run.</param>
        /// <param name="iconPath">The path to the icon to use for pinning this specific window to the taskbar; can be <c>null</c>.</param>
        public RelaunchEntry(string binaryName, string name, string target, bool needsTerminal, string iconPath)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException(nameof(target));
            #endregion

            BinaryName = binaryName;
            Name = name;
            IconPath = iconPath;
            Target = target;
            NeedsTerminal = needsTerminal;

            // Derive application model ID from Zero Install target
            var hash = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(target));
            AppID = "ZeroInstallApp." + BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the rule in the form "RelaunchEntry (Name): Target". Not safe for parsing!
        /// </summary>
        public override string ToString() => $"RelaunchEntry ({Name}): {Target}";
        #endregion
    }
}
