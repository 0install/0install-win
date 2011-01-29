﻿/*
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
using System.Runtime.Serialization;
using System.Security.Permissions;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Feeds
{
    /// <summary>
    /// Indicates a feed file that was supposed to be added to the <see cref="IFeedCache"/> is older than a version already located in the cache.
    /// </summary>
    /// <remarks>By adding a </remarks>
    [Serializable]
    public sealed class ReplayAttackException : Exception
    {
        #region Properties
        /// <summary>
        /// The URL of the feed file to be added to the cache.
        /// </summary>
        public Uri FeedUrl { get; private set; }

        /// <summary>
        /// The last changed time stamp of the existing file in the cache.
        /// </summary>
        public DateTime OldTime { get; private set; }

        /// <summary>
        /// The last changed time stamp of the new file to be added.
        /// </summary>
        public DateTime NewTime { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new replay attack exception.
        /// </summary>
        /// <param name="feedUrl">The URL of the feed file to be added to the cache.</param>
        /// <param name="oldTime">The last changed time stamp of the existing file in the cache.</param>
        /// <param name="newTime">The last changed time stamp of the new file to be added.</param>
        public ReplayAttackException(Uri feedUrl, DateTime oldTime, DateTime newTime)
            : base(string.Format(Resources.ReplayAttack, feedUrl, oldTime, newTime))
        {
            FeedUrl = feedUrl;
            OldTime = oldTime;
            NewTime = newTime;
        }

        /// <inheritdoc/>
        public ReplayAttackException()
            : base(string.Format(Resources.ReplayAttack, "unknown", "unknown", "unknown"))
        {}

        /// <inheritdoc/>
        public ReplayAttackException(string message) : base(message) 
        {}

        /// <inheritdoc/>
        public ReplayAttackException(string message, Exception innerException) : base (message, innerException)
        {}

        /// <summary>
        /// Deserializes an exception.
        /// </summary>
        private ReplayAttackException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            #region Sanity checks
            if (info == null) throw new ArgumentNullException("info");
            #endregion

            FeedUrl = new Uri(info.GetString("FeedUrl"));
            OldTime = info.GetDateTime("OldTime");
            NewTime = info.GetDateTime("NewTime");
        }
        #endregion

        #region Serialization
        /// <inheritdoc/>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            #region Sanity checks
            if (info == null) throw new ArgumentNullException("info");
            #endregion

            info.AddValue("FeedUrl", FeedUrl.ToString());
            info.AddValue("OldTime", OldTime);
            info.AddValue("NewTime", NewTime);

            base.GetObjectData(info, context);
        }
        #endregion
    }
}
