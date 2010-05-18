using System;
using System.IO;
using System.Diagnostics;
using System.Text;

namespace ZeroInstall
{
    class Program
    {
        static void Main(string[] args)
        {
            // Determine where this EXE file is located
            string basePath = AppDomain.CurrentDomain.BaseDirectory;

            // Find the Python interpreter
            string pythonExe = basePath + "\\python\\python.exe";
            if (!File.Exists(pythonExe))
            {
                Console.Error.WriteLine("Unable to find " + pythonExe);
                Environment.Exit(1);
            }

            // Find the Python script
            string pythonScript = basePath + "\\python\\Scripts\\0launch";
            if (!File.Exists(pythonScript))
            {
                Console.Error.WriteLine("Unable to find " + pythonScript);
                Environment.Exit(1);
            }

            // Find GnuPG
            string gnuPgEXE = basePath + "\\gnupg\\gpg.exe";
            if (!File.Exists(gnuPgEXE))
            {
                Console.Error.WriteLine("Unable to find " + gnuPgEXE);
                Environment.Exit(1);
            }

            // Find GTK+
            string gtkDll = basePath + "\\gtk\\bin\\libgtk-win32-2.0-0.dll";
            if (!File.Exists(gtkDll))
            {
                Console.Error.WriteLine("Unable to find " + gtkDll);
                Environment.Exit(1);
            }

            // Modify PATH for the child process (add bundled Python, GnuPG and GTK+ distributions)
            Environment.SetEnvironmentVariable("PATH", basePath + ";" + basePath + "\\python;" + basePath + "\\gnupg;" + basePath + "\\gtk\\bin;" + Environment.GetEnvironmentVariable("PATH"));

            // Convert arguments array to a single string
            StringBuilder argsString = new StringBuilder();
            for (int i = 0; i < args.Length; i++)
            {
                argsString.Append(" ");
                argsString.Append(args[i]);
            }

            // Prepare to run the Python script and block the GUI (otherwise there would never be a console-mode) 
            ProcessStartInfo startInfo = new ProcessStartInfo(pythonExe, '"' + pythonScript + "\" --console" + argsString);
            
            // Run in the same console window
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = false;

            // Start the Python process and wait until it exits
            Process process = new Process();
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
        }
    }
}
