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
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Fetchers;
using ZeroInstall.Injector;
using ZeroInstall.Injector.Solver;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// This behaves similarly to <see cref="Selection"/>, except that it also downloads the selected versions if they are not already cached.
    /// </summary>
    [CLSCompliant(false)]
    public class Download : Selection
    {
        #region Variables
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "download";
        
        /// <summary>Indicates the user wants the implementation locations on the disk.</summary>
        private bool _show;
        #endregion

        #region Properties
        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionDownload; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public Download(IHandler handler, Policy policy, ISolver solver) : base(handler, policy, solver)
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
            if (AdditionalArgs.Count != 0) throw new OptionException(Resources.TooManyArguments + "\n" + AdditionalArgs, "");
            ExecuteHelper();

            if (_show)
            {
                if (ShowXml) Handler.Output("Selections XML:", Selections.WriteToString());
                else Handler.Output(Resources.SelectedImplementations, Selections.GetHumanReadable(Policy.SearchStore));
            }
            else Handler.Output(Resources.DownloadComplete, Resources.AllComponentsDownloaded);
            return 0;
        }
        #endregion
    }
}
