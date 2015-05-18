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

using System.Linq;
using ZeroInstall.Publish.Properties;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Publish.WinForms.Controls
{
    /// <summary>
    /// Edits <see cref="Recipe"/> instances.
    /// </summary>
    public partial class RecipeEditor : RecipeEditorShim
    {
        public RecipeEditor()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        protected override void UpdateHint()
        {
            if (Target.Steps.OfType<DownloadRetrievalMethod>().Any(x => x.Size == 0)) ShowUpdateHint(Resources.SizeMissing);
            else base.UpdateHint();
        }
    }

    /// <summary>
    /// Non-generic base class for <see cref="RecipeEditor"/>, because WinForms editor cannot handle generics.
    /// </summary>
    public class RecipeEditorShim : RetrievalMethodEditor<Recipe>
    {}
}
