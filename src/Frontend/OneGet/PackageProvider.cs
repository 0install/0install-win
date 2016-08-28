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

using JetBrains.Annotations;
using NanoByte.Common.Storage;
using PackageManagement.Sdk;
using ZeroInstall.Commands;

namespace ZeroInstall.OneGet
{
    /// <summary>
    /// A OneGet package provider for Zero Install.
    /// </summary>
    [PublicAPI]
    public class PackageProvider : PackageProviderBase
    {
        /// <inheritdoc/>
        protected override string Name => "0install";

        /// <inheritdoc/>
        protected override bool IsDisabled => Locations.IsPortable || ProgramUtils.IsRunningFromCache;

        /// <inheritdoc/>
        protected override IOneGetContext BuildContext(Request request) => new OneGetContext(request);
    }
}
