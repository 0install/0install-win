/*
 * Copyright 2010-2013 Bastian Eicher
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
using Common;
using ZeroInstall.Publish.Properties;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Publish.WinForms.Wizards
{
    internal partial class SecurityPage : UserControl
    {
        public event Action<Uri, OpenPgpSecretKey> SecurityDataSet; 

        public SecurityPage()
        {
            InitializeComponent();
        }

        private void buttonSkip_Click(object sender, EventArgs e)
        {
            //if (!Msg.YesNo(this, Resources.AskSkipSecurity, MsgSeverity.Info)) return;
            //SecurityDataSet(null, null);
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            //SecurityDataSet(new Uri(), new OpenPgpSecretKey());
        }
    }
}
