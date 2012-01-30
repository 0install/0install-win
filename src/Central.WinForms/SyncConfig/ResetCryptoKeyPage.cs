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

using System;
using System.Windows.Forms;

namespace ZeroInstall.Central.WinForms.SyncConfig
{
    internal partial class ResetCryptoKeyPage : UserControl
    {
        public event Action<string> Continue;

        public ResetCryptoKeyPage()
        {
            InitializeComponent();
        }

        private void textBoxCryptoKey_TextChanged(object sender, EventArgs e)
        {
            buttonContinue.Enabled = !string.IsNullOrEmpty(textBoxCryptoKey.Text);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            // ToDo

            Continue(textBoxCryptoKey.Text);
        }
    }
}
