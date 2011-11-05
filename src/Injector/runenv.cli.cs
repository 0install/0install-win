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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

/// <summary>
/// This helper executable launches a command-line specified in specific environment variables.
/// <summary>
public class RunEnv
{
    public static int Main(string[] args)
    {
        string envName = Path.GetFileName(System.Environment.GetCommandLineArgs()[0]);
        string envFile = Environment.GetEnvironmentVariable("0install-runenv-file-" + envName);
        string envArgs = Environment.GetEnvironmentVariable("0install-runenv-args-" + envName);
        string userArgs = ConcatenateEscapeArgument(args);
        Process process = Process.Start(envFile, string.IsNullOrEmpty(userArgs) ? envArgs : envArgs + " " + userArgs);
        process.WaitForExit();
        return process.ExitCode;
    }

    #region StringUtils
    /// <summary>
    /// Escapes a string for use as a command-line argument, making sure it is encapsulated within <code>"</code> if it contains whitespace characters.
    /// </summary>s
    private static string EscapeArgument(string value)
    {
        if (value == null) return null;

        value = value.Replace("\"", "\\\""); // Escape quotation marks

        if (value.Contains(" ") || value.Contains("\t") || value.Contains("\n") || value.Contains("\r"))
        {
            // Escape trailing backslashes
            if (value.EndsWith("\\")) value += "\\";

            // ToDo: Handle multiple consecutive backslashes

            // Encapsulate within quotation marks
            value = "\"" + value + "\"";
        }
        return value;
    }

    /// <summary>
    /// Combines multiple strings into one for use as a command-line argument using <see cref="EscapeArgument"/>.
    /// </summary>
    /// <param name="parts">The strings to be combines.</param>
    private static string ConcatenateEscapeArgument(IEnumerable<string> parts)
    {
        #region Sanity checks
        if (parts == null) throw new ArgumentNullException("parts");
        #endregion

        StringBuilder output = new StringBuilder();
        bool first = true;
        foreach (string part in parts)
        {
            // No separator before first or after last line
            if (first) first = false;
            else output.Append(' ');

            output.Append(EscapeArgument(part));
        }

        return output.ToString();
    }
    #endregion
}