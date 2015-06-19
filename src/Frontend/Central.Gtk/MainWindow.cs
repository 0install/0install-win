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
using System.IO;
using Gtk;

namespace ZeroInstall.Central.Gtk
{
    public partial class MainWindow : Window
    {
        /// <summary>Apply operations machine-wide instead of just for the current user.</summary>
        private readonly bool _machineWide;

        /// <summary>
        /// Initializes the main GUI.
        /// </summary>
        /// <param name="machineWide">Apply operations machine-wide instead of just for the current user.</param>
        /// <exception cref="IOException">Failed to read a config file.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to a configuration file was not permitted.</exception>
        /// <exception cref="InvalidDataException">The config data is damaged.</exception>
        public MainWindow(bool machineWide) : base(WindowType.Toplevel)
        {
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            Build();

            _machineWide = machineWide;
        }
    }
}
