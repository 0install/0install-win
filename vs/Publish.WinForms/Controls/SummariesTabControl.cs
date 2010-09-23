/*
 * Copyright 2010 Simon E. Silva Lauinger
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
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Common.Collections;
using Common.Controls;

namespace ZeroInstall.Publish.WinForms.Controls
{
    public partial class SummariesTabControl : UserControl
    {
        #region Attributes
        private LocalizableStringCollection _summaries = new LocalizableStringCollection();
        #endregion

        #region Properties
        public LocalizableStringCollection Summaries
        {
            set
            {
                _summaries = value ?? new LocalizableStringCollection();

                // remove all tab pages except the last one.
                for (int i = 0; i < addRemoveTabControl1.tabControl1.TabPages.Count - 1; i++)
                {
                    addRemoveTabControl1.tabControl1.TabPages.RemoveAt(0);
                }

                foreach (var summary in _summaries)
                {
                    addRemoveTabControl1.tabControl1.TabPages.Insert(0, CreateTabPage(summary));
                }
            }
            get
            {
                return _summaries;
            }
        }
        #endregion

        #region Initialization
        public SummariesTabControl()
        {
            InitializeComponent();
        }
        #endregion

        private static TabPage CreateTabPage(LocalizableString summary)
        {
            var control = new SummariesControl();
            control.languageComboBox.comboBoxLanguages.SelectedItem = summary.Language;
            control.hintTextBoxSummary.Text = summary.Value;

            var newTabPage = new TabPage {Text = summary.Language.NativeName};
            newTabPage.Controls.Add(control);

            return newTabPage;
        }

        private void AddRemoveTabControl1NewTabCreated(TabControl sender, TabPage createdTabPage)
        {
            //createdTabPage.Controls.Add(CreateSummaryHintTextBox(String.Empty));
            //createdTabPage.Controls.Add(CreateLanguageComboBox(CultureInfo.CurrentCulture));
        }
    }
}
