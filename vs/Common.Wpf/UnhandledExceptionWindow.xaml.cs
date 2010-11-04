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
    public partial class UnhandledExceptionWindow : Window
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

            this.UnhandledException = unhandledException;

            this.UnhandledExceptionInformation = new ExceptionInformation(this.UnhandledException);

            // A missing file as the root is more important than the secondary exceptions it causes
            if (this.UnhandledException.InnerException != null && this.UnhandledException.InnerException is FileNotFoundException)
                this.UnhandledException = this.UnhandledException.InnerException;

            // Make the message simpler for missing files
            String technicalDetails = (this.UnhandledException is FileNotFoundException) ? this.UnhandledException.Message.Replace("\n", "\r\n") : this.UnhandledException.ToString();

            // Append inner exceptions
            if (this.UnhandledException.InnerException != null)
                technicalDetails += "\r\n\r\n" + this.UnhandledException.InnerException;

            this.tbTechnicalDetails.Text = technicalDetails;
        }
        #endregion

        #region Button: Report
        private void bReport_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	// Create report file with error details and comments
            String path = GenerateReportFile();

            // Upload report file
            //NanoGrid.Upload(path);

            // Restart application
            Restart();
        }
        #endregion

        #region Button: Do not report
        private void bDoNotReport_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Restart application
            Restart();
        }
        #endregion

        #region Restart
        private void Restart()
        {
            // TODO: Restart zero install.

            this.Close();
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
                writer.Write(this.tbTechnicalDetails.Text);
                writer.Flush();
                zipStream.CloseEntry();

                // Store the exception information as XML
                zipStream.PutNextEntry(new ZipEntry("Exception.xml"));
                XmlStorage.Save(zipStream, this.UnhandledExceptionInformation);
                zipStream.CloseEntry();

                if (!string.IsNullOrEmpty(this.tbComment.Text))
                {
                    // Store the user comment
                    zipStream.PutNextEntry(new ZipEntry("Comment.txt"));
                    writer.Write(this.tbComment.Text);
                    zipStream.CloseEntry();
                }
            }
            return reportPath;
        }
        #endregion
    }
}
