using System;
using System.IO;
using System.Windows.Forms;

namespace ZeroInstall.FeedEditor
{
    static class Program
    {
        #region Properties
        /// <summary>
        /// The directory where the executable file is located.
        /// </summary>
        public static string AppDir
        {
            get { return Path.GetDirectoryName(Application.ExecutablePath); }
        }
        #endregion

        //--------------------//

        #region Startup
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
        #endregion
    }
}
