/*
 * Copyright 2010-2016 Bastian Eicher
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
using NanoByte.Common;
using ZeroInstall.Publish.Properties;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Publish.WinForms.Controls
{
    /// <summary>
    /// Edits <see cref="Archive"/> instances.
    /// </summary>
    public partial class ArchiveEditor : ArchiveEditorShim
    {
        public ArchiveEditor()
        {
            InitializeComponent();

            RegisterControl(comboBoxMimeType, new PropertyPointer<string>(() => Target.MimeType, value => Target.MimeType = value));
            RegisterControl(textBoxExtract, new PropertyPointer<string>(() => Target.Extract, value => Target.Extract = value));
            RegisterControl(textBoxDestination, new PropertyPointer<string>(() => Target.Destination, value => Target.Destination = value));

            // ReSharper disable once CoVariantArrayConversion
            comboBoxMimeType.Items.AddRange(Archive.KnownMimeTypes);
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            ShowUpdateHint(Resources.ManifestDigestChanged);
        }
    }

    /// <summary>
    /// Non-generic base class for <see cref="ArchiveEditor"/>, because WinForms editor cannot handle generics.
    /// </summary>
    public class ArchiveEditorShim : DownloadRetrievalMethodEditor<Archive>
    {}
}
