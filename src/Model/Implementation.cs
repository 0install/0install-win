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
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using Common.Collections;

namespace ZeroInstall.Model
{
    /// <summary>
    /// An implementation is a specific version of an application that can be downloaded and executed (e.g. Firefox 3.6 for Windows).
    /// </summary>
    /// <seealso cref="Feed.Elements"/>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [Description("An implementation is a specific version of an application that can be downloaded and executed (e.g. Firefox 3.6 for Windows).")]
    [Serializable]
    [XmlRoot("implementation", Namespace = Feed.XmlNamespace), XmlType("implementation", Namespace = Feed.XmlNamespace)]
    public sealed class Implementation : ImplementationBase, IEquatable<Implementation>
    {
        #region Properties
        // Preserve order
        private readonly C5.ArrayList<RetrievalMethod> _retrievalMethods = new C5.ArrayList<RetrievalMethod>();

        /// <summary>
        /// A list of <see cref="Archive"/>s as <see cref="RetrievalMethod"/>s.
        /// </summary>
        [Browsable(false)]
        [XmlElement(typeof(Archive)), XmlElement(typeof(SingleFile)), XmlElement(typeof(Recipe))]
        public C5.ArrayList<RetrievalMethod> RetrievalMethods { get { return _retrievalMethods; } }
        #endregion

        //--------------------//

        #region Normalize
        /// <summary>
        /// Sets missing default values and handles legacy elements.
        /// </summary>
        /// <param name="feedID">The feed the data was originally loaded from.</param>
        /// <remarks>This method should be called to prepare a <see cref="Feed"/> for solver processing.
        /// It should not be called if you plan on serializing the interface again since it may change some of its structure.</remarks>
        public override void Normalize(string feedID)
        {
            base.Normalize(feedID);

            // Simplify retrieval methods and rebuild list to update sequenced hash value
            var newRetreivalMethods = new RetrievalMethod[RetrievalMethods.Count];
            int i = 0;
            foreach (var retrievalMethods in RetrievalMethods)
            {
                retrievalMethods.Normalize();
                newRetreivalMethods[i++] = retrievalMethods;
            }
            RetrievalMethods.Clear();
            RetrievalMethods.AddAll(newRetreivalMethods);
        }
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Implementation"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Implementation"/>.</returns>
        public Implementation CloneImplementation()
        {
            var implementation = new Implementation();
            CloneFromTo(this, implementation);
            implementation.RetrievalMethods.AddAll(RetrievalMethods.CloneElements());
            return implementation;
        }

        /// <summary>
        /// Creates a deep copy of this <see cref="Implementation"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Implementation"/>.</returns>
        public override Element Clone()
        {
            return CloneImplementation();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(Implementation other)
        {
            if (other == null) return false;
            return base.Equals(other) && RetrievalMethods.SequencedEquals(other.RetrievalMethods);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(Implementation)) return false;
            return Equals((Implementation)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ RetrievalMethods.GetSequencedHashCode();
            }
        }
        #endregion
    }
}
