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
        /// <summary>The preferences controlling how the <see cref="ISolver"/> evaluates this candidate.</summary>
        private readonly ImplementationPreferences _implementationPreferences;
        #endregion

        #region Properties
        /// <summary>
        /// The implementation this selection candidate references.
        /// </summary>
        internal Implementation Implementation { get; private set; }

        /// <summary>
        /// The file name or URL of the feed listing the implementation.
        /// </summary>
        public string FeedID { get; private set; }

        /// <summary>
        /// The version number of the implementation.
        /// </summary>
        [Description("The version number of the implementation.")]
        public ImplementationVersion Version { get { return Implementation.Version; } }

        /// <summary>
        /// The date this implementation was made available. For development versions checked out from version control this attribute should not be present.
        /// </summary>
        [Description("The date this implementation was made available. For development versions checked out from version control this attribute should not be present.")]
        public DateTime Released { get { return Implementation.Released; } }

        /// <summary>
        /// The default stability rating for this implementation.
        /// </summary>
        [Description("The default stability rating for this implementation.")]
        public Stability Stability { get { return Implementation.Stability; } }

        /// <summary>
        /// A user-specified override for the <see cref="Stability"/> specified in the feed.
        /// </summary>
        [Description("A user-specified override for the stability specified in the feed.")]
        public Stability UserStability { get { return _implementationPreferences.UserStability; } set { _implementationPreferences.UserStability = value; } }

        /// <summary>
        /// The <see cref="UserStability"/> if it is set, otherwise <see cref="Stability"/>.
        /// </summary>
        [Browsable(false)]
        public Stability EffectiveStability { get { return (UserStability == Stability.Unset) ? Stability : UserStability; } }

        /// <summary>
        /// For platform-specific binaries, the platform for which an <see cref="Model.Implementation"/> was compiled, in the form os-cpu. Either the os or cpu part may be *, which will make it available on any OS or CPU. 
        /// </summary>
        [Description("For platform-specific binaries, the platform for which an implementation was compiled, in the form os-cpu. Either the os or cpu part may be *, which will make it available on any OS or CPU. ")]
        public string Architecture { get { return Implementation.Architecture.ToString(); } }

        /// <summary>
        /// Human-readable notes about the implementation, e.g. "not suitable for this architecture".
        /// </summary>
        [Description("Human-readable notes about the implementation, e.g. \"not suitable for this architecture\".")]
        public string Notes { get; internal set; }

        /// <summary>
        /// Indicates wether this implementation fullfills all specified <see cref="Requirements"/>.
        /// </summary>
        [Browsable(false)]
        public bool IsUsable { get; internal set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new selection candidate.
        /// </summary>
        /// <param name="feedID">The file name or URL of the feed listing the implementation.</param>
        /// <param name="implementation">The implementation this selection candidate references.</param>
        /// <param name="implementationPreferences">The preferences controlling how the <see cref="ISolver"/> evaluates this candidate.</param>
        /// <param name="requirements">A set of requirements/restrictions the <paramref name="implementation"/> needs to fullfill for <see cref="IsUsable"/> to be <see langword="true"/>.</param>
        public SelectionCandidate(string feedID, Implementation implementation, ImplementationPreferences implementationPreferences, Requirements requirements)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(feedID)) throw new ArgumentNullException("feedID");
            if (implementation == null) throw new ArgumentNullException("implementation");
            if (implementationPreferences == null) throw new ArgumentNullException("implementationPreferences");
            if (requirements == null) throw new ArgumentNullException("requirements");
            #endregion

            FeedID = feedID;
            Implementation = implementation;
            _implementationPreferences = implementationPreferences;

            if (!implementation.Architecture.IsCompatible(requirements.Architecture))
            {
                Notes = (Implementation.Architecture.Cpu == Cpu.Source)
                    ? Resources.SelectionCandidateNoteSource
                    : Resources.SelectionCandidateNoteIncompatibleArchitecture;
            }
            else if (requirements.NotBeforeVersion != null && Version < requirements.NotBeforeVersion)
                Notes = Resources.SelectionCandidateNoteVersionTooOld;
            else if (requirements.BeforeVersion != null && Version >= requirements.BeforeVersion)
                Notes = Resources.SelectionCandidateNoteVersionTooNew;
            else if (EffectiveStability == Stability.Buggy)
                Notes = Resources.SelectionCandidateNoteBuggy;
            else if (EffectiveStability == Stability.Insecure)
                Notes = Resources.SelectionCandidateNoteInsecure;
            else IsUsable = true;
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the selection candidate in the form "SelectionCandidate: Implementation". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return "SelectionCandidate: " + Implementation;
        }
        #endregion
    }
}
