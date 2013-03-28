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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using ZeroInstall.Fetchers;
using ZeroInstall.Injector.Feeds;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Injector
{
    /// <summary>
    /// Combines UI access, configuration and resources used to solve dependencies and download implementations.
    /// </summary>
    /// <remarks>This class serves to simplify finding class dependencies and to reduce the number of arguments that need to be passed into <see cref="IFeedManager"/> and <see cref="ISolver"/> methods.</remarks>
    [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
    [Serializable]
    public class Policy : ICloneable, IEquatable<Policy>
    {
        #region Properties
        /// <summary>
        /// User settings controlling network behaviour, solving, etc.
        /// </summary>
        public Config Config { get; private set; }

        /// <summary>
        /// Allows configuration of the source used to request <see cref="Feed"/>s.
        /// </summary>
        public IFeedManager FeedManager { get; private set; }

        /// <summary>
        /// Used to download missing <see cref="Model.Implementation"/>s.
        /// </summary>
        public IFetcher Fetcher { get; private set; }

        /// <summary>
        /// The OpenPGP-compatible system used to validate new <see cref="Feed"/>s signatures.
        /// </summary>
        public IOpenPgp OpenPgp { get; private set; }

        /// <summary>
        /// Chooses a set of <see cref="Model.Implementation"/>s to satisfy the requirements of a program and its user. 
        /// </summary>
        public ISolver Solver { get; private set; }

        /// <summary>
        /// A callback object used when the the user needs to be asked questions or is to be about download and IO tasks.
        /// </summary>
        public IHandler Handler { get; private set; }

        private int _verbosity;

        /// <summary>
        /// The detail level of messages printed to the console.
        /// 0 = normal, 1 = verbose, 2 = very verbose
        /// </summary>
        public int Verbosity
        {
            get { return _verbosity; }
            set
            {
                _verbosity = value;
                OpenPgp.Verbose = (value >= 1);
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new policy.
        /// </summary>
        /// <param name="config">User settings controlling network behaviour, solving, etc.</param>
        /// <param name="feedManager">The source used to request <see cref="Feed"/>s.</param>
        /// <param name="fetcher">Used to download missing <see cref="Model.Implementation"/>s.</param>
        /// <param name="openPgp">The OpenPGP-compatible system used to validate new <see cref="Feed"/>s signatures.</param>
        /// <param name="solver">Chooses a set of <see cref="Model.Implementation"/>s to satisfy the requirements of a program and its user.</param>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or is to be about download and IO tasks.</param>
        /// <seealso cref="CreateDefault"/>
        public Policy(Config config, IFeedManager feedManager, IFetcher fetcher, IOpenPgp openPgp, ISolver solver, IHandler handler)
        {
            #region Sanity checks
            if (config == null) throw new ArgumentNullException("config");
            if (feedManager == null) throw new ArgumentNullException("feedManager");
            if (fetcher == null) throw new ArgumentNullException("fetcher");
            if (openPgp == null) throw new ArgumentNullException("openPgp");
            if (solver == null) throw new ArgumentNullException("solver");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            Config = config;
            FeedManager = feedManager;
            Fetcher = fetcher;
            OpenPgp = openPgp;
            Solver = solver;
            Handler = handler;
        }
        #endregion

        #region Factory method
        /// <summary>
        /// Creates a new policy using the default <see cref="Config"/>, <see cref="FeedCacheProvider"/>, <see cref="SolverProvider"/> and <see cref="FetcherProvider"/>.
        /// </summary>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or is to be about download and IO tasks.</param>
        /// <exception cref="IOException">Thrown if there was a problem accessing a configuration file or one of the stores.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if access to a configuration file or one of the stores was not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if a configuration file is damaged.</exception>
        public static Policy CreateDefault(IHandler handler)
        {
            return new Policy(
                Config.Load(), new FeedManager(FeedCacheProvider.CreateDefault()),
                FetcherProvider.CreateDefault(), OpenPgpProvider.CreateDefault(), SolverProvider.Default, handler);
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Helper utilitiy that combines <see cref="Selections.GetUncachedImplementations"/> and <see cref="ImplementationSelection.GetOriginalImplementation"/>.
        /// Provides suitable input for <see cref="Fetchers.Fetcher.FetchImplementations"/>.
        /// </summary>
        /// <param name="selections">The <see cref="Selections"/> to scan for uncached implementations.</param>
        /// <returns>Clones of the original <see cref="Implementation"/>s.</returns>
        public IEnumerable<Implementation> GetUncachedImplementations(Selections selections)
        {
            #region Sanity checks
            if (selections == null) throw new ArgumentNullException("selections");
            #endregion

            return selections.GetUncachedImplementations(Fetcher.Store).
                Select(impl => impl.GetOriginalImplementation(FeedManager.Cache).CloneImplementation());
        }
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a semi-deep copy of this <see cref="Policy"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Policy"/>.</returns>
        /// <remarks><see cref="Config"/> and <see cref="FeedManager"/> are cloned, <see cref="Fetcher"/>, <see cref="OpenPgp"/>, <see cref="Solver"/> and <see cref="Handler"/> are not.</remarks>
        public Policy Clone()
        {
            return new Policy(
                Config.Clone(), FeedManager.Clone(),
                Fetcher, OpenPgp, Solver, Handler);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(Policy other)
        {
            if (other == null) return false;
            return Equals(other.Config, Config) && Equals(other.FeedManager, FeedManager) && Equals(other.Fetcher, Fetcher) && Equals(other.Solver, Solver) && Equals(other.Handler, Handler);
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
                int result = Config.GetHashCode();
                result = (result * 397) ^ FeedManager.GetHashCode();
                result = (result * 397) ^ Fetcher.GetHashCode();
                result = (result * 397) ^ Solver.GetHashCode();
                result = (result * 397) ^ Handler.GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
