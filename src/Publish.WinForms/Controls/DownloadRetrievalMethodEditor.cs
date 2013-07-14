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
using Common;
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
        protected DownloadRetrievalMethodEditor()
        {
            var textBoxUrl = new UriTextBox {HintText = "HTTP/FTP URL"};
            RegisterControl(textBoxUrl, new PropertyPointer<Uri>(() => Target.Href, value => Target.Href = value));

            var textBoxSize = new HintTextBox {HintText = "in bytes", Location = new Point(0, textBoxUrl.Bottom)};
            RegisterControl(textBoxSize, new PropertyPointer<long>(() => Target.Size, value => Target.Size = value).ToStringPointer());
        }
    }
}
