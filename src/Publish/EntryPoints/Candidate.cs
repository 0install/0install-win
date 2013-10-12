/*
 * Copyright 2010-2013 Bastian Eicher
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
using Common.Utils;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Publish.EntryPoints
{
    /// <summary>
    /// Collects information about a potential candidate for an entry point.
    /// The subclass type determines the type of executable (native binary, interpreted script, etc.).
    /// </summary>
    public abstract class Candidate
    {
        #region Analyze
        /// <summary>
        /// Analyzes a file to determine whether it matches this candidate type and extracts meta data.
        /// </summary>
        /// <param name="file"></param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="file"/> matches this candidate type. The object will then contain all available metadata.
        /// <see langword="false"/> if <paramref name="file"/>does not match this candidate type. The object will then be in an inconsistent state. Do not reuse!
        /// </returns>
        internal virtual bool Analyze(FileInfo file)
        {
            #region Sanity checks
            if (file == null) throw new ArgumentNullException("file");
            #endregion

            RelativePath = file.RelativeTo(BaseDirectory);
            return true;
        }
        #endregion

        #region Helpers
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
                FlagUtils.GetExternalFlags(".xbit", BaseDirectory.FullName).Contains(path);
        }
        #endregion

        #region Properties
        /// <summary>
        /// The base directory containing the entire application.
        /// </summary>
        [Browsable(false)]
        public DirectoryInfo BaseDirectory { get; internal set; }

        /// <summary>
        /// The path of this entry point relative to <see cref="BaseDirectory"/> using Unix-style directory separators.
        /// </summary>
        [Browsable(false)]
        public string RelativePath { get; internal set; }

        /// <summary>
        /// A suggestion for <see cref="TargetBase.Architecture"/> extracted from the entry point metadata.
        /// </summary>
        [Browsable(false)]
        public Architecture Architecture { get; internal set; }

        /// <summary>
        /// A suggestion for <see cref="Feed.Name"/> extracted from the entry point metadata.
        /// </summary>
        [Browsable(false)]
        public string Name { get; internal set; }

        /// <summary>
        /// A suggestion for <see cref="Feed.Summaries"/> extracted from the entry point metadata.
        /// </summary>
        [Browsable(false)]
        public string Description { get; internal set; }

        /// <summary>
        /// A suggestion for <see cref="Feed.NeedsTerminal"/> extracted from the entry point metadata.
        /// </summary>
        [Browsable(false)]
        public bool NeedsTerminal { get; internal set; }

        /// <summary>
        /// A suggestion for <see cref="Element.Version"/> extracted from the entry point metadata.
        /// </summary>
        [Browsable(false)]
        public ImplementationVersion Version { get; internal set; }

        /// <summary>
        /// A <see cref="Command"/> to launch this entry point.
        /// </summary>
        [Browsable(false)]
        public abstract Command Command { get; }
        #endregion

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
                string.Equals(Description, other.Description) &&
                NeedsTerminal == other.NeedsTerminal &&
                Equals(Version, other.Version) &&
                Architecture == other.Architecture;
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
                hashCode = (hashCode * 397) ^ (Description != null ? Description.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ NeedsTerminal.GetHashCode();
                hashCode = (hashCode * 397) ^ (Version != null ? Version.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Architecture.GetHashCode();
                return hashCode;
            }
        }
        #endregion
    }
}
