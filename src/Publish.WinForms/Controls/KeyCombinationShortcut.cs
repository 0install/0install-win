/*
 * Copyright 2011 Simon E. Silva Lauinger
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
using System.ComponentModel;
using System.Windows.Forms;
using C5;
using Common;

namespace ZeroInstall.Publish.WinForms.Controls
{
    /// <summary>
    /// This class handles key combination shortcuts. After you linked some <see cref="Keys"/> with a method,
    /// the method will be raised after the key was pressed.
    /// </summary>
    public partial class KeyCombinationShortcut : Component
    {
        #region Variables
        /// <summary>
        /// Stores the shortcuts as keys and the associated events as values. 
        /// </summary>
        private readonly HashDictionary<Keys, SimpleEventHandler> _shortcuts = new HashDictionary<Keys, SimpleEventHandler>();
        #endregion

        #region Constructor
        public KeyCombinationShortcut()
        {
            InitializeComponent();
        }

        public KeyCombinationShortcut(IContainer container)
        {
            #region Sanity checks
            if (container == null) throw new ArgumentNullException("container");
            #endregion

            container.Add(this);

            InitializeComponent();
        }
        #endregion

        //--------------------//

        #region KeyDown
        /// <summary>
        /// Method to register as event handler for <see cref="Control.KeyDown"/>. 
        /// </summary>
        /// <param name="sender">not used.</param>
        /// <param name="e">not used.</param>
        public void KeyDown(object sender, KeyEventArgs e)
        {
            #region Sanity checks
            if (e == null) throw new ArgumentNullException("e");
            #endregion

            if (_shortcuts.Contains(e.KeyData)) _shortcuts[e.KeyData]();
        }
        #endregion

        #region Link/Unlink
        /// <summary>
        /// Links a key combination shortcut with a method. The method will be executed when the shortcut was pressed.
        /// </summary>
        /// <param name="shortcut">That shall execute a method.</param>
        /// <param name="toExecute">Method that will be executed.</param>
        public void LinkShortcut(Keys shortcut, SimpleEventHandler toExecute)
        {
            #region Sanity checks
            if (toExecute == null) throw new ArgumentNullException("toExecute");
            #endregion

            _shortcuts.Add(shortcut, toExecute);
        }

        /// <summary>
        /// Unlinks a key combination shortcut.
        /// </summary>
        /// <param name="shortcut"></param>
        public void UnlinkShortcut(Keys shortcut)
        {
            _shortcuts.Remove(shortcut);
        }
        #endregion
    }
}
