/*
 * Copyright 2006-2010 Bastian Eicher
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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Common.Helpers;

namespace Common
{
    /// <summary>
    /// Maintains log messages in memory and in a plain text file.
    /// </summary>
    public static class Log
    {
        #region Events
        /// <summary>
        /// Occurs whenever a new entry has been added to the log.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        public static event SimpleEventHandler NewEntry;
        #endregion

        #region Variables
        private static readonly StringBuilder _sessionContent = new StringBuilder();
        private static readonly StreamWriter _fileWriter;
        #endregion

        #region Properties
        /// <summary>
        /// All data logged in this session so far as plain text.
        /// </summary>
        public static string Content
        {
            get
            {
                lock (_sessionContent) { return _sessionContent.ToString(); }
            }
        }
        #endregion

        #region Constructor
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "The static constructor is used to add an identification header to the log file")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Any kind of problems writing the log file should be ignored")]
        static Log()
        {
            string filePath;
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:
                case PlatformID.MacOSX:
                    // ToDo: Write to sensible log directory

                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                default:
                    filePath = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Application.ExecutablePath) + " Log.txt");
                    break;
            }

            // Try to open the file for writing but give up right away if there are any problems
            FileStream file;
            try { file = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite); }
            catch { return; }

            // Check if the log file has exceeded 1MB
            if (file.Length > 1024 * 1024)
            {
                // In this case we just kill it and create a new one
                file.Close();
                file = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            }

            // When writing to a new file use UTF-8, otherwise keep existing encoding
            _fileWriter = (file.Length == 0 ? new StreamWriter(file, Encoding.UTF8) : new StreamWriter(file));
            _fileWriter.AutoFlush = true;

            // Go to end of file
            _fileWriter.BaseStream.Seek(0, SeekOrigin.End);

            // Add session identification block to the file
            Echo("");
            Echo("/// " + Application.ProductName + " v" + Application.ProductVersion);
            Echo("///  Log session started at: " + DateTime.Now.ToString(CultureInfo.InvariantCulture));
            Echo("");
        }
        #endregion

        //--------------------//

        #region Add entry
        private enum LogSeverity { Echo, Info, Warn, Error }

        /// <summary>
        /// Adds a new entry to the log.
        /// </summary>
        /// <param name="severity">The type/severity of the entry.</param>
        /// <param name="message">The actual message text of the entry.</param>
        private static void AddEntry(LogSeverity severity, string message)
        {
            #region Sanity checks
            if (message == null) throw new ArgumentNullException("message");
            #endregion

            #region Prepare message string
            // Create uniform line-breaks and indention
            string[] lines = StringHelper.SplitMultilineText(message.Trim());
            message = StringHelper.Concatenate(lines, "\r\n\t");

            switch(severity)
            {
                case LogSeverity.Info:
                    // Prepend current time to message
                    message = string.Format(CultureInfo.InvariantCulture, "[{0:T}] {1}", DateTime.Now, message);
                    break;

                case LogSeverity.Warn:
                    // Prepend current time and severity to message
                    message = string.Format(CultureInfo.InvariantCulture, "[{0:T}] WARN: {1}", DateTime.Now, message);
                    break;

                case LogSeverity.Error:
                    // Prepend current time and severity to message
                    message = string.Format(CultureInfo.InvariantCulture, "[{0:T}] ERROR: {1}", DateTime.Now, message);
                    break;

            }
            #endregion

            // Thread-safety: Only one log message is handled at a time
            lock (_sessionContent)
            {
#if DEBUG
                // In debug mode write the message to the console
                Console.WriteLine(message);
#endif

                // Store the message in RAM
                _sessionContent.AppendLine(message);

                // If possible write to the log file
                if (_fileWriter != null) _fileWriter.WriteLine(message);
            }

            // Raise event
            if (NewEntry != null) NewEntry();
        }
        #endregion

        #region Access
        /// <summary>
        /// Writes non-critical information to the log.
        /// </summary>
        //[LuaGlobal(Name = "Info", Description = "Writes non-critical information to the log.")]
        public static void Info(string message)
        {
            AddEntry(LogSeverity.Info, message);
        }

        /// <summary>
        /// Writes non-critical warnings to the log.
        /// </summary>
        //[LuaGlobal(Name = "Warn", Description = "Writes non-critical warnings to the log.")]
        public static void Warn(string message)
        {
            AddEntry(LogSeverity.Warn, message);
        }

        /// <summary>
        /// Writes critical errors to the log.
        /// </summary>
        //[LuaGlobal(Name = "Error", Description = "Writes critical errors to the log.")]
        public static void Error(string message)
        {
            AddEntry(LogSeverity.Error, message);
        }

        /// <summary>
        /// Echos to the log without a timestamp or severity.
        /// </summary>
        public static void Echo(string message)
        {
            AddEntry(LogSeverity.Echo, message);
        }
        #endregion
    }
}