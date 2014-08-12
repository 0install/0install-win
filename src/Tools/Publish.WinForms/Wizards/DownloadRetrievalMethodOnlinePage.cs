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
    internal partial class DownloadRetrievalMethodOnlinePage<T> : UserControl
        where T : DownloadRetrievalMethod, new()
    {
        public event Action Next;

        private readonly FeedBuilder _feedBuilder;

        public DownloadRetrievalMethodOnlinePage(string retrievalMethodType, FeedBuilder feedBuilder)
        {
            _feedBuilder = feedBuilder;
            InitializeComponent();

            labelTitle.Text = string.Format(labelTitle.Text, retrievalMethodType);
            labelQuestion.Text = string.Format(labelQuestion.Text, retrievalMethodType);
        }

        private void textBoxUrl_TextChanged(object sender, EventArgs e)
        {
            buttonNext.Enabled = (textBoxUrl.Text.Length > 0) && textBoxUrl.IsValid;
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            try
            {
                var retrievalMethod = new T {Href = textBoxUrl.Uri};
                var temporaryDirectory = retrievalMethod.DownloadAndApply(new GuiTaskHandler(this));

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
