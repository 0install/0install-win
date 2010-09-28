/*
 * Copyright 2006-2010 Simon E. Silva Lauinger, Bastian Eicher
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

namespace Common.Cli
{
    /// <summary>
    /// Provides helper methods for asking the user for input on the <see cref="Console"/>.
    /// </summary>
    public static class InputUtils
    {
        /// <summary>
        /// Asks the user to input a string.
        /// </summary>
        /// <param name="prompt">The prompt to display to the user on <see cref="Console.Error"/>.</param>
        /// <returns>The string the user entered; <see cref="string.Empty"/> if none; <see langword="null"/> if the input stream has been closed.</returns>
        public static string ReadString(string prompt)
        {
            Console.Error.Write(prompt);
            return Console.ReadLine();
        }

        /// <summary>
        /// Asks the user to input a password without echoing it.
        /// </summary>
        /// <param name="prompt">The prompt to display to the user on <see cref="Console.Error"/>.</param>
        /// <returns>The password the user entered; <see cref="string.Empty"/> if none.</returns>
        public static string ReadPassword(string prompt)
        {
            Console.Error.Write(prompt);

            string password = "";

            ConsoleKeyInfo key = Console.ReadKey(true);
            while (key.Key != ConsoleKey.Enter)
            {
                if (key.Key == ConsoleKey.Backspace)
                {
                    if (!string.IsNullOrEmpty(password))
                        password = password.Substring(0, password.Length - 1);
                }
                else password += key.KeyChar;

                key = Console.ReadKey(true);
            }
            Console.Error.WriteLine();

            return password;
        }
    }
}
