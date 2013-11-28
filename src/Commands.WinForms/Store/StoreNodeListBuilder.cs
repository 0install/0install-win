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
using System.IO;
using Common;
using Common.Collections;
using Common.Tasks;
using ZeroInstall.Commands.WinForms.Store.Nodes;
using ZeroInstall.Model;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Management;

namespace ZeroInstall.Commands.WinForms.Store
{
    /// <summary>
    /// Builds a list of <see cref="Node"/>s for <see cref="Feed"/>s and <see cref="Implementation"/>s.
    /// </summary>
    public sealed class StoreNodeListBuilder : ThreadTask
    {
        #region Dependencies
        private readonly IStore _store;
        private readonly IFeedCache _feedCache;
        private readonly StoreManageForm _parent;

        /// <summary>
        /// Creates a new list builder
        /// </summary>
        /// <param name="parent">The window using this builder. Used for callbacks.</param>
        /// <param name="store">Used to list <see cref="Implementation"/>s</param>
        /// <param name="feedCache">Used to load <see cref="Feed"/>s.</param>
        public StoreNodeListBuilder(StoreManageForm parent, IStore store, IFeedCache feedCache)
        {
            #region Sanity checks
            if (parent == null) throw new ArgumentNullException("parent");
            if (store == null) throw new ArgumentNullException("store");
            if (feedCache == null) throw new ArgumentNullException("feedCache");
            #endregion

            _parent = parent;
            _store = store;
            _feedCache = feedCache;
        }
        #endregion

        #region Properties
        /// <inheritdoc/>
        public override string Name { get { return "Loading"; } }

        /// <inheritdoc/>
        public override bool UnitsByte { get { return false; } }

        /// <summary>
        /// All generated nodes.
        /// </summary>
        public NamedCollection<Node> Nodes { get; private set; }

        /// <summary>
        /// The total size of all <see cref="Implementation"/>s in bytes.
        /// </summary>
        public long TotalSize { get; private set; }
        #endregion

        private IEnumerable<Feed> _feeds;

        /// <inheritdoc/>
        protected override void RunTask()
        {
            Nodes = new NamedCollection<Node>();
            _feeds = _feedCache.GetAll();

            foreach (var feed in _feeds) Add(feed);
            foreach (var digest in _store.ListAll()) Add(digest);
            foreach (string path in _store.ListAllTemp()) Add(path);

            lock (StateLock) State = TaskState.Complete;
        }

        private void Add(Feed feed)
        {
            Add(new FeedNode(_parent, _feedCache, feed));
        }

        private void Add(ManifestDigest digest)
        {
            try
            {
                Feed feed;
                var implementation = _feeds.GetImplementation(digest, out feed);

                ImplementationNode implementationNode;
                if (feed == null) implementationNode = new OrphanedImplementationNode(_parent, _store, digest);
                else implementationNode = new OwnedImplementationNode(_parent, _store, digest, new FeedNode(_parent, _feedCache, feed), implementation);

                TotalSize += implementationNode.Size;
                Add(implementationNode);
            }
                #region Error handling
            catch (FormatException ex)
            {
                Msg.Inform(null, string.Format("Problem processing the manifest file for '{0}'.\n" + ex.Message, digest), MsgSeverity.Error);
            }
            catch (IOException ex)
            {
                Msg.Inform(null, string.Format("Problem processing '{0}'.\n" + ex.Message, digest), MsgSeverity.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(null, string.Format("Problem processing '{0}'.\n" + ex.Message, digest), MsgSeverity.Error);
            }
            #endregion
        }

        private void Add(string path)
        {
            Add(new TempDirectoryNode(_parent, _store, path));
        }

        private void Add(Node entry)
        {
            // Avoid name collisions with suffix
            while (Nodes.Contains(entry.Name)) entry.SuffixCounter++;

            Nodes.Add(entry);
        }
    }
}
