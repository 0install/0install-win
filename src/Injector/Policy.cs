/*
 * Copyright 2010-2011 Bastian Eicher
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using ZeroInstall.Fetchers;
using ZeroInstall.Injector.Feeds;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Injector
{
    /// <summary>
    /// Combines UI access, preferences and resources used to solve dependencies and download implementations.
    /// </summary>
    /// <remarks>This class primarily serves to simplify the initialization process and to reduce the number of arguments that need to be passed into methods.</remarks>
    [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
    [Serializable]
    public class Policy : IEquatable<Policy>, ICloneable
    {
        #region Properties
        /// <summary>
        /// User-preferences controlling network behaviour, etc.
        /// </summary>
        public Preferences Preferences { get; private set; }

        /// <summary>
        /// Allows configuration of the source used to request <see cref="Feed"/>s.
        /// </summary>
        public FeedManager FeedManager { get; private set; }

        /// <summary>
        /// Chooses a set of <see cref="Model.Implementation"/>s to satisfy the requirements of a program and its user. 
        /// </summary>
        public ISolver Solver { get; private set; }

        /// <summary>
        /// Used to download missing <see cref="Model.Implementation"/>s.
        /// </summary>
        public IFetcher Fetcher { get; private set; }

        /// <summary>
        /// A location to search for cached <see cref="Implementation"/>s in addition to <see cref="Fetchers.Fetcher.Store"/>; may be <see langword="null"/>.
        /// </summary>
        /// <remarks>This location searched for <see cref="Implementation"/>s but new ones are not added here.</remarks>
        public IStore AdditionalStore { get; set; }

        /// <summary>
        /// The locations to search for cached <see cref="Implementation"/>s.
        /// </summary>
        public IStore SearchStore
        {
            get
            {
                return (AdditionalStore == null
                    // No additional Store => search in same Stores the Fetcher writes to
                    ? Fetcher.Store
                    // Additional Stores => search in more Stores than the Fetcher writes to
                    : new StoreSet(new[] { AdditionalStore, Fetcher.Store }));
            }
        }

        /// <summary>
        /// A callback object used when the the user needs to be asked questions or is to be about download and IO tasks.
        /// </summary>
        public IHandler Handler { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new policy.
        /// </summary>
        /// <param name="preferences">User-preferences controlling network behaviour, etc.</param>
        /// <param name="feedManager">The source used to request <see cref="Feed"/>s.</param>
        /// <param name="solver">Chooses a set of <see cref="Model.Implementation"/>s to satisfy the requirements of a program and its user.</param>
        /// <param name="fetcher">Used to download missing <see cref="Model.Implementation"/>s.</param>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or is to be about download and IO tasks.</param>
        /// <seealso cref="CreateDefault"/>
        public Policy(Preferences preferences, FeedManager feedManager, ISolver solver, IFetcher fetcher, IHandler handler)
        {
            #region Sanity checks
            if (preferences == null) throw new ArgumentNullException("preferences");
            if (feedManager == null) throw new ArgumentNullException("feedManager");
            if (solver == null) throw new ArgumentNullException("solver");
            if (fetcher == null) throw new ArgumentNullException("fetcher");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            Preferences = preferences;
            FeedManager = feedManager;
            Solver = solver;
            Fetcher = fetcher;
            Handler = handler;
        }
        #endregion

        #region Factory method
        /// <summary>
        /// Creates a new policy using the default <see cref="Preferences"/>, <see cref="FeedCacheProvider"/>, <see cref="SolverProvider"/> and <see cref="FetcherProvider"/>.
        /// </summary>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or is to be about download and IO tasks.</param>
        /// <exception cref="IOException">Thrown if a problem occurred while creating a directory.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a directory is not permitted.</exception>
        public static Policy CreateDefault(IHandler handler)
        {
            return new Policy(Preferences.LoadDefault(), new FeedManager(FeedCacheProvider.Default), SolverProvider.Default, FetcherProvider.Default, handler);
        }
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a semi-deep copy of this <see cref="Policy"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Policy"/>.</returns>
        /// <remarks><see cref="Preferences"/> and <see cref="FeedManager"/> are cloned, <see cref="Solver"/>, <see cref="Fetcher"/> and <see cref="Handler"/> are not.</remarks>
        public Policy ClonePolicy()
        {
            return new Policy(Preferences.ClonePreferences(), FeedManager.CloneFeedManager(), Solver, Fetcher, Handler) {AdditionalStore = AdditionalStore};
        }

        /// <summary>
        /// Creates a semi-deep copy of this <see cref="Policy"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Policy"/>.</returns>
        /// <remarks><see cref="Preferences"/> and <see cref="FeedManager"/> are cloned, <see cref="Fetcher"/> and <see cref="Handler"/> are not.</remarks>
        public object Clone()
        {
            return ClonePolicy();
        }
        #endregion
        
        #region Equality
        /// <inheritdoc/>
        public bool Equals(Policy other)
        {
            if (other == null) return false;

            return Equals(other.Preferences, Preferences) && Equals(other.FeedManager, FeedManager) && Equals(other.Solver, Solver) && Equals(other.Fetcher, Fetcher) && Equals(other.AdditionalStore, AdditionalStore) && Equals(other.Handler, Handler);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(Policy) && Equals((Policy)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = Preferences.GetHashCode();
                result = (result * 397) ^ FeedManager.GetHashCode();
                result = (result * 397) ^ Solver.GetHashCode();
                result = (result * 397) ^ Fetcher.GetHashCode();
                result = (result * 397) ^ (AdditionalStore != null ? AdditionalStore.GetHashCode() : 0);
                result = (result * 397) ^ Handler.GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
