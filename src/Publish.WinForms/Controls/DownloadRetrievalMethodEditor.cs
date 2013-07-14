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
using System.Drawing;
using System.Globalization;
using Common.Controls;
using ZeroInstall.Model;

namespace ZeroInstall.Publish.WinForms.Controls
{
    /// <summary>
    /// A common base for <see cref="DownloadRetrievalMethod"/> editors.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="DownloadRetrievalMethod"/> to edit.</typeparam>
    public abstract class DownloadRetrievalMethodEditor<T> : RetrievalMethodEditor<T>
        where T : DownloadRetrievalMethod
    {
        private readonly UriTextBox _textBoxUrl;
        private readonly HintTextBox _textBoxSize;

        protected DownloadRetrievalMethodEditor()
        {
            _textBoxUrl = new UriTextBox {HintText = "HTTP/FTP URL"};
            _textBoxUrl.Validated += UpdateHref;
            Controls.Add(_textBoxUrl);

            _textBoxSize = new HintTextBox {HintText = "in bytes", Location = new Point(0, _textBoxUrl.Bottom)};
            Controls.Add(_textBoxSize);
        }

        private void UpdateHref(object sender, EventArgs e)
        {
            if (!_textBoxUrl.IsValid || _textBoxUrl.Uri == Target.Href) return;

            if (CommandExecutor == null) Target.Href = _textBoxUrl.Uri;
            else CommandExecutor.Execute(new Common.Undo.SetValueCommand<Uri>(() => Target.Href, value => Target.Href = value, _textBoxUrl.Uri));
        }

        public override void Refresh()
        {
            _textBoxUrl.Uri = Target.Href;
            _textBoxSize.Text = Target.Size.ToString(CultureInfo.InvariantCulture);
            base.Refresh();
        }
    }
}
