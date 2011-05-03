/*
 * Copyright 2006-2011 Bastian Eicher
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
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using Common.Properties;
using Common.Storage;
using ICSharpCode.SharpZipLib.Zip;

namespace Common.Controls
{
    /// <summary>
    /// Presents the user with a friendly interface in case of an error, offering to report it to the developers.
    /// </summary>
    /// <remarks>This class should only be used by <see cref="System.Windows.Forms"/> applications.</remarks>
    public sealed partial class ErrorReportForm : Form
    {
        #region Variables
        private readonly Uri _uploadUri;
        private readonly ExceptionInformation _exceptionInformation;
        #endregion

        #region Constructor
        /// <summary>
        /// Prepares reporting an error.
        /// </summary>
        /// <param name="ex">The exception object describing the error.</param>
        /// <param name="uploadUri">The URI to upload error reports to.</param>
        private ErrorReportForm(Exception ex, Uri uploadUri)
        {
            #region Sanity checks
            if (ex == null) throw new ArgumentNullException("ex");
            if (uploadUri == null) throw new ArgumentNullException("uploadUri");
            #endregion

            InitializeComponent();

            _exceptionInformation = new ExceptionInformation(ex);

            // A missing file as the root is more important than the secondary exceptions it causes
            if (ex.InnerException != null && ex.InnerException is FileNotFoundException)
                ex = ex.InnerException;

            // Make the message simpler for missing files
            detailsBox.Text = (ex is FileNotFoundException) ? ex.Message.Replace("\n", "\r\n") : ex.ToString();

            // Append inner exceptions
            if (ex.InnerException != null)
                detailsBox.Text += "\r\n\r\n" + ex.InnerException;

            _uploadUri = uploadUri;
        }
        #endregion

        #region Static access
        /// <summary>
        /// Runs a delegate, catching and reporting any unhandled exceptions that occur inside.
        /// </summary>
        /// <param name="run">The delegate to run.</param>
        /// <param name="uploadUri">The URI to upload error reports to.</param>
        /// <returns><see langword="true"/> if <paramref name="run"/> was executed successfully; <see langword="false"/> if an exception was caught.</returns>
        /// <remarks>
        ///   <para>
        ///     If an exception is caught on a <see cref="System.Windows.Forms"/> thread any remaining <see cref="Form"/>s are closed in the hopes this will end <paramref name="run"/>.
        ///     Such exceptions can only be reported once the <see cref="System.Windows.Forms"/> message loop has ended.
        ///   </para>
        ///   <para>
        ///     If an exception is caught on a background thread any remaing <see cref="System.Windows.Forms"/> threads will continue to execute until the error has been reported.
        ///     Then the entire process is terminated.
        ///   </para>
        /// </remarks>
        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes", Justification = "If the actual exception is unknown the generic top-level Exception is the most appropriate")]
        public static bool RunAppMonitored(SimpleEventHandler run, Uri uploadUri)
        {
            #region Sanity checks
            if (run == null) throw new ArgumentNullException("run");
            if (uploadUri == null) throw new ArgumentNullException("uploadUri");
            #endregion

            // Catch exceptions in WinForms threads
            Exception delayedException = null;
            ThreadExceptionEventHandler winFormsHandler = delegate(object sender, ThreadExceptionEventArgs e)
            {
                // Can only report exception after the message loop has terminated
                delayedException = e.Exception;

                // Cause the message loop to end by closing all forms
                while (Application.OpenForms.Count > 0)
                    Application.OpenForms[0].Close();
            };

            // Catch exceptions in background threads
            UnhandledExceptionEventHandler backgroundHandler = delegate(object sender, UnhandledExceptionEventArgs e)
            {
                Report((e.ExceptionObject as Exception) ?? new Exception("Unknown error"), uploadUri);

                // Normally a background exception would only kill a single thread, but we want the whole application to end to be on the safe side
                Process.GetCurrentProcess().Kill();
            };

            // Activate WinForms exception handling
            try { Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException); }
            catch (InvalidOperationException) {}

            Application.ThreadException += winFormsHandler;
            AppDomain.CurrentDomain.UnhandledException += backgroundHandler;
            run();
            AppDomain.CurrentDomain.UnhandledException -= backgroundHandler;
            Application.ThreadException -= winFormsHandler;

            // Report exceptions from WinForms threads
            if (delayedException != null)
            {
                Report(delayedException, uploadUri);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Runs a new message loop to display the error reporting form.
        /// </summary>
        /// <param name="ex">The exception to report.</param>
        /// <param name="uploadUri">The URI to upload error reports to.</param>
        public static void Report(Exception ex, Uri uploadUri)
        {
            Application.Run(new ErrorReportForm(ex, uploadUri));
        }
        #endregion

        //--------------------//

        #region Buttons
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "A final exception handler may never throw exceptions itself")]
        private void buttonReport_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            commentBox.Enabled = detailsBox.Enabled = buttonReport.Enabled = buttonCancel.Enabled = false;
            reportWorker.RunWorkerAsync();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion

        #region Background worker
        private void reportWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            new WebClient().UploadFile(_uploadUri, GenerateReportFile());
        }

        private void reportWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Cursor = Cursors.Default;
            if (e.Error == null)
            {
                Msg.Inform(this, Resources.ErrorReportSent, MsgSeverity.Info);
                Close();
            }
            else
            {
                Msg.Inform(this, Resources.UnableToGenerateErrorReportFile + "\n" + e.Error.Message, MsgSeverity.Error);
                commentBox.Enabled = detailsBox.Enabled = buttonReport.Enabled = buttonCancel.Enabled = true;
            }
        }
        #endregion

        #region Generate report
        /// <summary>
        /// Generates a ZIP archive containing the log file, exception information and any user comments.
        /// </summary>
        /// <returns></returns>
        private string GenerateReportFile()
        {
            string reportPath = Path.Combine(Path.GetTempPath(), Application.ProductName + " Error Report.zip");
            if (File.Exists(reportPath)) File.Delete(reportPath);
            Stream reportStream = File.Create(reportPath);

            using (var zipStream = new ZipOutputStream(reportStream))
            {
                zipStream.SetLevel(9);

                // Store the exception information as XML
                zipStream.PutNextEntry(new ZipEntry("Exception.xml"));
                XmlStorage.Save(zipStream, _exceptionInformation);
                zipStream.CloseEntry();

                var writer = new StreamWriter(zipStream);

                // Store the exception information as TXT
                zipStream.PutNextEntry(new ZipEntry("Exception.txt"));
                writer.Write(detailsBox.Text);
                writer.Flush();
                zipStream.CloseEntry();

                // Store the log file
                zipStream.PutNextEntry(new ZipEntry("Log.txt"));
                writer.Write(Log.Content);
                writer.Flush();
                zipStream.CloseEntry();

                if (!string.IsNullOrEmpty(commentBox.Text))
                {
                    // Store the user comment
                    zipStream.PutNextEntry(new ZipEntry("Comment.txt"));
                    writer.Write(commentBox.Text);
                    writer.Flush();
                    zipStream.CloseEntry();
                }
            }
            return reportPath;
        }
        #endregion
    }
}