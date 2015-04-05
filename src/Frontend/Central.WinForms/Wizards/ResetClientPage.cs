/*
 * Copyright 2010-2014 Bastian Eicher
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
using System.Windows.Forms;
using ZeroInstall.Commands.CliCommands;

namespace ZeroInstall.Central.WinForms.Wizards
{
    internal partial class ResetClientPage : UserControl
    {
        public event Action Next;
        private readonly bool _machineWide;

        public ResetClientPage(bool machineWide)
        {
            InitializeComponent();

            _machineWide = machineWide;
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            Program.RunCommand(_machineWide, SyncApps.Name, "--reset=client");
            Next();
        }
    }
}
