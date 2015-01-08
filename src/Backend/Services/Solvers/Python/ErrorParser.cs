/*
 * Copyright 2010-2014 Bastian Eicher
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
using System.Text;
using System.Web;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Tasks;
using ZeroInstall.Services.Properties;

namespace ZeroInstall.Services.Solvers.Python
{
    /// <summary>
    /// Helper class for <see cref="PythonSolver"/> for parsing <see cref="Process.StandardError"/> data.
    /// </summary>
    internal class ErrorParser
    {
        #region Enumerations
        private enum ErrorMode
        {
            None,
            Info,
            Warn,
            Error,
            Critical,
            Question
        }
        #endregion

        #region Variables
        private readonly ITaskHandler _handler;

        private StringBuilder _cache;
        private ErrorMode _currentErrorMode;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new error parser.
        /// </summary>
        /// <param name="handler">A callback object used if the the user needs to be asked any questions (such as whether to trust a certain GPG key).</param>
        public ErrorParser(ITaskHandler handler)
        {
            _handler = handler;
        }
        #endregion

        //--------------------//

        #region Handle line
        /// <summary>
        /// To be called for every line of error data received from the external solver.
        /// </summary>
        /// <param name="line">The error line written to stderr.</param>
        /// <returns>The response to write to stdin; <see langword="null"/> for none.</returns>
        [CanBeNull]
        public string HandleStdErrorLine([NotNull] string line)
        {
            #region Sanity checks
            if (line == null) throw new ArgumentNullException("line");
            #endregion

            _handler.CancellationToken.ThrowIfCancellationRequested();

            var lineMode = IdentifyErrorMode(ref line);

            // Restore non-ASCII characters
            line = HttpUtility.HtmlDecode(line);

            switch (_currentErrorMode)
            {
                case ErrorMode.None:
                    _currentErrorMode = lineMode;
                    _cache = new StringBuilder();
                    if (!string.IsNullOrEmpty(line)) _cache.AppendLine(line);
                    break;

                case ErrorMode.Info:
                case ErrorMode.Warn:
                case ErrorMode.Error:
                case ErrorMode.Critical:
                    if (lineMode == ErrorMode.None) _cache.AppendLine(line);
                    else
                    {
                        FlushMessage(_currentErrorMode, _cache.ToString());

                        _currentErrorMode = lineMode;
                        _cache = new StringBuilder();
                        _cache.AppendLine(line);
                    }
                    break;

                case ErrorMode.Question:
                    if (lineMode == ErrorMode.None)
                    {
                        if (line.Contains("[Y/N]") && _currentErrorMode == ErrorMode.Question)
                        {
                            _currentErrorMode = ErrorMode.None;
                            return _handler.AskQuestion(_cache.ToString(), batchInformation: Resources.UntrustedKeys) ? "Y" : "N";
                        }
                        _cache.AppendLine(line);
                    }
                    else if (lineMode >= ErrorMode.Info && lineMode <= ErrorMode.Critical)
                        FlushMessage(_currentErrorMode, line);
                    else
                        throw new ArgumentException(@"Question within question is invalid", "line");
                    break;
            }

            return null;
        }

        /// <summary>
        /// Handle any messages that are still pending.
        /// </summary>
        public void Flush()
        {
            switch (_currentErrorMode)
            {
                case ErrorMode.Info:
                case ErrorMode.Warn:
                case ErrorMode.Error:
                case ErrorMode.Critical:
                    FlushMessage(_currentErrorMode, _cache.ToString());
                    break;

                case ErrorMode.Question:
                    throw new ArgumentException("Unfinished question is invalid. Remaining data in cache:\n" + _cache);
            }
        }
        #endregion

        #region Helpers
        private static ErrorMode IdentifyErrorMode(ref string line)
        {
            if (ProbeErrorMode(ref line, "INFO:")) return ErrorMode.Info;
            if (ProbeErrorMode(ref line, "WARNING:")) return ErrorMode.Warn;
            if (ProbeErrorMode(ref line, "ERROR:")) return ErrorMode.Error;
            if (ProbeErrorMode(ref line, "CRITICAL:")) return ErrorMode.Critical;
            if (ProbeErrorMode(ref line, "QUESTION:")) return ErrorMode.Question;
            if (line.StartsWith("Traceback")) return ErrorMode.Critical;
            return ErrorMode.None;
        }

        private static bool ProbeErrorMode(ref string line, string prefix)
        {
            if (line.StartsWith(prefix))
            {
                line = line.Substring(prefix.Length);
                return true;
            }
            return false;
        }

        private static void FlushMessage(ErrorMode currentErrorMode, string message)
        {
            switch (currentErrorMode)
            {
                case ErrorMode.Info:
                    Log.Info(message);
                    break;

                case ErrorMode.Warn:
                    Log.Warn(message);
                    break;

                case ErrorMode.Error:
                case ErrorMode.Critical:
                    throw new SolverException(message.Trim());
            }
        }
        #endregion
    }
}
