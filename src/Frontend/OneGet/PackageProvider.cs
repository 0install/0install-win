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

using PackageManagement.Sdk;

namespace ZeroInstall.OneGet
{
    /// <summary>
    /// A per-user OneGet package provider for Zero Install.
    /// </summary>
    public class PackageProvider : PackageProviderBase
    {
        public override string PackageProviderName { get { return "0install"; } }

        protected override OneGetCommand BuildCommand(Request request)
        {
            return new OneGetCommand(request);
        }
    }
}
