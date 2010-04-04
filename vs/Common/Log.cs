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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Common.Helpers;
using LuaInterface;

namespace Common
{
    /// <summary>
    /// Static functions for writing and maintaining a log file.
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
        private static readonly StreamWriter _writer;
        private static readonly string _filePath = Path.Combine(Path.GetTempPath(),
            Path.GetFileNameWithoutExtension(Application.ExecutablePath) + " Log.txt");
        private static readonly StringBuilder _logContent = new StringBuilder();
        #endregion

        #region Properties
        /// <summary>
        /// All data logged in this session so far.
        /// </summary>
        public static string Content { get { return _logContent.ToString(); } }
        #endregion

        #region Constructor
        /// <summary>
        /// Returns an ISO Date (Year-Month-Day.
        /// </summary>
        private static string IsoDate(DateTime date)
        {
            return date.Year + "-" + date.Month.ToString("00", CultureInfo.InvariantCulture) + "-" + date.Day.ToString("00", CultureInfo.InvariantCulture);
        }

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "The static constructor is used to add an identification header to the log file")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Any kind of problems writing the log file should be ignored")]
        static Log()
        {
            #region Setup file writer
            // Try to open the file for writing but give up right away if there are any problems
            FileStream file;
            try { file = new FileStream(_filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite); }
            catch { return; }

            // Check if file is too big
            if (file.Length > 1024 * 1024)
            {
                // In this case we just kill it and create a new one
                file.Close();
                file = new FileStream(_filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            }

            // When writing to a new file make sure it uses UTF-8
            _writer = file.Length == 0 ? new StreamWriter(file, Encoding.UTF8) : new StreamWriter(file) { AutoFlush = true };

            // Go to end of file
            _writer.BaseStream.Seek(0, SeekOrigin.End);
            #endregion

            // Add a section identification block to the file
            AddLine("");
            AddLine("/// " + Application.ProductName + " v" + Application.ProductVersion);
            AddLine("/// Session started at: " + IsoDate(DateTime.Now));
            AddLine("");
        }
        #endregion

        #region Write log entry
        private static void AddLine(string text)
        {
            // Split lines and put them back together in order to create uniform line-breaks and indention
            string[] lines = StringHelper.SplitMultilineText(text.Trim());
            text = StringHelper.BuildStringFromLines(lines, "\r\n\t");

            // Make thread-safe
            lock (_logContent)
            {
#if DEBUG
                // In debug mode write the message to the console
                Console.WriteLine(text);
#endif

                // Store the message in RAM
                _logContent.AppendLine(text);

                // If possible write to the log file
                if (_writer != null) _writer.WriteLine(text);
            }
        }

        /// <summary>
        /// Writes a entry and the current time to the log file.
        /// </summary>
        public static void Write(string entry)
        {
            if (string.IsNullOrEmpty(entry)) return;

            DateTime ct = DateTime.Now;
            entry = "[" + ct.Hour.ToString("00", CultureInfo.InvariantCulture) + ":" +
                    ct.Minute.ToString("00", CultureInfo.InvariantCulture) + ":" +
                    ct.Second.ToString("00", CultureInfo.InvariantCulture) + "] " +
                    entry;

            AddLine(entry);

            // Raise event
            if (NewEntry != null) NewEntry();
        }

        /// <summary>
        /// Writes a entry and the current time to the log file.
        /// </summary>
        [LuaGlobal(Name = "Log", Description = "Writes a entry and the current time to the log file.")]
        public static void Write(object entry)
        {
            if (entry != null) Write(entry.ToString());
        }
        #endregion
    }

    /// <summary>
    /// Struct that allows you to log timed execution blocks.
    /// </summary>
    /// <example>
    ///   <code>using(new LogEvent("Message")) {}</code>
    /// </example>
    public struct LogEvent : IDisposable
    {
        #region Variables
        private readonly Stopwatch _timer;
        private readonly string _entry;
        #endregion

        #region Event control
        /// <summary>
        /// Starts a new log event.
        /// </summary>
        /// <param name="entry">The entry for the log file. Elapsed time will automatically be appended.</param>
        public LogEvent(string entry)
        {
            _entry = entry;
            _timer = Stopwatch.StartNew();
        }

        /// <summary>
        /// Ends the log event.
        /// </summary>
        public void Dispose()
        {
            _timer.Stop();
            Log.Write(_entry + " => " + (float)_timer.Elapsed.TotalSeconds + "s");
        }
        #endregion
    }
}