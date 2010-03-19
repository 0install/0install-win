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
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
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
        #region Unify slashes
        /// <summary>
        /// Replaces all slashes (forward and backward) with <see cref="Path.DirectorySeparatorChar"/>
        /// </summary>
        [LuaGlobal(Description = "Replaces all slashes (forward and backward) with the system's directory separator character")]
        public static string UnifySlashes(string value)
        {
            if (value == null) return null;
            return value.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
        }
        #endregion

        #region Comparing, Counting and Extraction
        /// <summary>
        /// Compare strings using case insensitive, invariant culture comparison
        /// </summary>
        [LuaGlobal(Name = "CompareStrings", Description = "Compare strings using case insensitive, invariant culture comparison")]
        public static bool Compare(string s1, string s2)
        {
            return string.Compare(s1, s2, StringComparison.OrdinalIgnoreCase) == 0;
        }

        /// <summary>
        /// Helps to compare strings, uses case insensitive comparison.
        /// string.Compare is also gay because we have always to check for == 0.
        /// This overload allows multiple strings to be checked, if any of
        /// them matches we are good to go (e.g. ("hi", {"hey", "hello", "hi"})
        /// will return <see langword="true"/>).
        /// </summary>
        public static bool Compare(string s1, string[] anyMatch)
        {
            #region Sanity checks
            if (s1 == null) throw new ArgumentNullException("s1");
            if (anyMatch == null) throw new ArgumentNullException("anyMatch");
            #endregion

            foreach (string match in anyMatch)
                if (string.Compare(s1, match, StringComparison.OrdinalIgnoreCase) == 0)
                    return true;
            return false;
        }

        /// <summary>
        /// Is a specific name in a list of strings?
        /// </summary>
        public static bool IsInList(string name, ArrayList list, bool ignoreCase, CultureInfo culture)
        {
            #region Sanity checks
            if (name == null) throw new ArgumentNullException("name");
            if (list == null) throw new ArgumentNullException("list");
            if (culture == null) throw new ArgumentNullException("culture");
            #endregion

            foreach (string listEntry in list)
                if (string.Compare(name, listEntry, ignoreCase, culture) == 0)
                    return true;
            return false;
        }

        /// <summary>
        /// Is a specific name in a list of strings?
        /// </summary>
        public static bool IsInList(string name, string[] list, bool ignoreCase, CultureInfo culture)
        {
            #region Sanity checks
            if (name == null) throw new ArgumentNullException("name");
            if (list == null) throw new ArgumentNullException("list");
            if (culture == null) throw new ArgumentNullException("culture");
            #endregion

            foreach (string listEntry in list)
                if (string.Compare(name, listEntry, ignoreCase, culture) == 0)
                    return true;
            return false;
        }

        /// <summary>
        /// Count words in a text (words are only separated by ' ' (spaces))
        /// </summary>
        [LuaGlobal(Description = "Count words in a text (words are only separated by ' ' (spaces))")]
        public static int CountWords(string text)
        {
            return string.IsNullOrEmpty(text) ? 0 : text.Split(new[] {' '}).Length;
        }

        /// <summary>
        /// Case-insensitive character comparison
        /// </summary>
        public static bool CompareChar(char c1, char c2)
        {
            return char.ToLower(c1, CultureInfo.InvariantCulture) == char.ToLower(c2, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Get last word in a string
        /// </summary>
        public static string GetLastWord(string text)
        {
            #region Sanity checks
            if (text == null) throw new ArgumentNullException("text");
            #endregion

            string[] words = text.Split(new[] {' '});
            return words.Length > 0 ? words[words.Length - 1].TrimEnd('.') : text.TrimEnd('.');
        }

        /// <summary>
        /// Remove last word in a string
        /// </summary>
        public static string RemoveLastWord(string text)
        {
            #region Sanity checks
            if (text == null) throw new ArgumentNullException("text");
            #endregion

            string lastWord = GetLastWord(text);
            if (text == lastWord)  return "";
            if (lastWord.Length == 0 || text.Length == 0 || text.Length - lastWord.Length - 1 <= 0) return text;
            return text.Substring(0, text.Length - lastWord.Length - 1);
        }

        /// <summary>
        /// Get tab depth
        /// </summary>
        public static int GetTabDepth(string text)
        {
            if (text == null) throw new ArgumentNullException("text");

            for (int textPos = 0; textPos < text.Length; textPos++)
                if (text[textPos] != '\t')
                    return textPos;
            return text.Length;
        }
        #endregion

        #region String contains (for case insensitive compares)
        /// <summary>
        /// Use case-insensitive compare to check for a contained string
        /// </summary>
        /// <param name="textToCheck">Text to check</param>
        /// <param name="searchName">Search name</param>
        [LuaGlobal(Description = "Use case-insensitive compare to check for a contained string")]
        public static bool Contains(string textToCheck, string searchName)
        {
            if (textToCheck == null) throw new ArgumentNullException("textToCheck");
            if (searchName == null) throw new ArgumentNullException("searchName");

            return textToCheck.ToUpperInvariant().Contains(searchName.ToUpperInvariant());
        }

        /// <summary>
        /// Is any of the names in <paramref name="searchNames"/> contained in <paramref name="textToCheck"/>? (will check case-insensitive)
        /// </summary>
        /// <param name="textToCheck">String to check</param>
        /// <param name="searchNames">Names to search for</param>
        public static bool Contains(string textToCheck, string[] searchNames)
        {
            if (textToCheck == null) throw new ArgumentNullException("textToCheck");
            if (searchNames == null) throw new ArgumentNullException("searchNames");

            string stringToCheckUpper = textToCheck.ToUpperInvariant();
            foreach (string name in searchNames)
                if (stringToCheckUpper.Contains(name.ToUpperInvariant()))
                    return true;

            // Nothing found, no searchNames is contained in textToCheck
            return false;
        }
        #endregion

        #region Write data
        /// <summary>
        /// Returns a string with the array data
        /// </summary>
        public static string WriteArrayData(byte[] array)
        {
            var ret = new StringBuilder();
            if (array != null)
                for (int i = 0; i < array.Length; i++)
                    ret.Append((ret.Length == 0 ? "" : ", ") + array[i].ToString(CultureInfo.InvariantCulture.NumberFormat));
            return ret.ToString();
        }

        /// <summary>
        /// Returns a string with the array data
        /// </summary>
        public static string WriteArrayData(int[] array)
        {
            var ret = new StringBuilder();
            if (array != null)
                for (int i = 0; i < array.Length; i++)
                    ret.Append((ret.Length == 0 ? "" : ", ") + array[i].ToString(CultureInfo.InvariantCulture.NumberFormat));
            return ret.ToString();
        }

        /// <summary>
        /// Returns a string with the array data
        /// </summary>
        public static string WriteArrayData(Array array)
        {
            var ret = new StringBuilder();
            if (array != null)
                for (int i = 0; i < array.Length; i++)
                    ret.Append((ret.Length == 0 ? "" : ", ") + (array.GetValue(i) == null ? "null" : array.GetValue(i).ToString()));
            return ret.ToString();
        }

        /// <summary>
        /// Returns a string with the array data
        /// </summary>
        public static string WriteArrayData(ArrayList array)
        {
            var ret = new StringBuilder();
            if (array != null)
                foreach (object obj in array)
                    ret.Append((ret.Length == 0 ? "" : ", ") + obj);
            return ret.ToString();
        }

        /// <summary>
        /// Returns a string with the array data
        /// </summary>
        public static string WriteArrayData(CollectionBase collection)
        {
            var ret = new StringBuilder();
            if (collection != null)
                foreach (object obj in collection)
                    ret.Append((ret.Length == 0 ? "" : ", ") + obj);
            return ret.ToString();
        }

        /// <summary>
        /// Returns a string with the array data
        /// </summary>
        public static string WriteArrayData(StringCollection textCollection)
        {
            var ret = new StringBuilder();
            if (textCollection != null)
                foreach (string s in textCollection)
                    ret.Append((ret.Length == 0 ? "" : ", ") + s);
            return ret.ToString();
        }

        /// <summary>
        /// Returns a string with the array data
        /// </summary>
        public static string WriteArrayData(IEnumerable enumerableClass)
        {
            var ret = new StringBuilder();
            if (enumerableClass != null)
                foreach (object obj in enumerableClass)
                    ret.Append((ret.Length == 0 ? "" : ", ") + obj);
            return ret.ToString();
        }

        /// <summary>
        /// Write into space string, useful for writing parameters without
        /// knowing the length of each string, e.g. when writing numbers
        /// (-1, 1.45, etc.). You can use this function to give all strings
        /// the same width in a table. Maybe there is already a string function
        /// for this, but I don't found any useful stuff.
        /// </summary>
        public static string WriteIntoSpaceString(string message, int spaces)
        {
            if (message == null) throw new ArgumentNullException("message");

            // Msg is already that long or longer?
            if (message.Length >= spaces)
                return message;

            // Create string with number of specified spaces
            var ret = new char[spaces];

            // Copy data
            int i;
            for (i = 0; i < message.Length; i++)
                ret[i] = message[i];
            // Fill rest with spaces
            for (i = message.Length; i < spaces; i++)
                ret[i] = ' ';

            // Return result
            return new string(ret);
        }

        /// <summary>
        /// Write ISO Date (Year-Month-Day)
        /// </summary>
        public static string WriteIsoDate(DateTime date)
        {
            return date.Year + "-" + date.Month.ToString("00", CultureInfo.InvariantCulture) + "-" + date.Day.ToString("00", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Write ISO Date and time (Year-Month-Day Hour:Minute)
        /// </summary>
        public static string WriteIsoDateAndTime(DateTime date)
        {
            return date.Year + "-" +
                   date.Month.ToString("00", CultureInfo.InvariantCulture) + "-" +
                   date.Day.ToString("00", CultureInfo.InvariantCulture) + " " +
                   date.Hour.ToString("00", CultureInfo.InvariantCulture) + ":" +
                   date.Minute.ToString("00", CultureInfo.InvariantCulture);
        }
        #endregion

        #region String splitting and getting it back together
        /// <summary>
        /// Splits a multi line string to several strings and returns the result as a string array
        /// </summary>
        public static string[] SplitMultilineText(string text)
        {
            var ret = new ArrayList();
            // Supports any format, only \r, only \n, normal \r\n,
            // crazy \n\r or even mixed \r\n with any format
            string[] splitted1 = text.Split(new[] {'\n'});
            string[] splitted2 = text.Split(new[] {'\r'});
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

            return (string[]) ret.ToArray(typeof(string));
        }

        /// <summary>
        /// Build string from lines
        /// </summary>
        public static string BuildStringFromLines(string[] lines, int startLine, int startOffset, int endLine, int endOffset, string separator)
        {
            if (lines == null) throw new ArgumentNullException("lines");
            if (string.IsNullOrEmpty(separator)) throw new ArgumentNullException("separator");

            // Check if all values are in range (correct if not)
            if (startLine >= lines.Length)
                startLine = lines.Length - 1;
            if (endLine >= lines.Length)
                endLine = lines.Length - 1;
            if (startLine < 0)
                startLine = 0;
            if (endLine < 0)
                endLine = 0;
            if (startOffset >= lines[startLine].Length)
                startOffset = lines[startLine].Length - 1;
            if (endOffset >= lines[endLine].Length)
                endOffset = lines[endLine].Length - 1;
            if (startOffset < 0)
                startOffset = 0;
            if (endOffset < 0)
                endOffset = 0;

            var builder = new StringBuilder((endLine - startLine)*80);
            for (int lineNumber = startLine; lineNumber <= endLine; lineNumber++)
            {
                if (lineNumber == startLine)
                    builder.Append(lines[lineNumber].Substring(startOffset));
                else if (lineNumber == endLine)
                    builder.Append(lines[lineNumber].Substring(0, endOffset + 1));
                else
                    builder.Append(lines[lineNumber]);

                if (lineNumber != endLine)
                    builder.Append(separator);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Build string from lines
        /// </summary>
        public static string BuildStringFromLines(string[] lines, string separator)
        {
            if (lines == null) throw new ArgumentNullException("lines");
            if (string.IsNullOrEmpty(separator)) throw new ArgumentNullException("separator");

            var builder = new StringBuilder(lines.Length*80);
            for (int lineNumber = 0; lineNumber < lines.Length; lineNumber++)
            {
                builder.Append(lines[lineNumber]);
                if (lineNumber != lines.Length - 1)
                    builder.Append(separator);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Build string from lines
        /// </summary>
        public static string BuildStringFromLines(string[] lines)
        {
            if (lines == null) throw new ArgumentNullException("lines");

            return BuildStringFromLines(lines, "\r\n");
        }

        /// <summary>
        /// Build string from lines
        /// </summary>
        public static string BuildStringFromLines(string[] lines, int startLine, int endLine, string separator)
        {
            if (lines == null) throw new ArgumentNullException("lines");
            if (string.IsNullOrEmpty(separator)) throw new ArgumentNullException("separator");

            // Check if all values are in range (correct if not)
            if (startLine < 0)
                startLine = 0;
            if (endLine < 0)
                endLine = 0;
            if (startLine >= lines.Length)
                startLine = lines.Length - 1;
            if (endLine >= lines.Length)
                endLine = lines.Length - 1;

            var builder = new StringBuilder((endLine - startLine)*80);
            for (int lineNumber = startLine; lineNumber <= endLine; lineNumber++)
            {
                builder.Append(lines[lineNumber]);
                if (lineNumber != endLine)
                    builder.Append(separator);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Get left part of everything to the left of the first occurrence of a character
        /// </summary>
        public static string GetLeftPartAtFirstOccurrence(string sourceText, char ch)
        {
            if (sourceText == null) throw new ArgumentNullException("sourceText");
            
            int index = sourceText.IndexOf(ch);
            return index == -1 ? sourceText : sourceText.Substring(0, index);
        }

        /// <summary>
        /// Get right part of everything to the right of the first occurrence of a character
        /// </summary>
        public static string GetRightPartAtFirstOccurrence(string sourceText, char ch)
        {
            if (sourceText == null) throw new ArgumentNullException("sourceText");

            int index = sourceText.IndexOf(ch);
            return index == -1 ? "" : sourceText.Substring(index + 1);
        }

        /// <summary>
        /// Get left part of everything to the left of the last occurrence of a character
        /// </summary>
        public static string GetLeftPartAtLastOccurrence(string sourceText, char ch)
        {
            if (sourceText == null) throw new ArgumentNullException("sourceText");

            int index = sourceText.LastIndexOf(ch);
            return index == -1 ? sourceText : sourceText.Substring(0, index);
        }

        /// <summary>
        /// Get right part of everything to the right of the last occurrence of a character
        /// </summary>
        public static string GetRightPartAtLastOccurrence(string sourceText, char ch)
        {
            if (sourceText == null) throw new ArgumentNullException("sourceText");

            int index = sourceText.LastIndexOf(ch);
            return index == -1 ? sourceText : sourceText.Substring(index + 1);
        }

        /// <summary>
        /// Convert single letter to lowercase
        /// </summary>
        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "We specifically want lowercase")]
        public static char ToLower(char letter)
        {
            return letter.ToString().ToLowerInvariant()[0];
        }

        /// <summary>
        /// Convert single letter to uppercase
        /// </summary>
        public static char ToUpper(char letter)
        {
            return letter.ToString().ToUpper(CultureInfo.InvariantCulture)[0];
        }

        /// <summary>
        /// Helper function to check if this is an lowercase letter.
        /// </summary>
        public static bool IsLowercaseLetter(char letter)
        {
            return letter == ToLower(letter);
        }

        /// <summary>
        /// Helper function to check if this is an uppercase letter.
        /// </summary>
        public static bool IsUppercaseLetter(char letter)
        {
            return letter == ToUpper(letter);
        }

        /// <summary>
        /// Checks if letter is space ' ' or any punctuation (. , : ; ' " ! ?)
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
        #endregion

        #region Remove character
        /// <summary>
        /// Removes a character from a string
        /// </summary>
        public static void RemoveCharacter(ref string text, char characterToBeRemoved)
        {
            if (text == null) throw new ArgumentNullException("text");

            if (text.Contains(characterToBeRemoved.ToString()))
                text = text.Replace(characterToBeRemoved.ToString(), "");
        }
        #endregion
    }
}