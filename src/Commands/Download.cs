/*
 * Copyright 2010-2011 Bastian Eicher
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
using System.Text;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Fetchers;
using ZeroInstall.Injector;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// This behaves similarly to <see cref="Selection"/>, except that it also downloads the selected versions if they are not already cached.
    /// </summary>
    [CLSCompliant(false)]
    public class Download : Selection
    {
        #region Variables
        /// <summary>Indicates the user wants the implementation locations on the disk.</summary>
        private bool _show;
        #endregion

        #region Properties
        /// <inheritdoc/>
        public override string Name { get { return "download"; } }

        /// <inheritdoc/>
        public override string Description { get { return Resources.DescriptionDownload; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public Download(IHandler handler) : base(handler)
        {
            Options.Add("show", Resources.OptionShow, unused => _show = true);
        }
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        protected override void ExecuteHelper()
        {
            base.ExecuteHelper();

            if (Policy.Preferences.NetworkLevel == NetworkLevel.Offline) return;

            Policy.Fetcher.RunSync(new FetchRequest(Selections.ListUncachedImplementations(Policy)), Handler);
        }

        /// <inheritdoc/>
        public override int Execute()
        {
            if (AdditionalArgs.Count != 0) throw new OptionException(Resources.TooManyArguments, Name);
            ExecuteHelper();

            if (_show)
            {
                // Build a list of all implementation paths
                var builder = new StringBuilder();
                foreach (var implementation in Selections.Implementations)
                    builder.AppendLine(Policy.SearchStore.GetPath(implementation.ManifestDigest));
                builder.Remove(builder.Length - 1, 1); // Remove trailing line-break
                Handler.Inform(Resources.SelectedComponents, builder.ToString());
            }
            else Handler.Inform(Resources.DownloadComplete, Resources.AllComponentsDownloaded);

            return 0;
        }
        #endregion
    }
}
