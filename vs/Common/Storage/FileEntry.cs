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
