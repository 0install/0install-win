/*
 * Copyright 2010-2016 Bastian Eicher
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

/// <summary>
/// This helper executable launches a command-line specified in specific environment variables.
/// <summary>
public class RunEnv
{
    public const int Win32RequestedOperationRequiresElevation = 740, Win32Cancelled = 1223;

    public static int Main(string[] args)
    {
        // Get file name without ending
        string envName = Path.GetFileName(System.Environment.GetCommandLineArgs()[0]);
        if (envName.EndsWith(".exe", true, CultureInfo.InvariantCulture)) envName = envName.Substring(0, envName.Length - 4);

        // Read environment variables
        string envFile = Environment.GetEnvironmentVariable("ZEROINSTALL_RUNENV_FILE_" + envName) ?? Environment.GetEnvironmentVariable("0install-runenv-file-" + envName);
        string envArgs = Environment.GetEnvironmentVariable("ZEROINSTALL_RUNENV_ARGS_" + envName) ?? Environment.GetEnvironmentVariable("0install-runenv-args-" + envName);
        string userArgs = ConcatenateEscapeArgument(args);

        // Detect missing environment variables
        if (string.IsNullOrEmpty(envFile))
        {
            Console.Error.WriteLine(string.Format("Environment variable '{0}' not set!", "ZEROINSTALL_RUNENV_FILE_" + envName));
            return 1;
        }

        // Launch child process
        ProcessStartInfo startInfo = new ProcessStartInfo(envFile, string.IsNullOrEmpty(userArgs) ? envArgs : envArgs + " " + userArgs);
        startInfo.UseShellExecute = false;
        Process process;
        try
        {
            try
            {
                process = Process.Start(startInfo);
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == Win32RequestedOperationRequiresElevation)
            {
                // UAC handling requires ShellExecute
                startInfo.UseShellExecute = true;
                process = Process.Start(startInfo);
            }

            process.WaitForExit();
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == Win32Cancelled)
        {
            return 100;
        }
        return process.ExitCode;
    }

    #region StringUtils
    /// <summary>
    /// Escapes a string for use as a Windows command-line argument, making sure it is encapsulated within <c>"</c> if it contains whitespace characters.
    /// </summary>
    /// <remarks>
    /// This corresponds to Windows' handling of command-line arguments as specified in:
    /// http://msdn.microsoft.com/library/17w5ykft
    /// </remarks>
    private static string EscapeArgument(string value)
    {
        if (value == null) return null;

        // Add leading quotation mark if there are whitespaces
        bool containsWhitespace = ContainsWhitespace(value);
        StringBuilder result = containsWhitespace ? new StringBuilder("\"", value.Length + 2) : new StringBuilder(value.Length);

        // Split by quotation marks
        string[] parts = value.Split('"');
        for (int i = 0; i < parts.Length; i++)
        {
            // Count slashes preceeding each quotation mark
            string slashesTrimmed = parts[i].TrimEnd('\\');
            int slashesCount = parts[i].Length - slashesTrimmed.Length;

            result.Append(parts[i]);

            if (i < parts.Length - 1)
            { // Not last part
                for (int j = 0; j < slashesCount; j++) result.Append('\\'); // Double number of slashes
                result.Append("\\\""); // Escaped quotation mark
            }
            else if (containsWhitespace)
            { // Last part if there are whitespaces
                for (int j = 0; j < slashesCount; j++) result.Append('\\'); // Double number of slashes
                result.Append('"'); // Non-escaped quotation mark
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Combines multiple strings into one for use as a Windows command-line argument using <see cref="EscapeArgument"/>.
    /// </summary>
    /// <param name="parts">The strings to be combined.</param>
    /// <remarks>
    /// This coressponds to Windows' handling of command-line arguments as specified in:
    /// http://msdn.microsoft.com/library/17w5ykft
    /// </remarks>
    private static string ConcatenateEscapeArgument(IEnumerable<string> parts)
    {
        if (parts == null) return null;

        StringBuilder output = new StringBuilder();
        bool first = true;
        foreach (string part in parts)
        {
            // No separator before first or after last part
            if (first) first = false;
            else output.Append(' ');

            output.Append(EscapeArgument(part));
        }

        return output.ToString();
    }

    /// <summary>
    /// Checks whether a string contains any whitespace characters
    /// </summary>
    private static bool ContainsWhitespace(string text)
    {
        return text.Contains(" ") || text.Contains("\t") || text.Contains("\n") || text.Contains("\r");
    }
    #endregion
}
