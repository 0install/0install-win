using System;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace ZeroInstall
{
    class Program
    {
        static void Main(string[] args)
        {
            // Determine where this EXE file is located
            string basePath = Path.GetDirectoryName(Application.ExecutablePath);

            // Find the Python interpreter
            string pythonExe = basePath + "\\python\\pythonw.exe";
            if (!File.Exists(pythonExe))
            {
                MessageBox.Show("Unable to find " + pythonExe, "Zero Install", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }

            // Find the Python script
            string pythonScript = basePath + "\\python\\Scripts\\0store";
            if (!File.Exists(pythonScript))
            {
                MessageBox.Show("Unable to find " + pythonScript, "Zero Install", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }

            // Find GnuPG
            string gnuPgEXE = basePath + "\\gnupg\\gpg.exe";
            if (!File.Exists(gnuPgEXE))
            {
                MessageBox.Show("Unable to find " + gnuPgEXE, "Zero Install", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }

            // Find GTK+
            string gtkDll = basePath + "\\gtk\\bin\\libgtk-win32-2.0-0.dll";
            if (!File.Exists(gtkDll))
            {
                MessageBox.Show("Unable to find " + gtkDll, "Zero Install", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }

            // Modify PATH for the child process (add bundled Python, GnuPG and GTK+ distributions)
            Environment.SetEnvironmentVariable("PATH", basePath + ";" + basePath + "\\python;" + basePath + "\\gnupg;" + basePath + "\\gtk\\bin;" + Environment.GetEnvironmentVariable("PATH"));

            // The arguments array is ignored

            // Prepare to run the Python script and force the GUI (required since there is no console output)
            ProcessStartInfo startInfo = new ProcessStartInfo(pythonExe, '"' + pythonScript + '"' + " manage");

            // Start the Python process and wait until it exits
            Process process = new Process();
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
        }
    }
}
