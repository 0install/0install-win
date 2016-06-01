/*
 * Copyright 2010-2016 Bastian Eicher
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

using NanoByte.Common;
using NanoByte.Common.Streams;
using ZeroInstall.Store;

namespace ZeroInstall.Bootstrap
{
    /// <summary>
    /// Represents configuration embedded into the exectuable itself.
    /// This is used to create customized bootstrappers that use Zero Install to run or integrate another application.
    /// </summary>
    public class EmbeddedConfig
    {
        /// <summary>
        /// The name of the target application to bootstrap.
        /// Only relevant if <see cref="AppMode"/> is not <see cref="Bootstrap.AppMode.None"/>.
        /// </summary>
        public string AppName { get; private set; }

        /// <summary>
        /// The feed URI of the target application to bootstrap.
        /// Only relevant if <see cref="AppMode"/> is not <see cref="Bootstrap.AppMode.None"/>.
        /// </summary>
        public FeedUri AppUri { get; private set; }

        /// <summary>
        /// The application bootstrapping mode to use.
        /// </summary>
        public AppMode AppMode { get; private set; }

        /// <summary>
        /// Loads the embedded configuration.
        /// </summary>
        private EmbeddedConfig()
        {
            var lines = typeof(EmbeddedConfig).GetEmbeddedString("EmbeddedConfig.txt").SplitMultilineText();
            AppName = lines[0].TrimEnd();
            AppUri = new FeedUri(lines[1].TrimEnd());
            AppMode = GetAppMode(lines[2].TrimEnd());
        }

        private static AppMode GetAppMode(string value)
        {
            switch (value)
            {
                case "run":
                    return AppMode.Run;
                case "integrate":
                    return AppMode.Integrate;
                default:
                    return AppMode.None;
            }
        }

        /// <summary>
        /// The embedded config as a singleton.
        /// </summary>
        public static readonly EmbeddedConfig Instance = new EmbeddedConfig();
    }
}