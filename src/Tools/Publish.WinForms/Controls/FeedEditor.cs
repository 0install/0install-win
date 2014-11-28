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
using NanoByte.Common;
using NanoByte.Common.Controls;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Publish.WinForms.Controls
{
    /// <summary>
    /// Edits <see cref="Feed"/> instances.
    /// </summary>
    public partial class FeedEditor : FeedEditorShim
    {
        public FeedEditor()
        {
            InitializeComponent();

            RegisterControl(textBoxName, new PropertyPointer<string>(() => Target.Name, value => Target.Name = value));
            RegisterControl(textBoxUri, new PropertyPointer<Uri>(() => Target.Uri, value => Target.Uri = new FeedUri(value)));
            RegisterControl(textBoxDescription, () => Target.Descriptions);
            RegisterControl(textBoxSummary, () => Target.Summaries);
            RegisterControl(textBoxHomepage, new PropertyPointer<Uri>(() => Target.Homepage, value => Target.Homepage = value));
            RegisterControl(checkBoxNeedTerminal, new PropertyPointer<bool>(() => Target.NeedsTerminal, value => Target.NeedsTerminal = value));
            RegisterControl(comboBoxMinimumZeroInstallVersion, new PropertyPointer<string>(() => Target.MinInjectorVersionString, value => Target.MinInjectorVersionString = value));
        }
    }

    /// <summary>
    /// Non-generic base class for <see cref="FeedEditor"/>, because WinForms editor cannot handle generics.
    /// </summary>
    public class FeedEditorShim : EditorControlBase<Feed>
    {}
}
