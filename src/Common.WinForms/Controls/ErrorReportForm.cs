/*
 * Copyright 2006-2014 Bastian Eicher
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
using NanoByte.Common.Info;
using NanoByte.Common.Properties;
using NanoByte.Common.Storage;

namespace NanoByte.Common.Controls
{
    /// <summary>
    /// Presents the user with a friendly interface in case of an error, offering to report it to the developers.
    /// </summary>
    /// <remarks>This class should only be used by <see cref="System.Windows.Forms"/> applications.</remarks>
    public sealed partial class ErrorReportForm : Form
    {
        #region Variables
        private readonly Exception _exception;
        private readonly Uri _uploadUri;
        #endregion

        #region Constructor
        /// <summary>
        /// Prepares reporting an error.
        /// </summary>
        /// <param name="exception">The exception object describing the error.</param>
        /// <param name="uploadUri">The URI to upload error reports to.</param>
        private ErrorReportForm(Exception exception, Uri uploadUri)
        {
            #region Sanity checks
            if (exception == null) throw new ArgumentNullException("exception");
            if (uploadUri == null) throw new ArgumentNullException("uploadUri");
            #endregion

            InitializeComponent();

            _exception = exception;

            // A missing file as the root is more important than the secondary exceptions it causes
            if (exception.InnerException is FileNotFoundException)
                exception = exception.InnerException;

            // Make the message simpler for missing files
            detailsBox.Text = (exception is FileNotFoundException) ? exception.Message.Replace("\n", Environment.NewLine) : exception.ToString();

            // Append inner exceptions
            if (exception.InnerException != null)
                detailsBox.Text += Environment.NewLine + Environment.NewLine + exception.InnerException;

            _uploadUri = uploadUri;
        }
        #endregion

        #region Static access
        private static readonly object _monitoringLock = new object();

        /// <summary>
        /// Sets up hooks that catch and report any unhandled exceptions. Calling this more than once has no effect.
        /// </summary>
        /// <param name="uploadUri">The URI to upload error reports to.</param>
        /// <remarks>If an exception is caught any remaining threads will continue to execute until the error has been reported. Then the entire process will be terminated.</remarks>
        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes", Justification = "If the actual exception is unknown the generic top-level Exception is the most appropriate")]
        [Conditional("ERROR_REPORT")]
        public static void SetupMonitoring(Uri uploadUri)
        {
            #region Sanity checks
            if (uploadUri == null) throw new ArgumentNullException("uploadUri");
            #endregion

            // Only execute this code once per process
            if (!Monitor.TryEnter(_monitoringLock)) return;

            AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e)
            {
                Log.Error("AppDomain.CurrentDomain.UnhandledException raised");

                HideForms();
                Report((e.ExceptionObject as Exception) ?? new Exception("Unknown error"), uploadUri);
                Process.GetCurrentProcess().Kill();
            };
        }

        /// <summary>
        /// Prevent any further user interaction with the crashing application
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We are already handling unexpected exceptions. We need to ignore any additional problems in the cleanup process.")]
        private static void HideForms()
        {
            foreach (Form form in Application.OpenForms)
            {
                try
                {
                    form.Invoke(new Action(form.Hide));
                }
                    // ReSharper disable once EmptyGeneralCatchClause
                catch
                {}
            }
        }

        /// <summary>
        /// Displays the error reporting form.
        /// </summary>
        /// <param name="ex">The exception to report.</param>
        /// <param name="uploadUri">The URI to upload error reports to.</param>
        /// <remarks>Modal to all windows on the current thread. Creates a new message loop if none exists.</remarks>
        public static void Report(Exception ex, Uri uploadUri)
        {
            var form = new ErrorReportForm(ex, uploadUri);
            if (Application.MessageLoop) form.ShowDialog();
            else Application.Run(form);
        }
        #endregion

        //--------------------//

        #region Buttons
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "A final exception handler may never throw exceptions itself")]
        private void buttonReport_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            commentBox.Enabled = detailsBox.Enabled = buttonReport.Enabled = buttonCancel.Enabled = false;

            // Create WebClient for upload and register as component for automatic disposal
            var webClient = new WebClient();
            if (components == null) components = new Container();
            components.Add(webClient);

            webClient.UploadFileCompleted += delegate(object uploadSender, UploadFileCompletedEventArgs uploadEventArgs)
            {
                Cursor = Cursors.Default;
                if (uploadEventArgs.Error == null)
                {
                    Msg.Inform(this, Resources.ErrorReportSent, MsgSeverity.Info);
                    Close();
                }
                else
                {
                    Msg.Inform(this, Resources.UnableToGenerateErrorReportFile + "\n" + uploadEventArgs.Error.Message, MsgSeverity.Error);
                    commentBox.Enabled = detailsBox.Enabled = buttonReport.Enabled = buttonCancel.Enabled = true;
                }
            };
            webClient.UploadFileAsync(_uploadUri, GenerateReportFile());
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion

        #region Generate report
        /// <summary>
        /// Generates a ZIP archive containing crash information.
        /// </summary>
        /// <returns></returns>
        private string GenerateReportFile()
        {
            var crashInfo = new ErrorReport
            {
                Application = AppInfo.Current,
                OS = OSInfo.Current,
                Exception = new ExceptionInfo(_exception),
                Log = Log.Content,
                Comments = commentBox.Text
            };

            string reportPath = Path.Combine(Path.GetTempPath(), Application.ProductName + " Error Report.zip");
            if (File.Exists(reportPath)) File.Delete(reportPath);
            crashInfo.SaveXmlZip(reportPath);
            return reportPath;
        }
        #endregion
    }
}
