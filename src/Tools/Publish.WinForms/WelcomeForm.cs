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
using JetBrains.Annotations;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Publish.WinForms
{
    /// <summary>
    /// The welcome window for the Zero Install Publishing Tools.
    /// </summary>
    internal partial class WelcomeForm : Form
    {
        #region Dependencies
        private readonly IOpenPgp _openPgp;

        /// <summary>
        /// Creates a new welcome form.
        /// </summary>
        /// <param name="openPgp">The OpenPGP-compatible system used to create signatures.</param>
        public WelcomeForm([NotNull] IOpenPgp openPgp)
        {
            #region Sanity checks
            if (openPgp == null) throw new ArgumentNullException("openPgp");
            #endregion

            InitializeComponent();

            _openPgp = openPgp;
        }
        #endregion

        private void buttonNewEmpty_Click(object sender, EventArgs e)
        {
            SwitchToMain(new FeedEditing());
        }

        private void buttonNewWizard_Click(object sender, EventArgs e)
        {
            var result = Wizards.NewFeedWizard.Run(_openPgp, this);
            if (result != null) SwitchToMain(new FeedEditing(result));
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            try
            {
                SwitchToMain(MainForm.OpenFeed(this));
            }
            catch (OperationCanceledException)
            {}
        }

        private void SwitchToMain(FeedEditing feedEditing)
        {
            using (var form = new MainForm(feedEditing, _openPgp))
            {
                Hide();
                form.ShowDialog();
                Close();
            }
        }
    }
}
