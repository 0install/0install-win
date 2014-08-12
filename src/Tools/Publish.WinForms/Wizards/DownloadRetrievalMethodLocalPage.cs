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
using System.IO;
using System.Net;
using System.Windows.Forms;
using NanoByte.Common;
using NanoByte.Common.Tasks;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Publish.WinForms.Wizards
{
    internal partial class DownloadRetrievalMethodLocalPage<T> : UserControl
        where T : DownloadRetrievalMethod, new()
    {
        public event Action Next;

        private readonly FeedBuilder _feedBuilder;

        public DownloadRetrievalMethodLocalPage(string retrievalMethodType, FeedBuilder feedBuilder)
        {
            _feedBuilder = feedBuilder;
            InitializeComponent();

            labelTitle.Text = string.Format(labelTitle.Text, retrievalMethodType);
            labelQuestion.Text = string.Format(labelQuestion.Text, retrievalMethodType);
            labelUrl.Text = string.Format(labelUrl.Text, retrievalMethodType);
        }

        private void buttonSelectPath_Click(object sender, EventArgs e)
        {
            openFileDialog.FileName = textBoxPath.Text;
            openFileDialog.ShowDialog(this);
        }

        private void openFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            textBoxPath.Text = openFileDialog.FileName;
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            buttonNext.Enabled = (textBoxPath.Text.Length > 0) && (textBoxUrl.Text.Length > 0) && textBoxUrl.IsValid;
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            try
            {
                var retrievalMethod = new T {Href = textBoxUrl.Uri};
                var temporaryDirectory = retrievalMethod.LocalApply(textBoxPath.Text, new GuiTaskHandler(this));

                _feedBuilder.RetrievalMethod = retrievalMethod;
                _feedBuilder.TemporaryDirectory = temporaryDirectory;
                Next();
            }
                #region Error handling
            catch (OperationCanceledException)
            {}
            catch (ArgumentException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Warn);
            }
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Warn);
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Warn);
            }
            catch (WebException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Warn);
            }
            #endregion
        }
    }
}
