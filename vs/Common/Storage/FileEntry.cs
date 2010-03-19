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
using System.Drawing;
using Common.Helpers;

namespace Common.Storage
{
    #region Enumerations
    /// <seealso cref="FileEntry.EntryType"/>
    public enum FileEntryType
    {
        /// <summary>The file is present in the main game and was not modified by a mod.</summary>
        Normal,
        /// <summary>The file is present in the main game and was modified/overwritten by a mod.</summary>
        Modified,
        /// <summary>The file is not present in the main game and was added by a mod.</summary>
        Added
    }
    #endregion

    /// <summary>
    /// Describes a file returned by <see cref="ContentManager.GetFileList"/>.
    /// </summary>
    public class FileEntry : INamed, IHighlightable, IEquatable<FileEntry>, IComparable<FileEntry>
    {
        #region Properties
        private readonly string _name;
        /// <summary>
        /// The relative file path.
        /// </summary>
        public string Name { get { return _name; } }

        private readonly FileEntryType _entryType;
        /// <summary>
        /// The kind of file entry this is (in relation to its mod status).
        /// </summary>
        public FileEntryType EntryType { get { return _entryType; } }

        /// <summary>
        /// The color to highlight this file entry with in list representations.
        /// <see cref="Color.Empty"/> for <see cref="Storage.FileEntryType.Normal"/> (no highlighting).
        /// <see cref="Color.Blue"/> for <see cref="Storage.FileEntryType.Modified"/>.
        /// <see cref="Color.Green"/> for <see cref="Storage.FileEntryType.Added"/>.
        /// </summary>
        public Color HighlightColor
        {
            get
            {
                switch (_entryType)
                {
                    case FileEntryType.Normal: return Color.Empty;
                    case FileEntryType.Modified: return Color.Blue;
                    case FileEntryType.Added: return Color.Green;
                    default: throw new InvalidOperationException(); // Can never be reached
                }
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new file entry.
        /// </summary>
        /// <param name="name">The relative file path.</param>
        /// <param name="entryType">The kind of file entry this is (in relation to its mod status).</param>
        internal FileEntry(string name, FileEntryType entryType)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            #endregion

            _name = name;
            _entryType = entryType;
        }
        #endregion

        //--------------------//

        #region Equality and comparison
        public bool Equals(FileEntry other)
        {
            if (other == null) return false;
            return StringHelper.Compare(Name, other.Name);
        }

        public int CompareTo(FileEntry other)
        {
            return Name.CompareTo(other.Name);
        }
        #endregion
    }
}
