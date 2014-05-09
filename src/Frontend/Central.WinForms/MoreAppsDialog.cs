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
using NanoByte.Common.Controls;
using ZeroInstall.Central.Properties;
using ZeroInstall.Commands;

namespace ZeroInstall.Central.WinForms
{
    public partial class MoreAppsDialog : Form
    {
        private readonly bool _machineWide;

        public MoreAppsDialog(bool machineWide)
        {
            InitializeComponent();

            _machineWide = machineWide;
        }

        private void buttonFeed_Click(object sender, EventArgs e)
        {
            string interfaceID = InputBox.Show(this, "Zero Install", Resources.EnterInterfaceUrl);
            if (string.IsNullOrEmpty(interfaceID)) return;

            Program.RunCommand(_machineWide, AddApp.Name, interfaceID);
            Close();
        }

        private void buttonCatalog_Click(object sender, EventArgs e)
        {
            Program.RunCommand(Configure.Name, "--tab=catalog");
            Close();
        }

        private void buttonFeedEditor_Click(object sender, EventArgs e)
        {
            Program.RunCommand(Run.Name, "http://0install.de/feeds/ZeroInstall_Tools.xml");
            Close();
        }
    }
}
