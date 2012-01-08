/*
 * Copyright 2010-2012 Bastian Eicher
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
using Common.Controls;
using Common.Tasks;

namespace ZeroInstall.Commands.WinForms
{
    /// <summary>
    /// Combines a <see cref="TrackingProgressBar"/> and a <see cref="TrackingLabel"/>.
    /// </summary>
    public partial class TrackingControl : UserControl
    {
        #region Properties
        /// <summary>
        /// The <see cref="ITask"/> to track.
        /// </summary>
        /// <remarks>
        ///   <para>Setting this property will hook up event handlers to monitor the task.</para>
        ///   <para>Remember to set it back to <see langword="null"/> or to call <see cref="IDisposable.Dispose"/> when done, to remove the event handlers again.</para>
        ///   <para>The value must not be set from a background thread.</para>
        ///   <para>The value must not be set before <see cref="Control.Handle"/> has been created.</para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if the value is set from a thread other than the UI thread or before the <see cref="Control.Handle"/> is created.</exception>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ITask Task
        {
            set
            {
                #region Sanity checks
                if (InvokeRequired) throw new InvalidOperationException("Method called from a non UI thread.");
                if (!IsHandleCreated) throw new InvalidOperationException("Method called before control handle was created.");
                #endregion

                labelOperation.Text = (value == null ? "" : value.Name);
                toolTip.SetToolTip(labelOperation, labelOperation.Text); // Show as tooltip in case text is cut off
                progressBar.Task = value;
                progressLabel.Task = value;
            }
            get { return progressBar.Task; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new tracking control.
        /// </summary>
        public TrackingControl()
        {
            InitializeComponent();
        }
        #endregion
    }
}
