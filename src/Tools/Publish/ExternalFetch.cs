/*
 * Copyright 2010-2017 Bastian Eicher
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
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.Publish.Properties;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Publish
{
    /// <summary>
    /// Uses the "0install fetch" command of the launching 0install instance to download an implementation.
    /// </summary>
    public class ExternalFetch : TaskBase
    {
        private readonly Implementation _implementation;

        /// <summary>
        /// Creates a new external fetch job.
        /// </summary>
        /// <param name="implementation">The implementation to download.</param>
        public ExternalFetch([NotNull] Implementation implementation)
            => _implementation = implementation ?? throw new ArgumentNullException(nameof(implementation));

        /// <inheritdoc/>
        public override string Name => string.Format(Resources.FetchingExternal, _implementation.ID);

        /// <inheritdoc/>
        protected override bool UnitsByte => false;

        /// <inheritdoc/>
        public override bool CanCancel => true;

        /// <inheritdoc/>
        protected override void Execute()
        {
            string externalFetcher = Environment.GetEnvironmentVariable("ZEROINSTALL_EXTERNAL_FETCHER") ?? "0install fetch";
            var parts = WindowsUtils.IsWindows ? WindowsUtils.SplitArgs(externalFetcher) : externalFetcher.Split(new[] {' '}, count: 2);

            var process = new ProcessStartInfo(parts[0], parts.Skip(1).JoinEscapeArguments())
            {
                UseShellExecute = false,
                RedirectStandardInput = true,
                CreateNoWindow = true
            }.Start();

            using (CancellationToken.Register(process.Kill))
            {
                process.StandardInput.WriteLine(new Feed {Elements = {_implementation}}.ToXmlString().Replace("\n", ""));
                process.WaitForExit();
            }

            CancellationToken.ThrowIfCancellationRequested();
        }
    }
}
