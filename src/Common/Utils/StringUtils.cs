/*
 * Copyright 2006-2014 Bastian Eicher
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
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using NanoByte.Common.Collections;

namespace NanoByte.Common.Utils
{
    /// <summary>
    /// Provides additional or simplified string functions.
    /// </summary>
    public static class StringUtils
    {
        #region Comparing
        /// <summary>
        /// Compare strings using case-insensitive comparison.
        /// </summary>
        public static bool EqualsIgnoreCase(string s1, string s2)
        {
            return string.Equals(s1, s2, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Compare chars using case-insensitive comparison.
        /// </summary>
        public static bool EqualsIgnoreCase(char c1, char c2)
        {
            return char.ToLowerInvariant(c1) == char.ToLowerInvariant(c2);
        }

        /// <summary>
        /// Compare strings using case sensitive, invariant culture comparison and considering <see langword="null"/> and <see cref="string.Empty"/> equal.
        /// </summary>
        public static bool EqualsEmptyNull(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) && string.IsNullOrEmpty(s2)) return true;
            return s1 == s2;
        }

        /// <summary>
        /// Use case-insensitive compare to check for a contained string.
        /// </summary>
        /// <param name="text">The string to search.</param>
        /// <param name="value">The string to search for in <paramref name="text"/>.</param>
        public static bool ContainsIgnoreCase(this string text, string value)
        {
            #region Sanity checks
            if (text == null) throw new ArgumentNullException("text");
            if (value == null) throw new ArgumentNullException("value");
            #endregion

            return text.ToUpperInvariant().Contains(value.ToUpperInvariant());
        }

        /// <summary>
        /// Checks whether a string contains any whitespace characters
        /// </summary>
        public static bool ContainsWhitespace(this string text)
        {
            #region Sanity checks
            if (text == null) throw new ArgumentNullException("text");
            #endregion

            return text.Contains(" ") || text.Contains("\t") || text.Contains("\n") || text.Contains("\r");
        }

        /// <summary>
        /// Counts how many times a character occurs within a string.
        /// </summary>
        /// <param name="value">The string to search within.</param>
        /// <param name="token">The character to search for.</param>
        /// <returns>The number of occurences of <paramref name="token"/> wihin <paramref name="value"/>.</returns>
        public static int CountOccurences(this string value, char token)
        {
            if (string.IsNullOrEmpty(value)) return 0;

            return value.Count(t => t == token);
        }

        /// <summary>
        /// Determines whether the beginning of this string matches a specific value case-insensitive comparison.
        /// </summary>
        public static bool StartsWithIgnoreCase(this string text, string value)
        {
            #region Sanity checks
            if (text == null) throw new ArgumentNullException("text");
            if (value == null) throw new ArgumentNullException("value");
            #endregion

            return text.StartsWith(value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines whether the end of this string matches a specific value case-insensitive comparison.
        /// </summary>
        public static bool EndsWithIgnoreCase(this string text, string value)
        {
            #region Sanity checks
            if (text == null) throw new ArgumentNullException("text");
            if (value == null) throw new ArgumentNullException("value");
            #endregion

            return text.EndsWith(value, StringComparison.OrdinalIgnoreCase);
        }
        #endregion

        #region Extraction
        /// <summary>
        /// Gets the last word in a string.
        /// </summary>
        public static string GetLastWord(this string value)
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
        public static string RemoveLastWord(this string value)
        {
            #region Sanity checks
            if (value == null) throw new ArgumentNullException("value");
            #endregion

            string lastWord = GetLastWord(value);
            if (value == lastWord) return "";
            if (lastWord.Length == 0 || value.Length == 0 || value.Length - lastWord.Length - 1 <= 0) return value;
            return value.Substring(0, value.Length - lastWord.Length - 1);
        }

        /// <summary>
        /// Removes all occurences of a set of characters from a string.
        /// </summary>
        public static string RemoveAll(this string value, IEnumerable<char> toRemove)
        {
            return toRemove.Aggregate(value, (acc, target) => acc.Replace(target.ToString(CultureInfo.InvariantCulture), ""));
        }

        /// <summary>
        /// Removes all occurences of a specific set of characters from a string.
        /// </summary>
        // ReSharper disable once ParameterTypeCanBeEnumerable.Global
        public static string StripCharacters(this string value, char[] characters)
        {
            return new string(value.Except(characters.Contains).ToArray());
        }
        #endregion

        #region Splitting
        /// <summary>
        /// Splits a multiline string to several strings and returns the result as a string array.
        /// </summary>
        public static string[] SplitMultilineText(this string value)
        {
            #region Sanity checks
            if (value == null) throw new ArgumentNullException("value");
            #endregion

            var result = new List<string>();
            string[] splitted1 = value.Split('\n');
            string[] splitted2 = value.Split('\r');
            string[] splitted = splitted1.Length >= splitted2.Length ? splitted1 : splitted2;

            foreach (string s in splitted)
            {
                // Never add any \r or \n to the single lines
                if (s.EndsWithIgnoreCase("\r") || s.EndsWithIgnoreCase("\n"))
                    result.Add(s.Substring(0, s.Length - 1));
                else if (s.StartsWithIgnoreCase("\n") || s.StartsWithIgnoreCase("\r"))
                    result.Add(s.Substring(1));
                else
                    result.Add(s);
            }

            return result.ToArray();
        }

        /// <summary>
        /// Combines multiple strings into one, placing a <paramref name="separator"/> between the <paramref name="parts"/>.
        /// </summary>
        /// <param name="separator">The separator characters to place between the <paramref name="parts"/>.</param>
        /// <param name="parts">The strings to be combined.</param>
        /// <remarks>Works like <see cref="string.Join(string,string[])"/> but for <see cref="IEnumerable{T}"/>s.</remarks>
        public static string Join(string separator, IEnumerable<string> parts)
        {
            #region Sanity checks
            if (parts == null) throw new ArgumentNullException("parts");
            #endregion

            var output = new StringBuilder();
            bool first = true;
            foreach (var part in parts)
            {
                // No separator before first or after last part
                if (first) first = false;
                else output.Append(separator);

                output.Append(part);
            }

            return output.ToString();
        }

        /// <summary>
        /// Get everything to the left of the first occurrence of a character.
        /// </summary>
        public static string GetLeftPartAtFirstOccurrence(this string sourceText, char ch)
        {
            #region Sanity checks
            if (sourceText == null) throw new ArgumentNullException("sourceText");
            #endregion

            int index = sourceText.IndexOf(ch);
            return index == -1 ? sourceText : sourceText.Substring(0, index);
        }

        /// <summary>
        /// Get everything to the right of the first occurrence of a character.
        /// </summary>
        public static string GetRightPartAtFirstOccurrence(this string sourceText, char ch)
        {
            #region Sanity checks
            if (sourceText == null) throw new ArgumentNullException("sourceText");
            #endregion

            int index = sourceText.IndexOf(ch);
            return index == -1 ? "" : sourceText.Substring(index + 1);
        }

        /// <summary>
        /// Get everything to the left of the last occurrence of a character.
        /// </summary>
        public static string GetLeftPartAtLastOccurrence(this string sourceText, char ch)
        {
            #region Sanity checks
            if (sourceText == null) throw new ArgumentNullException("sourceText");
            #endregion

            int index = sourceText.LastIndexOf(ch);
            return index == -1 ? sourceText : sourceText.Substring(0, index);
        }

        /// <summary>
        /// Get everything to the right of the last occurrence of a character.
        /// </summary>
        public static string GetRightPartAtLastOccurrence(this string sourceText, char ch)
        {
            #region Sanity checks
            if (sourceText == null) throw new ArgumentNullException("sourceText");
            #endregion

            int index = sourceText.LastIndexOf(ch);
            return index == -1 ? sourceText : sourceText.Substring(index + 1);
        }

        /// <summary>
        /// Get everything to the left of the first occurrence of a string.
        /// </summary>
        public static string GetLeftPartAtFirstOccurrence(this string sourceText, string str)
        {
            #region Sanity checks
            if (sourceText == null) throw new ArgumentNullException("sourceText");
            if (string.IsNullOrEmpty(str)) throw new ArgumentNullException("str");
            #endregion

            int index = sourceText.IndexOf(str, StringComparison.Ordinal);
            return index == -1 ? sourceText : sourceText.Substring(0, index);
        }

        /// <summary>
        /// Get everything to the right of the first occurrence of a string.
        /// </summary>
        public static string GetRightPartAtFirstOccurrence(this string sourceText, string str)
        {
            #region Sanity checks
            if (sourceText == null) throw new ArgumentNullException("sourceText");
            if (string.IsNullOrEmpty(str)) throw new ArgumentNullException("str");
            #endregion

            int index = sourceText.IndexOf(str, StringComparison.Ordinal);
            return index == -1 ? "" : sourceText.Substring(index + str.Length);
        }

        /// <summary>
        /// Get everything to the left of the last occurrence of a string.
        /// </summary>
        public static string GetLeftPartAtLastOccurrence(this string sourceText, string str)
        {
            #region Sanity checks
            if (sourceText == null) throw new ArgumentNullException("sourceText");
            if (string.IsNullOrEmpty(str)) throw new ArgumentNullException("str");
            #endregion

            int index = sourceText.LastIndexOf(str, StringComparison.Ordinal);
            return index == -1 ? sourceText : sourceText.Substring(0, index);
        }

        /// <summary>
        /// Get everything to the right of the last occurrence of a string.
        /// </summary>
        public static string GetRightPartAtLastOccurrence(this string sourceText, string str)
        {
            #region Sanity checks
            if (sourceText == null) throw new ArgumentNullException("sourceText");
            if (string.IsNullOrEmpty(str)) throw new ArgumentNullException("str");
            #endregion

            int index = sourceText.LastIndexOf(str, StringComparison.Ordinal);

            return index == -1 ? "" : sourceText.Substring(index + str.Length);
        }
        #endregion

        #region Arguments escaping
        /// <summary>
        /// Escapes a string for use as a Windows command-line argument, making sure it is encapsulated within <code>"</code> if it contains whitespace characters.
        /// </summary>
        /// <remarks>
        /// This coressponds to Windows' handling of command-line arguments as specified in:
        /// http://msdn.microsoft.com/library/17w5ykft
        /// </remarks>
        public static string EscapeArgument(this string value)
        {
            if (value == null) return null;

            // Add leading quotation mark if there are whitespaces
            bool containsWhitespace = ContainsWhitespace(value);
            var result = containsWhitespace ? new StringBuilder("\"", value.Length + 2) : new StringBuilder(value.Length);

            // Split by quotation marks
            string[] parts = value.Split('"');
            for (int i = 0; i < parts.Length; i++)
            {
                // Count slashes preceeding the quotation mark
                int slashesCount = parts[i].Length - parts[i].TrimEnd('\\').Length;

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
        public static string JoinEscapeArguments(this IEnumerable<string> parts)
        {
            if (parts == null) return null;

            var output = new StringBuilder();
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
        #endregion

        #region base64
        /// <summary> 
        /// Encodes a string as UTF-8 in base64.
        /// </summary>
        public static string Base64Utf8Encode(this string value)
        {
            return value == null ? null : Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        }

        /// <summary>
        /// Decodes a UTF-8 in base64 string.
        /// </summary>
        /// <exception cref="FormatException">Thrown if <paramref name="value"/> is not a valid base 64 string.</exception>
        public static string Base64Utf8Decode(this string value)
        {
            return value == null ? null : Encoding.UTF8.GetString(Convert.FromBase64String(value));
        }
        #endregion

        #region base32
        private static readonly char[] _base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".ToCharArray();
        private const int NormaleByteSize = 8, Base32ByteSize = 5;

        /// <summary>
        /// Encodes a byte array in base32 without padding.
        /// </summary>
        public static string Base32Encode(this byte[] data)
        {
            #region Sanity checks
            if (data == null) throw new ArgumentNullException("data");
            if (data.Length == 0) return "";
            #endregion

            int i = 0, index = 0;
            var result = new StringBuilder((data.Length + 7) * NormaleByteSize / Base32ByteSize);

            while (i < data.Length)
            {
                // ReSharper disable ConditionIsAlwaysTrueOrFalse
                int currentByte = (data[i] >= 0) ? data[i] : (data[i] + 256);
                int digit;

                // Is the current digit going to span a byte boundary?
                if (index > (NormaleByteSize - Base32ByteSize))
                {
                    int nextByte = (i + 1) < data.Length
                        ? ((data[i + 1] >= 0) ? data[i + 1]
                            : (data[i + 1] + 256)) : 0;

                    digit = currentByte & (0xFF >> index);
                    index = (index + Base32ByteSize) % NormaleByteSize;
                    digit <<= index;
                    digit |= nextByte >> (NormaleByteSize - index);
                    i++;
                }
                else
                {
                    digit = (currentByte >> (NormaleByteSize - (index + Base32ByteSize))) & 0x1F;
                    index = (index + Base32ByteSize) % NormaleByteSize;
                    if (index == 0)
                        i++;
                }
                // ReSharper restore ConditionIsAlwaysTrueOrFalse
                result.Append(_base32Alphabet[digit]);
            }

            return result.ToString();
        }
        #endregion

        #region base16
        /// <summary>
        /// Encodes a byte array in base16 (hexadecimal).
        /// </summary>
        public static string Base16Encode(this byte[] data)
        {
            #region Sanity checks
            if (data == null) throw new ArgumentNullException("data");
            if (data.Length == 0) return "";
            #endregion

            return BitConverter.ToString(data).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>
        /// Decodes a base16 (hexadecimal) to a byte array.
        /// </summary>
        public static byte[] Base16Decode(this string encoded)
        {
            #region Sanity checks
            if (encoded == null) throw new ArgumentNullException("encoded");
            #endregion

            var result = new byte[encoded.Length / 2];
            for (int i = 0; i < encoded.Length / 2; i++)
                result[i] = Convert.ToByte(encoded.Substring(i * 2, 2), 16);
            return result;
        }
        #endregion

        #region Hash
        /// <summary>
        /// Computes the hash value of a string encoded as UTF-8.
        /// </summary>
        /// <param name="value">The string to hash.</param>
        /// <param name="algorithm">The hashing algorithm to use.</param>
        /// <returns>A hexadecimal string representation of the hash value.</returns>
        public static string Hash(this string value, HashAlgorithm algorithm)
        {
            #region Sanity checks
            if (value == null) throw new ArgumentNullException("value");
            if (algorithm == null) throw new ArgumentNullException("algorithm");
            #endregion

            var hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(value));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
        #endregion

        #region Generate password
        /// <summary>
        /// Returns a string filled with random human-readable ASCII characters based on a cryptographic random number generator.
        /// </summary>
        /// <param name="length">The length of the string to be generated.</param>
        public static string GeneratePassword(int length)
        {
            var generator = RandomNumberGenerator.Create();
            var array = new byte[(int)Math.Round(length * 3 / 4f)];
            generator.GetBytes(array);

            // Use base64 encoding without '=' padding and with '-' instead of 'l'
            return Convert.ToBase64String(array).Substring(0, length).Replace('l', '-');
        }
        #endregion

        #region File size
        /// <summary>
        /// Formats a byte number in human-readable form (KB, MB, GB).
        /// </summary>
        /// <param name="value">The value in bytes.</param>
        /// <param name="provider">Provides culture-specific formatting information.</param>
        public static string FormatBytes(this long value, IFormatProvider provider)
        {
            if (value >= 1073741824)
                return string.Format(provider, "{0:0.00}", value / 1073741824f) + " GB";
            if (value >= 1048576)
                return string.Format(provider, "{0:0.00}", value / 1048576f) + " MB";
            if (value >= 1024)
                return string.Format(provider, "{0:0.00}", value / 1024f) + " KB";
            return value + " Bytes";
        }
        #endregion

        #region Unix paths
        /// <summary>
        /// Expands/substitutes any Unix-style environment variables in the string.
        /// </summary>
        /// <param name="value">The string containing variables to be expanded.</param>
        /// <param name="variables">The list of variables available for expansion.</param>
        public static string ExpandUnixVariables(string value, StringDictionary variables)
        {
            #region Sanity checks
            if (value == null) throw new ArgumentNullException("value");
            if (variables == null) throw new ArgumentNullException("variables");
            #endregion

            // Substitute ${VAR} for the value of VAR
            value = new Regex(@"\${(.+)}").Replace(value, match => variables[match.Groups[1].Value]);

            // Substitute $VAR for the value of VAR
            value = new Regex(@"\$([^\$\s\\/-]+)").Replace(value, match => variables[match.Groups[1].Value]);

            return value;
        }
        #endregion
    }
}
