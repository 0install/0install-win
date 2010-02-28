using System;
using System.IO;
using System.Windows.Forms;
using Common.Storage;
using ICSharpCode.SharpZipLib.Zip;

namespace Common.Controls
{
    /// <summary>
    /// Presents the user with a friendly interface in case of an error, offering to report it to the developers.
    /// </summary>
    public partial class ErrorReportForm : Form
    {
        #region Variables
        private readonly Action<string> _callback;
        private readonly ExceptionInformation _exceptionInformation;
        #endregion

        #region Constructor
        /// <summary>
        /// Prepares reporting an error.
        /// </summary>
        /// <param name="ex">The exception object describing the error.</param>
        /// <param name="callback">A delegate that is called when the user decides to report the error with the path of the file with the report information.</param>
        public ErrorReportForm(Exception ex, Action<string> callback)
        {
            #region Sanity checks
            if (ex == null) throw new ArgumentNullException("ex");
            if (callback == null) throw new ArgumentNullException("callback");
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

            _callback = callback;
        }
        #endregion

        //--------------------//

        #region Report
        private void buttonReport_Click(object sender, EventArgs e)
        {
            string reportPath = Path.Combine(Path.GetTempPath(), Application.ProductName + " Error Report.zip");
            if (File.Exists(reportPath)) File.Delete(reportPath);
            Stream reportStream = File.Create(reportPath);

            using (var zipStream = new ZipOutputStream(reportStream))
            {
                zipStream.SetLevel(9);

                using (var writer = new StreamWriter(zipStream))
                {
                    writer.AutoFlush = true;

                    // Store the log file
                    zipStream.PutNextEntry(new ZipEntry("Log.txt"));
                    writer.Write(Log.Content);
                    zipStream.CloseEntry();

                    // Store the exception information as TXT
                    zipStream.PutNextEntry(new ZipEntry("Exception.txt"));
                    writer.Write(detailsBox.Text);
                    zipStream.CloseEntry();

                    // Store the exception information as XML
                    zipStream.PutNextEntry(new ZipEntry("Exception.xml"));
                    XmlStorage.Save(zipStream, _exceptionInformation);
                    zipStream.CloseEntry();

                    if (!string.IsNullOrEmpty(commentBox.Text))
                    {
                        // Store the user comment
                        zipStream.PutNextEntry(new ZipEntry("Comment.txt"));
                        writer.Write(commentBox.Text);
                        zipStream.CloseEntry();
                    }
                }
            }
            _callback(reportPath);

            Close();
        }
        #endregion

        #region Misc
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion
    }
}