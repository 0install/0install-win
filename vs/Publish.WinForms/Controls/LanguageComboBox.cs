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
using System.Globalization;
using System.Windows.Forms;

namespace ZeroInstall.Publish.WinForms.Controls
{
    /// <summary>
    /// A <see cref="ComboBox"/> filled with all <see cref="CultureTypes.SpecificCultures"/> and <see cref="CultureTypes.NeutralCultures"/>.
    /// </summary>
    public class LanguageComboBox : ComboBox
    {
        #region Properties
        /// <summary>
        /// The currently selected language.
        /// </summary>
        public CultureInfo SelectedLanguage
        {
            get
            {
                return (SelectedItem is CultureInfo) ? (CultureInfo) SelectedItem : null;
            }
            set
            {
                if(value != null) SelectedItem = value;
            }
        }
        #endregion

        #region Initialization
        public LanguageComboBox()
        {
            FillLanguages();
        }

        /// <summary>
        /// Clears the items and fills the <see cref="ComboBox"/> with all <see cref="CultureTypes.SpecificCultures"/> and <see cref="CultureTypes.NeutralCultures"/>.
        /// </summary>
        public void FillLanguages()
        {
            BeginUpdate();
            Items.Clear();
            foreach (var language in CultureInfo.GetCultures(CultureTypes.SpecificCultures | CultureTypes.NeutralCultures))
                Items.Add(language);
            EndUpdate();
            SelectedItem = CultureInfo.CurrentCulture;
        }
        #endregion

        #region Add/Remove language
        /// <summary>
        /// Adds a language to the control if it doesn't contains it.
        /// </summary>
        /// <param name="language">The langugage to add.</param>
        public void AddLanguage(CultureInfo language)
        {
            #region sanity checks
            if (language == null) throw new ArgumentNullException("language");
            #endregion

            if (Items.Contains(language)) return;
            Items.Add(language);
        }

        /// <summary>
        /// Removes the given language from the control.
        /// </summary>
        /// <param name="language">The language to remove.</param>
        public void RemoveLanguage(CultureInfo language)
        {
            #region sanity checks
            if (language == null) throw new ArgumentNullException("language");
            #endregion

            Items.Remove(language);
        }
        #endregion
    }
}
