/*
 * Copyright 2010-2015 Bastian Eicher
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
using NanoByte.Common;
using NanoByte.Common.Controls;
using ZeroInstall.Publish.Properties;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Publish.WinForms.Controls
{
    /// <summary>
    /// A common base for <see cref="DownloadRetrievalMethod"/> editors.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="DownloadRetrievalMethod"/> to edit.</typeparam>
    public abstract class DownloadRetrievalMethodEditor<T> : RetrievalMethodEditor<T>
        where T : DownloadRetrievalMethod
    {
        #region Constructor
        protected DownloadRetrievalMethodEditor()
        {
            Controls.Add(new Label
            {
                Location = new System.Drawing.Point(0, 3),
                AutoSize = true,
                TabIndex = 0,
                Text = "Source URL:"
            });

            var textBoxHref = new UriTextBox
            {
                Location = new System.Drawing.Point(77, 0),
                Size = new System.Drawing.Size(73, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                TabIndex = 1,
                HintText = "HTTP/FTP URL (required)",
                AllowRelative = true
            };
            textBoxHref.TextChanged += delegate { ShowUpdateHint(Resources.ManifestDigestChanged); };
            RegisterControl(textBoxHref, new PropertyPointer<Uri>(() => Target.Href, value => Target.Href = value));

            Controls.Add(new Label
            {
                Location = new System.Drawing.Point(0, 29),
                AutoSize = true,
                TabIndex = 2,
                Text = "File size:"
            });

            var textBoxSize = new HintTextBox
            {
                Location = new System.Drawing.Point(77, 26),
                Size = new System.Drawing.Size(73, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                TabIndex = 3,
                HintText = "in bytes (required)"
            };
            RegisterControl(textBoxSize, new PropertyPointer<long>(() => Target.Size, value => Target.Size = value).ToStringPointer());
        }
        #endregion

        /// <inheritdoc/>
        protected override void UpdateHint()
        {
            if (Target.Size == 0) ShowUpdateHint(Resources.SizeMissing);
            else base.UpdateHint();
        }
    }
}
