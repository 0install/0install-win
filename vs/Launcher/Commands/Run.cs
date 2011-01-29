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
using Common.Utils;
using ZeroInstall.Launcher.Properties;
using ZeroInstall.Model;

namespace ZeroInstall.Launcher.Commands
{
    /// <summary>
    /// This behaves similarly to <see cref="Download"/>, except that it also runs the program after ensuring it is in the cache.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class Run : Download
    {
        #region Variables
        /// <summary>An alternative executable to to run from the main <see cref="Model.Implementation"/> instead of <see cref="Element.Main"/>.</summary>
        private string _main;

        /// <summary>Instead of executing the selected program directly, pass it as an argument to this program. Useful for debuggers.</summary>
        private string _wrapper;

        /// <summary>Immediately returns once the chosen program has been launched instead of waiting for it to finish executing.</summary>
        private bool _noWait;
        #endregion

        #region Properties
        /// <inheritdoc/>
        public override string Name { get { return "run"; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public Run(IHandler handler) : base(handler)
        {
            Options.Add("m|main=", Resources.OptionMain, newMain => _main = newMain);
            Options.Add("w|wrapper=", Resources.OptionWrapper, newWrapper => _wrapper = newWrapper);
            Options.Add("no-wait", Resources.OptionNoWait, unused => _noWait = true);
        }
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        public override void Execute()
        {
            base.Execute();

            Handler.CloseAsync();

            var executor = new Executor(Selections, Policy.SearchStore) {Main = _main, Wrapper = _wrapper};
            var startInfo = executor.GetStartInfo(StringUtils.Concatenate(AdditionalArgs, " "));
            if (_noWait) ProcessUtils.RunDetached(startInfo);
            else ProcessUtils.RunReplace(startInfo);
        }
        #endregion
    }
}
