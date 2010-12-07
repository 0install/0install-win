/*
 * Copyright 2010 Bastian Eicher
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

namespace ZeroInstall.Model
{
    /// <summary>
    /// A downloadable implementation of a <see cref="Feed"/>.
    /// </summary>
    /// <remarks>An implementation is a specific version of an application, e.g. Fire fox 3.6 for Windows.</remarks>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [Serializable]
    [XmlType("implementation", Namespace = Feed.XmlNamespace)]
    public sealed class Implementation : ImplementationBase
    {
        #region Properties
        // Preserve order
        private readonly C5.ArrayList<RetrievalMethod> _retrievalMethods = new C5.ArrayList<RetrievalMethod>();
        /// <summary>
        /// A list of <see cref="Archive"/>s as <see cref="RetrievalMethod"/>s.
        /// </summary>
        [Category("Retrieval"), Description("A list of archives as retrieval methods.")]
        [XmlElement(typeof(Archive)), XmlElement(typeof(Recipe))]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.ArrayList<RetrievalMethod> RetrievalMethods { get { return _retrievalMethods; } }
        #endregion

        //--------------------//

        #region Simplify
        /// <summary>
        /// Sets missing default values.
        /// </summary>
        /// <remarks>This should be called to prepare an interface for launch.
        /// It should not be called if you plan on serializing the interface again since it will may some of its structure.</remarks>
        public override void Simplify()
        {
            base.Simplify();

            // Simplify retrieval methods
            foreach (var retrievalMethods in RetrievalMethods) retrievalMethods.Simplify();
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
            foreach (var method in RetrievalMethods)
                implementation.RetrievalMethods.Add(method.CloneRetrievalMethod());

            return implementation;
        }

        /// <summary>
        /// Creates a deep copy of this <see cref="Implementation"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Implementation"/>.</returns>
        public override Element CloneElement()
        {
            return CloneImplementation();
        }
        #endregion

        #region Equality
        public bool Equals(Implementation other)
        {
            if (ReferenceEquals(null, other)) return false;

            if (!base.Equals(other)) return false;
            if (!RetrievalMethods.SequencedEquals(other.RetrievalMethods)) return false;
            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(Implementation)) return false;
            return Equals((Implementation)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ RetrievalMethods.GetSequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
