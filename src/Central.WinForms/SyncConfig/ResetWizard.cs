/*
 * Copyright 2010-2012 Bastian Eicher
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

using Common.Controls;
using ZeroInstall.Injector;

namespace ZeroInstall.Central.WinForms.SyncConfig
{
    /// <summary>
    /// A wizard guiding the user through resetting a lost crypto key or damaged sync data.
    /// </summary>
    /// <remarks>Modifies the default <see cref="Config"/>.</remarks>
    public partial class ResetWizard : Wizard
    {
        public ResetWizard()
        {
            InitializeComponent();
        }
    }
}
