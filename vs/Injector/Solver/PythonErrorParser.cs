/*
 * Copyright 2010 Bastian Eicher
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
using Common;

namespace ZeroInstall.Injector.Solver
{
    /// <summary>
    /// Helper class for <see cref="PythonSolver"/> for parsing <see cref="Process.StandardError"/> data.
    /// </summary>
    internal class PythonErrorParser
    {
        #region Enumerations
        private enum ErrorMode
        {
            None, Info, Warn, Error, Critical, Question
        }
        #endregion

        #region Variables
        private readonly Action<string> _questionCallback;
        private readonly Action<string> _errorCallback;

        private StringBuilder _cache;
        private ErrorMode _currentErrorMode;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new error parser.
        /// </summary>
        /// <param name="questionCallback">The callback to use to ask the user questions.</param>
        /// <param name="errorCallback">The callback to use to report errors while solving.</param>
        public PythonErrorParser(Action<string> questionCallback, Action<string> errorCallback)
        {
            _questionCallback = questionCallback;
            _errorCallback = errorCallback;
        }
        #endregion

        //--------------------//

        #region Handle line
        /// <summary>
        /// To be called for every line of data received from <see cref="Process.StandardError"/>.
        /// </summary>
        /// <param name="line">The line of data from <see cref="Process.StandardError"/>.</param>
        public void HandleStdErrorLine(string line)
        {
            if (string.IsNullOrEmpty(line)) return;

            var lineMode = IdentifyErrorMode(ref line);

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
                            _questionCallback(_cache.ToString());
                            _currentErrorMode = ErrorMode.None;
                        }
                        else _cache.AppendLine(line);
                    }
                    else if (lineMode >= ErrorMode.Info && lineMode <= ErrorMode.Critical)
                        FlushMessage(_currentErrorMode, line);
                    else
                        throw new ArgumentException("Question within question is invalid", "line");
                    break;
            }
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
                    throw new ArgumentException("Unfinished question is invalid");
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

        private void FlushMessage(ErrorMode currentErrorMode, string message)
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
                    Log.Error(message);
                    if (_errorCallback != null) _errorCallback(message);
                    break;
            }
        }
        #endregion
    }
}
