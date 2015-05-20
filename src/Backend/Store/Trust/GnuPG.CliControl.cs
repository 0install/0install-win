/*
 * Copyright 2010-2015 Bastian Eicher, Simon E. Silva Lauinger
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 *
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Diagnostics;
using System.IO;
using NanoByte.Common;
using NanoByte.Common.Cli;
using NanoByte.Common.Storage;
using NanoByte.Common.Streams;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Trust
{
    partial class GnuPG
    {
        /// <summary>
        /// Manages the interaction with the command-line interface of the external process.
        /// </summary>
        private class CliControl : BundledCliAppControl
        {
            /// <inheritdoc/>
            protected override string AppBinary { get { return "gpg"; } }

            /// <inheritdoc/>
            protected override string AppDirName { get { return "GnuPG"; } }

            private static readonly object _gpgLock = new object();

            /// <inheritdoc/>
            public override string Execute(params string[] arguments)
            {
                // Run only one gpg instance at a time to prevent file system race conditions
                lock (_gpgLock)
                {
                    return base.Execute(arguments);
                }
            }

            /// <inheritdoc/>
            protected override ProcessStartInfo GetStartInfo(params string[] arguments)
            {
                var startInfo = base.GetStartInfo(arguments);
                if (Locations.IsPortable) startInfo.EnvironmentVariables["GNUPGHOME"] = Locations.GetSaveConfigPath("GnuPG", false, "gnupg");
                return startInfo;
            }

            /// <inheritdoc/>
            protected override string HandleStderr(string line)
            {
                #region Sanity checks
                if (line == null) throw new ArgumentNullException("line");
                #endregion

                if (line.StartsWith("gpg: Signature made ") ||
                    line.StartsWith("gpg: Good signature from ") ||
                    line.StartsWith("gpg:                 aka") ||
                    line.StartsWith("gpg: WARNING: This key is not certified") ||
                    line.Contains("There is no indication") ||
                    line.StartsWith("Primary key fingerprint: ") ||
                    line.StartsWith("gpg: Can't check signature: public key not found"))
                    return null;

                if (line.StartsWith("gpg: BAD signature from ") ||
                    line.StartsWith("gpg: WARNING:") ||
                    (line.StartsWith("gpg: renaming ") && line.EndsWith("failed: Permission denied")))
                {
                    Log.Warn(line);
                    return null;
                }

                if (line.StartsWith("gpg: waiting for lock") ||
                    (line.StartsWith("gpg: keyring ") && line.EndsWith(" created")) ||
                    (line.StartsWith("gpg: ") && line.EndsWith(": trustdb created")))
                {
                    Log.Info(line);
                    return null;
                }

                if (line.StartsWith("gpg: skipped ") && line.EndsWith(": bad passphrase")) throw new WrongPassphraseException();
                if (line.StartsWith("gpg: signing failed: bad passphrase")) throw new WrongPassphraseException();
                if (line.StartsWith("gpg: signing failed: file exists")) throw new IOException(Resources.SignatureAldreadyExists);
                if (line.StartsWith("gpg: signing failed: ") ||
                    line.StartsWith("gpg: error") ||
                    line.StartsWith("gpg: critical"))
                    throw new IOException(line);

                // Unknown GnuPG message
                Log.Warn(line);
                return null;
            }
        }

        /// <summary>
        /// Manages the interaction with the command-line interface of the external process. Writes a data array to stdin.
        /// </summary>
        private class CliControlData : CliControl
        {
            private readonly byte[] _stdinData;

            public CliControlData(byte[] stdinData)
            {
                _stdinData = stdinData;
            }

            protected override void InitStdin(StreamWriter writer)
            {
                #region Sanity checks
                if (writer == null) throw new ArgumentNullException("writer");
                #endregion

                writer.BaseStream.Write(_stdinData, 0, _stdinData.Length);
                writer.Close();
            }
        }

        /// <summary>
        /// Manages the interaction with the command-line interface of the external process. Writes a text-line and a stream to stdin.
        /// </summary>
        private class CliControlStream : CliControl
        {
            private readonly string _stdinLine;
            private readonly Stream _stdinStream;

            public CliControlStream(string stdinLine, Stream stdinStream)
            {
                _stdinLine = stdinLine;
                _stdinStream = stdinStream;
            }

            protected override void InitStdin(StreamWriter writer)
            {
                #region Sanity checks
                if (writer == null) throw new ArgumentNullException("writer");
                #endregion

                writer.WriteLine(_stdinLine ?? "");
                _stdinStream.CopyTo(writer.BaseStream);
                writer.Close();
            }
        }
    }
}
