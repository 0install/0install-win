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
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NanoByte.Common;
using NanoByte.Common.Controls;
using NanoByte.Common.Tasks;
using NanoByte.Common.Utils;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Commands.WinForms.StoreManagementNodes
{
    /// <summary>
    /// Models information about a <see cref="Feed"/> in the <see cref="IFeedCache"/> for display in a GUI.
    /// </summary>
    public sealed class FeedNode : Node, IContextMenu
    {
        #region Variables
        private readonly IFeedCache _cache;
        private readonly Feed _feed;
        #endregion

        #region Properties
        /// <inheritdoc/>
        public override string Name { get { return _feed.Name + (SuffixCounter == 0 ? "" : " " + SuffixCounter); } set { throw new NotSupportedException(); } }

        /// <summary>
        /// The URI indentifying this feed.
        /// </summary>
        [Description("The URI indentifying this feed.")]
        public Uri Uri { get { return _feed.Uri; } }

        /// <summary>
        /// The main website of the application.
        /// </summary>
        [Description("The main website of the application.")]
        public Uri Homepage { get { return _feed.Homepage; }}

        /// <summary>
        /// A short one-line description of the application.
        /// </summary>
        [Description("A short one-line description of the application.")]
        public string Summary { get { return _feed.GetBestSummary(CultureInfo.CurrentUICulture); } }

        /// <summary>
        /// A comma-separated list of categories the applications fits into.
        /// </summary>
        [Description("A comma-separated list of categories the applications fits into.")]
        public string Categories { get { return StringUtils.Join(",", _feed.Categories.Select(x => x.Name)); } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new feed node.
        /// </summary>
        /// <param name="feed">The <see cref="Feed"/> to be represented by this node.</param>
        /// <param name="cache">The <see cref="IFeedCache"/> the <see cref="Feed"/> is located in.</param>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about IO tasks.</param>
        public FeedNode(Feed feed, IFeedCache cache, ITaskHandler handler)
            : base(handler)
        {
            #region Sanity checks
            if (cache == null) throw new ArgumentNullException("cache");
            if (feed == null) throw new ArgumentNullException("feed");
            #endregion

            _cache = cache;
            _feed = feed;
        }
        #endregion

        //--------------------//

        #region Delete
        /// <summary>
        /// Deletes this <see cref="Feed"/> from the <see cref="IFeedCache"/> it is located in.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown if no matching feed could be found in the <see cref="IFeedCache"/>.</exception>
        /// <exception cref="IOException">Thrown if the feed could not be deleted.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the cache is not permitted.</exception>
        public override void Delete()
        {
            _cache.Remove(_feed.UriString);
        }
        #endregion

        #region Verify
        /// <summary>
        /// Does nothing.
        /// </summary>
        public override void Verify()
        {}
        #endregion

        #region Context menu
        /// <inheritdoc/>
        public ContextMenu GetContextMenu()
        {
            return new ContextMenu(new[]
            {
                new MenuItem(Resources.Remove, delegate
                {
                    if (Handler.AskQuestion(Resources.DeleteEntry))
                    {
                        try
                        {
                            Delete();
                        }
                            #region Error handling
                        catch (KeyNotFoundException ex)
                        {
                            Msg.Inform(null, ex.Message, MsgSeverity.Error);
                        }
                        catch (IOException ex)
                        {
                            Msg.Inform(null, ex.Message, MsgSeverity.Error);
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            Msg.Inform(null, ex.Message, MsgSeverity.Error);
                        }
                        #endregion

                        Handler.Batch = true;
                    }
                })
            });
        }
        #endregion
    }
}
