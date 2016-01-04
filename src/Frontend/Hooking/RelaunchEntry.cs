/*
 * Copyright 2010-2015 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

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
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException("target");
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
        public override string ToString()
        {
            return string.Format("RelaunchEntry ({0}): {1}", Name, Target);
        }
        #endregion
    }
}
