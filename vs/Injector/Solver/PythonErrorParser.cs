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
using System.IO;
using System.Text;
using ZeroInstall.Store.Feed;
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
        private readonly IFeedHandler _handler;
        private readonly StreamWriter _standardInput;

        private StringBuilder _cache;
        private ErrorMode _currentErrorMode;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new error parser.
        /// </summary>
        /// <param name="handler">The callback object to use to ask the user questions.</param>
        /// <param name="standardInput">The stream to write user answers to.</param>
        public PythonErrorParser(IFeedHandler handler, StreamWriter standardInput)
        {
            _handler = handler;
            _standardInput = standardInput;
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
                        if (line == "Trust [Y/N] " && _currentErrorMode == ErrorMode.Question)
                        {
                            FlushQuestion(_cache.ToString());
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
            if (ProbeErrorMode(ref line, ErrorMode.Info, "INFO:")) return ErrorMode.Info;
            if (ProbeErrorMode(ref line, ErrorMode.Warn, "WARN:")) return ErrorMode.Warn;
            if (ProbeErrorMode(ref line, ErrorMode.Error, "ERROR:")) return ErrorMode.Error;
            if (ProbeErrorMode(ref line, ErrorMode.Critical, "CRITICAL:")) return ErrorMode.Critical;
            if (ProbeErrorMode(ref line, ErrorMode.Question, "QUESTION:")) return ErrorMode.Question;
            return ErrorMode.None;
        }

        private static bool ProbeErrorMode(ref string line, ErrorMode mode, string prefix)
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
                    Log.Error(message);
                    break;

                case ErrorMode.Critical:
                    throw new Exception(message);
            }
        }

        private void FlushQuestion(string question)
        {
            char answer = _handler.AcceptNewKey(question) ? 'Y' : 'N';
            _standardInput.WriteLine(answer);
            _standardInput.Flush();
        }
        #endregion
    }
}
