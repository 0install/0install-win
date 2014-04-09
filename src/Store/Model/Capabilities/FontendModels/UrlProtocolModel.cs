/*
 * Copyright 2011 Simon E. Silva Lauinger
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

using System.Linq;
using NanoByte.Common.Utils;

namespace ZeroInstall.Store.Model.Capabilities.FontendModels
{
    /// <summary>
    /// Wraps a <see cref="UrlProtocol"/> for data binding.
    /// </summary>
    public class UrlProtocolModel : IconCapabilityModel
    {
        private readonly UrlProtocol _urlProtocol;

        /// <summary>
        /// All <see cref="UrlProtocol.KnownPrefixes"/> concatenated with ", ". If no <see cref="UrlProtocol.KnownPrefixes"/> is available <see cref="Capability.ID"/> will be returned.
        /// </summary>
        public string KnownPrefixes { get { return _urlProtocol.KnownPrefixes.Count == 0 ? Capability.ID : StringUtils.Join(", ", _urlProtocol.KnownPrefixes.Select(extension => extension.Value)); } }

        /// <inheritdoc/>
        public UrlProtocolModel(UrlProtocol capability, bool used) : base(capability, used)
        {
            _urlProtocol = capability;
        }
    }
}
