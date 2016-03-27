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

using System.Collections.Generic;
using NanoByte.Common;
using NanoByte.Common.Native;
using PackageManagement.Sdk;
using ZeroInstall.Commands;
using ZeroInstall.Commands.Properties;

namespace ZeroInstall.OneGet
{
    /// <summary>
    /// A machine-wide OneGet package provider for Zero Install.
    /// </summary>
    public class PackageProviderMachineWide : PackageProviderBase
    {
        private bool IsDisabled { get { return ProgramUtils.IsRunningFromPerUserDir; } }

        public override string PackageProviderName { get { return IsDisabled ? "0install-Machine-disabled" : "0install-Machine"; } }

        public override void GetFeatures(Request request)
        {
            base.GetFeatures(request);

            if (IsDisabled) request.Yield(new KeyValuePair<string, string[]>(Constants.Features.AutomationOnly, Constants.FeaturePresent));
        }

        protected override OneGetCommand BuildCommand(Request request)
        {
            if (!WindowsUtils.IsAdministrator) throw new NotAdminException(Resources.MustBeAdminForMachineWide);
            return new OneGetCommand(request, machineWide: true);
        }
    }
}
