/*
 * Copyright 2010-2014 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.ComponentModel;
using System.IO;
using NanoByte.Common.Utils;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Publish.EntryPoints
{
    /// <summary>
    /// Collects information about a potential candidate for an entry point.
    /// The subclass type determines the type of executable (native binary, interpreted script, etc.).
    /// </summary>
    public abstract class Candidate
    {
        /// <summary>
        /// Analyzes a file to determine whether it matches this candidate type and extracts meta data.
        /// </summary>
        /// <param name="baseDirectory">The base directory containing the entire application.</param>
        /// <param name="file">The file to be analyzed. Must be located within the <paramref name="baseDirectory"/> or a subdirectory.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="file"/> matches this candidate type. The object will then contain all available metadata.
        /// <see langword="false"/> if <paramref name="file"/>does not match this candidate type. The object will then be in an inconsistent state. Do not reuse!
        /// </returns>
        internal virtual bool Analyze(DirectoryInfo baseDirectory, FileInfo file)
        {
            #region Sanity checks
            if (baseDirectory == null) throw new ArgumentNullException("baseDirectory");
            if (file == null) throw new ArgumentNullException("file");
            #endregion

            BaseDirectory = baseDirectory;
            RelativePath = file.RelativeTo(BaseDirectory);
            return true;
        }

        /// <summary>
        /// Determines whether a file is executable.
        /// </summary>
        protected bool IsExecutable(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            return
                FileUtils.IsExecutable(path) ||
                FlagUtils.GetFiles(".xbit", BaseDirectory.FullName).Contains(path);
        }

        /// <summary>
        /// The base directory containing the entire application.
        /// </summary>
        [Browsable(false)]
        protected DirectoryInfo BaseDirectory { get; private set; }

        /// <summary>
        /// The path of this entry point relative to <see cref="BaseDirectory"/> using Unix-style directory separators.
        /// </summary>
        [Browsable(false)]
        public string RelativePath { get; internal set; }

        /// <summary>
        /// The application's name.
        /// </summary>
        /// <remarks>A suggestion for <see cref="Feed.Name"/>.</remarks>
        [Category("Basic (required)"), Description("The application's name.")]
        public string Name { get; set; }

        /// <summary>
        /// Short one-line description; the first word should not be upper-case unless it is a proper noun (e.g. "cures all ills").
        /// </summary>
        /// <remarks>A suggestion for <see cref="Feed.Summaries"/>.</remarks>
        [Category("Basic (required)"), Description("Short one-line description; the first word should not be upper-case unless it is a proper noun (e.g. \"cures all ills\").")]
        public string Summary { get; set; }

        /// <summary>
        /// A suggestion for <see cref="Feed.NeedsTerminal"/>.
        /// </summary>
        [Browsable(false)]
        public bool NeedsTerminal { get; internal set; }

        /// <summary>
        /// The application's current version.
        /// </summary>
        /// <remarks>A suggestion for <see cref="Element.Version"/>.</remarks>
        [Category("Basic (required)"), Description("The application's current version.")]
        public ImplementationVersion Version { get; set; }

        /// <summary>
        /// A suggestion for <see cref="TargetBase.Architecture"/>.
        /// </summary>
        [Browsable(false)]
        public Architecture Architecture { get; internal set; }

        /// <summary>
        /// Creates a <see cref="Command"/> to launch this entry point.
        /// </summary>
        [Browsable(false)]
        public abstract Command CreateCommand();

        /// <summary>The <see cref="Command.Name"/> used by <see cref="CreateCommand"/>.</summary>
        protected string CommandName { get { return (Path.GetFileNameWithoutExtension(RelativePath) ?? "unknown").Replace(" ", "-"); } }

        public override string ToString()
        {
            return RelativePath + " (" + GetType().Name + ")";
        }

        #region Equality
        protected bool Equals(Candidate other)
        {
            if (other == null) return false;
            return
                string.Equals(RelativePath, other.RelativePath) &&
                string.Equals(Name, other.Name) &&
                string.Equals(Summary, other.Summary) &&
                Equals(Version, other.Version) &&
                Architecture == other.Architecture &&
                NeedsTerminal == other.NeedsTerminal;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Candidate)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (RelativePath != null ? RelativePath.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Summary != null ? Summary.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Version != null ? Version.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Architecture.GetHashCode();
                hashCode = (hashCode * 397) ^ NeedsTerminal.GetHashCode();
                return hashCode;
            }
        }
        #endregion
    }
}
