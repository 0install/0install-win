/*
 * Copyright 2010 Dennis Keil
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
using System.Windows;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using Common.Storage;

namespace Common.Wpf
{
    /// <summary>
    /// Interaktionslogik für UnhandledExceptionWindow.xaml
    /// </summary>
    public partial class UnhandledExceptionWindow
    {
        #region Instance Members / Properties

        Exception UnhandledException { get; set; }
        ExceptionInformation UnhandledExceptionInformation { get; set; }

        #endregion

        #region Constructors
        public UnhandledExceptionWindow()
        {
            InitializeComponent();
        }

        public UnhandledExceptionWindow(Exception unhandledException)
        {
            InitializeComponent();

            UnhandledException = unhandledException;

            UnhandledExceptionInformation = new ExceptionInformation(UnhandledException);

            // A missing file as the root is more important than the secondary exceptions it causes
            if (UnhandledException.InnerException != null && UnhandledException.InnerException is FileNotFoundException)
                UnhandledException = UnhandledException.InnerException;

            // Make the message simpler for missing files
            string technicalDetails = (UnhandledException is FileNotFoundException) ? UnhandledException.Message.Replace("\n", "\r\n") : UnhandledException.ToString();

            // Append inner exceptions
            if (UnhandledException.InnerException != null)
                technicalDetails += "\r\n\r\n" + UnhandledException.InnerException;

            tbTechnicalDetails.Text = technicalDetails;
        }
        #endregion

        #region Button: Report
        private void bReport_Click(object sender, RoutedEventArgs e)
        {
        	// Create report file with error details and comments
            //string path = GenerateReportFile();

            // Upload report file
            //NanoGrid.Upload(path);

            // Restart application
            Restart();
        }
        #endregion

        #region Button: Do not report
        private void bDoNotReport_Click(object sender, RoutedEventArgs e)
        {
            // Restart application
            Restart();
        }
        #endregion

        #region Restart
        private void Restart()
        {
            // TODO: Restart zero install.

            Close();
        }
        #endregion

        #region Generate report
        /// <summary>
        /// Generates a ZIP archive containing the log file, exception information and any user comments.
        /// </summary>
        /// <returns></returns>
        private string GenerateReportFile()
        {
            // INFO: Application.ProductName in WPF -> Application.Current.MainWindow.GetType().Assembly.GetName().Name
            string reportPath = Path.Combine(Path.GetTempPath(), Application.Current.MainWindow.GetType().Assembly.GetName().Name + " Error Report.zip");
            if (File.Exists(reportPath)) File.Delete(reportPath);
            Stream reportStream = File.Create(reportPath);

            using (var zipStream = new ZipOutputStream(reportStream))
            {
                zipStream.SetLevel(9);

                var writer = new StreamWriter(zipStream);

                // Store the log file
                zipStream.PutNextEntry(new ZipEntry("Log.txt"));
                writer.Write(Log.Content);
                writer.Flush();
                zipStream.CloseEntry();

                // Store the exception information as TXT
                zipStream.PutNextEntry(new ZipEntry("Exception.txt"));
                writer.Write(tbTechnicalDetails.Text);
                writer.Flush();
                zipStream.CloseEntry();

                // Store the exception information as XML
                zipStream.PutNextEntry(new ZipEntry("Exception.xml"));
                XmlStorage.Save(zipStream, UnhandledExceptionInformation);
                zipStream.CloseEntry();

                if (!string.IsNullOrEmpty(tbComment.Text))
                {
                    // Store the user comment
                    zipStream.PutNextEntry(new ZipEntry("Comment.txt"));
                    writer.Write(tbComment.Text);
                    zipStream.CloseEntry();
                }
            }
            return reportPath;
        }
        #endregion
    }
}
