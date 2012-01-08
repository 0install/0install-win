/*
 * Copyright 2010-2012 Bastian Eicher
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
using ZeroInstall.Injector.Feeds;
using ZeroInstall.Injector.Properties;
using ZeroInstall.Model;

namespace ZeroInstall.Injector.Solver
{
    /// <summary>
    /// Represents an <see cref="Implementation"/> that is available to an <see cref="ISolver"/> for selection.
    /// </summary>
    public class SelectionCandidate
    {
        #region Variables
        /// <summary>The implementation this selection candidate references.</summary>
        private readonly Implementation _implementation;

        /// <summary>The preferences controlling how the <see cref="ISolver"/> evaluates this candidate.</summary>
        private readonly ImplementationPreferences _implementationPreferences;
        #endregion

        #region Properties
        /// <summary>
        /// The file name or URL of the feed listing the implementation.
        /// </summary>
        public string FeedID { get; private set; }

        /// <summary>
        /// The version number of the implementation.
        /// </summary>
        [Description("The version number of the implementation.")]
        public ImplementationVersion Version { get { return _implementation.Version; } }

        /// <summary>
        /// The date this implementation was made available. For development versions checked out from version control this attribute should not be present.
        /// </summary>
        [Description("The date this implementation was made available. For development versions checked out from version control this attribute should not be present.")]
        public DateTime Released { get { return _implementation.Released; } }

        /// <summary>
        /// The default stability rating for this implementation.
        /// </summary>
        [Description("The default stability rating for this implementation.")]
        public Stability Stability { get { return _implementation.Stability; } }

        /// <summary>
        /// A user-specified override for the <see cref="Stability"/> specified in the feed.
        /// </summary>
        [Description("A user-specified override for the stability specified in the feed.")]
        public Stability UserStability { get { return _implementationPreferences.UserStability; } set { _implementationPreferences.UserStability = value; } }

        /// <summary>
        /// For platform-specific binaries, the platform for which an <see cref="Model.Implementation"/> was compiled, in the form os-cpu. Either the os or cpu part may be *, which will make it available on any OS or CPU. 
        /// </summary>
        [Description("For platform-specific binaries, the platform for which an implementation was compiled, in the form os-cpu. Either the os or cpu part may be *, which will make it available on any OS or CPU. ")]
        public string Architecture { get { return _implementation.Architecture.ToString(); } }

        /// <summary>
        /// Human-readable notes about the implementation, e.g. "not suitable for this architecture".
        /// </summary>
        [Description("Human-readable notes about the implementation, e.g. \"not suitable for this architecture\".")]
        public string Notes { get; private set; }

        /// <summary>
        /// Indicates wether this implementation can be executed on the current system.
        /// </summary>
        [Browsable(false)]
        public bool IsCompatible { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new selection candidate.
        /// </summary>
        /// <param name="feedID">The file name or URL of the feed listing the implementation.</param>
        /// <param name="implementation">The implementation this selection candidate references.</param>
        /// <param name="implementationPreferences">The preferences controlling how the <see cref="ISolver"/> evaluates this candidate.</param>
        public SelectionCandidate(string feedID, Implementation implementation, ImplementationPreferences implementationPreferences)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(feedID)) throw new ArgumentNullException("feedID");
            if (implementation == null) throw new ArgumentNullException("implementation");
            if (implementationPreferences == null) throw new ArgumentNullException("implementationPreferences");
            #endregion

            FeedID = feedID;
            _implementation = implementation;
            _implementationPreferences = implementationPreferences;
            IsCompatible = implementation.Architecture.IsCompatible(Model.Architecture.CurrentSystem);
            Notes = GetNotes();
        }

        /// <summary>
        /// Creates notes about the implementation's relation to the current system (e.g. why a certain implementation is not be suitable for selection).
        /// </summary>
        private string GetNotes()
        {
            if (_implementation.Architecture.Cpu == Cpu.Source) return Resources.SelectionCandidateNoteSource;
            if (!_implementation.Architecture.IsCompatible(Model.Architecture.CurrentSystem)) return Resources.SelectionCandidateNoteIncompatibleSystem;
            return "None";
        }
        #endregion
    }
}
