/*
 * Copyright 2006-2010 Bastian Eicher
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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using LuaInterface;

namespace Common.Helpers
{
    /// <summary>
    /// Provides additional or simplified string functions
    /// </summary>
    public static class StringHelper
    {
        #region File paths
        /// <summary>
        /// Replaces all slashes (forward and backward) with <see cref="Path.DirectorySeparatorChar"/>.
        /// </summary>
        [LuaGlobal(Description = "Replaces all slashes (forward and backward) with the system's directory separator character.")]
        public static string UnifySlashes(string value)
        {
            if (value == null) return null;
            return value.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
        }
        #endregion

        #region Comparing
        /// <summary>
        /// Compare strings using case insensitive, invariant culture comparison.
        /// </summary>
        [LuaGlobal(Name = "CompareStrings", Description = "Compare strings using case insensitive, invariant culture comparison.")]
        public static bool Compare(string s1, string s2)
        {
            return string.Compare(s1, s2, StringComparison.OrdinalIgnoreCase) == 0;
        }

        /// <summary>
        /// Compare chars using case insensitive, invariant culture comparison.
        /// </summary>
        public static bool CompareChar(char c1, char c2)
        {
            return char.ToLower(c1, CultureInfo.InvariantCulture) == char.ToLower(c2, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Checks if a character is a space ' ' or any punctuation (. , : ; ' " ! ?).
        /// </summary>
        public static bool IsSpaceOrPunctuation(char letter)
        {
            return
                letter == ' ' ||
                letter == '.' ||
                letter == ',' ||
                letter == ':' ||
                letter == ';' ||
                letter == '\'' ||
                letter == '\"' ||
                letter == '!' ||
                letter == '?' ||
                letter == '*';
        }

        /// <summary>
        /// Use case-insensitive compare to check for a contained string.
        /// </summary>
        /// <param name="text">The string to search.</param>
        /// <param name="value">The string to search for in <paramref name="text"/>.</param>
        [LuaGlobal(Name = "StringContains", Description = "Use case-insensitive compare to check for a contained string.")]
        public static bool Contains(string text, string value)
        {
            #region Sanity checks
            if (text == null) throw new ArgumentNullException("text");
            if (value == null) throw new ArgumentNullException("value");
            #endregion

            return text.ToUpperInvariant().Contains(value.ToUpperInvariant());
        }
        #endregion

        #region Extraction
        /// <summary>
        /// Gets the last word in a string.
        /// </summary>
        [LuaGlobal(Name = "GetLastWord", Description = "Gets the last word in a string.")]
        public static string GetLastWord(string value)
        {
            #region Sanity checks
            if (value == null) throw new ArgumentNullException("value");
            #endregion

            string[] words = value.Split(new[] {' '});
            return words.Length > 0 ? words[words.Length - 1].TrimEnd('.') : value.TrimEnd('.');
        }

        /// <summary>
        /// Removes the last word in a string.
        /// </summary>
        [LuaGlobal(Name = "RemoveLastWord", Description = "Removes the last word in a string.")]
        public static string RemoveLastWord(string value)
        {
            #region Sanity checks
            if (value == null) throw new ArgumentNullException("value");
            #endregion

            string lastWord = GetLastWord(value);
            if (value == lastWord)  return "";
            if (lastWord.Length == 0 || value.Length == 0 || value.Length - lastWord.Length - 1 <= 0) return value;
            return value.Substring(0, value.Length - lastWord.Length - 1);
        }

        /// <summary>
        /// Removes a character from a string.
        /// </summary>
        public static void RemoveCharacter(ref string text, char characterToBeRemoved)
        {
            #region Sanity checks
            if (text == null) throw new ArgumentNullException("text");
            #endregion

            if (text.Contains(characterToBeRemoved.ToString()))
                text = text.Replace(characterToBeRemoved.ToString(), "");
        }
        #endregion

        #region Splitting
        /// <summary>
        /// Splits a multi line string to several strings and returns the result as a string array.
        /// </summary>
        public static string[] SplitMultilineText(string value)
        {
            #region Sanity checks
            if (value == null) throw new ArgumentNullException("value");
            #endregion

            var ret = new ArrayList();
            string[] splitted1 = value.Split(new[] { '\n' });
            string[] splitted2 = value.Split(new[] { '\r' });
            string[] splitted = splitted1.Length >= splitted2.Length ? splitted1 : splitted2;

            foreach (string s in splitted)
            {
                // Never add any \r or \n to the single lines
                if (s.EndsWith("\r", StringComparison.Ordinal) || s.EndsWith("\n", StringComparison.Ordinal))
                    ret.Add(s.Substring(0, s.Length - 1));
                else if (s.StartsWith("\n", StringComparison.Ordinal) || s.StartsWith("\r", StringComparison.Ordinal))
                    ret.Add(s.Substring(1));
                else
                    ret.Add(s);
            }

            return (string[])ret.ToArray(typeof(string));
        }

        /// <summary>
        /// Combines multiple strings into one, placing a <paramref name="separator"/> between the <paramref name="parts"/>.
        /// </summary>
        /// <param name="parts">The strings to be combines.</param>
        /// <param name="separator">The separator characters to place between the <paramref name="parts"/>.</param>
        public static string Concatenate(IEnumerable<string> parts, string separator)
        {
            #region Sanity checks
            if (parts == null) throw new ArgumentNullException("parts");
            #endregion

            var output = new StringBuilder();
            foreach (var part in parts)
            {
                output.Append(part);
                output.Append(separator);
            }

            // No separator after last line
            if (output.Length != 0) output.Remove(output.Length - 1, 1);

            return output.ToString();
        }

        /// <summary>
        /// Get left part of everything to the left of the first occurrence of a character.
        /// </summary>
        public static string GetLeftPartAtFirstOccurrence(string sourceText, char ch)
        {
            #region Sanity checks
            if (sourceText == null) throw new ArgumentNullException("sourceText");
            #endregion
            
            int index = sourceText.IndexOf(ch);
            return index == -1 ? sourceText : sourceText.Substring(0, index);
        }

        /// <summary>
        /// Get right part of everything to the right of the first occurrence of a character.
        /// </summary>
        public static string GetRightPartAtFirstOccurrence(string sourceText, char ch)
        {
            #region Sanity checks
            if (sourceText == null) throw new ArgumentNullException("sourceText");
            #endregion

            int index = sourceText.IndexOf(ch);
            return index == -1 ? "" : sourceText.Substring(index + 1);
        }

        /// <summary>
        /// Get left part of everything to the left of the last occurrence of a character.
        /// </summary>
        public static string GetLeftPartAtLastOccurrence(string sourceText, char ch)
        {
            #region Sanity checks
            if (sourceText == null) throw new ArgumentNullException("sourceText");
            #endregion

            int index = sourceText.LastIndexOf(ch);
            return index == -1 ? sourceText : sourceText.Substring(0, index);
        }

        /// <summary>
        /// Get right part of everything to the right of the last occurrence of a character.
        /// </summary>
        public static string GetRightPartAtLastOccurrence(string sourceText, char ch)
        {
            #region Sanity checks
            if (sourceText == null) throw new ArgumentNullException("sourceText");
            #endregion

            int index = sourceText.LastIndexOf(ch);
            return index == -1 ? sourceText : sourceText.Substring(index + 1);
        }
        #endregion
    }
}