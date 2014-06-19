/*
 * Copyright 2010-2014 Bastian Eicher, Simon E. Silva Lauinger
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Controls;
using NanoByte.Common.Utils;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.DesktopIntegration.ViewModel;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Commands.WinForms
{
    /// <summary>
    /// A dialog form used for displaying and applying desktop integration options for applications.
    /// </summary>
    public sealed partial class IntegrateAppForm : OKCancelDialog
    {
        #region Dependencies
        /// <summary>
        /// A View-Model for modifying the current desktop integration state.
        /// </summary>
        private readonly IntegrationState _state;

        /// <summary>
        /// Creates an instance of the form.
        /// </summary>
        /// <param name="state">A View-Model for modifying the current desktop integration state.</param>
        public IntegrateAppForm(IntegrationState state)
        {
            #region Sanity checks
            if (state == null) throw new ArgumentNullException("state");
            #endregion

            InitializeComponent();

            _state = state;
        }
        #endregion

        #region Startup
        /// <summary>
        /// Loads the <see cref="Store.Model.Capabilities.Capability"/>s into the controls of this form.
        /// </summary>
        private void IntegrateAppForm_Load(object sender, EventArgs e)
        {
            this.SetForegroundWindow();

            Text = string.Format(Resources.Integrate, _state.AppEntry.Name);

            checkBoxAutoUpdate.Checked = _state.AppEntry.AutoUpdate;
            checkBoxCapabilities.Visible = (_state.AppEntry.CapabilityLists.Count != 0);
            checkBoxCapabilities.Checked = _state.CapabilitiyRegistration;

            SetupCommandAccessPoints();
            SetupDefaultAccessPoints();
            _switchToBasicMode();
        }
        #endregion

        #region Event handlers
        /// <summary>
        /// Integrates all <see cref="Store.Model.Capabilities.Capability"/>s chosen by the user.
        /// </summary>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (buttonAdvancedMode.Visible) _switchToAdvancedMode(); // Apply changes made in "Simple View"
            _state.CapabilitiyRegistration = checkBoxCapabilities.Checked;
            _state.AppEntry.AutoUpdate = checkBoxAutoUpdate.Checked;
        }

        private void buttonHelpCommandAccessPoint_Click(object sender, EventArgs e)
        {
            Msg.Inform(this, Resources.DataGridCommandAccessPointHelp, MsgSeverity.Info);
        }

        private void buttonHelpDefaultAccessPoint_Click(object sender, EventArgs e)
        {
            Msg.Inform(this, Resources.DataGridDefaultAccessPointHelp, MsgSeverity.Info);
        }

        private void accessPointDataGrid_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            labelLastDataError.Visible = true;
            labelLastDataError.Text = e.Exception.Message;
        }
        #endregion

        //--------------------//

        #region Commmand access points
        /// <summary>
        /// Sets up the UI elements for configuring <see cref="DesktopIntegration.AccessPoints.CommandAccessPoint"/>s.
        /// </summary>
        /// <remarks>Users create <see cref="DesktopIntegration.AccessPoints.CommandAccessPoint"/>s themselves based on <see cref="EntryPoint"/>s.</remarks>
        private void SetupCommandAccessPoints()
        {
            SetupCommandAccessPoint(checkBoxStartMenuSimple, labelStartMenuSimple, _state.MenuEntries, () => Suggest.MenuEntries(_state.Feed));
            SetupCommandAccessPoint(checkBoxDesktopSimple, labelDesktopSimple, _state.DesktopIcons, () => Suggest.DesktopIcons(_state.Feed));
            SetupCommandAccessPoint(checkBoxAliasesSimple, labelAliasesSimple, _state.Aliases, () => Suggest.Aliases(_state.Feed));

            SetupCommandComboBoxes();
            _state.LoadCommandAccessPoints();
            ShowCommandAccessPoints();
        }

        /// <summary>
        /// Fills the <see cref="DataGridViewComboBoxColumn"/>s with data from <see cref="Feed.EntryPoints"/> .
        /// </summary>
        private void SetupCommandComboBoxes()
        {
            var commands = _state.Feed.EntryPoints
                .Select(entryPoint => entryPoint.Command)
                .Append(Command.NameRun).Distinct()
                .Cast<object>().ToArray();

            dataGridStartMenuColumnCommand.Items.AddRange(commands);
            dataGridDesktopColumnCommand.Items.AddRange(commands);
            dataGridAliasesColumnCommand.Items.AddRange(commands);
        }

        /// <summary>
        /// Apply data to DataGrids in bulk for better performance
        /// </summary>
        private void ShowCommandAccessPoints()
        {
            dataGridStartMenu.DataSource = _state.MenuEntries;
            dataGridDesktop.DataSource = _state.DesktopIcons;
            dataGridAliases.DataSource = _state.Aliases;
        }

        /// <summary>
        /// Hooks up event handlers for <see cref="DesktopIntegration.AccessPoints.CommandAccessPoint"/>s.
        /// </summary>
        /// <typeparam name="T">The specific kind of <see cref="DesktopIntegration.AccessPoints.CommandAccessPoint"/> to handle.</typeparam>
        /// <param name="checkBoxSimple">The simple mode checkbox for this type of <see cref="DesktopIntegration.AccessPoints.CommandAccessPoint"/>.</param>
        /// <param name="labelSimple">A simple mode description for this type of <see cref="DesktopIntegration.AccessPoints.CommandAccessPoint"/>.</param>
        /// <param name="accessPoints">A list of applied <see cref="DesktopIntegration.AccessPoints.CommandAccessPoint"/>.</param>
        /// <param name="getSuggestions">Retrieves a list of default <see cref="DesktopIntegration.AccessPoints.CommandAccessPoint"/> suggested by the system.</param>
        private void SetupCommandAccessPoint<T>(CheckBox checkBoxSimple, Label labelSimple, ICollection<T> accessPoints, Func<IEnumerable<T>> getSuggestions)
            where T : DesktopIntegration.AccessPoints.CommandAccessPoint
        {
            _switchToBasicMode += () => SetCommandAccessPointCheckBox(checkBoxSimple, labelSimple, accessPoints, getSuggestions);
            _switchToAdvancedMode += () => SetCommandAccessPointCheckBox(checkBoxSimple, accessPoints, getSuggestions);
        }
        #endregion

        #region Default access points
        /// <summary>
        /// Sets up the UI elements for configuring <see cref="DesktopIntegration.AccessPoints.DefaultAccessPoint"/>s.
        /// </summary>
        /// <remarks>Users enable or disable predefined <see cref="DesktopIntegration.AccessPoints.DefaultAccessPoint"/>s based on <see cref="Store.Model.Capabilities.DefaultCapability"/>s.</remarks>
        private void SetupDefaultAccessPoints()
        {
            SetupDefaultAccessPoint(checkBoxFileTypesSimple, labelFileTypesSimple, checkBoxFileTypesAll, _state.FileTypes);
            SetupDefaultAccessPoint(checkBoxUrlProtocolsSimple, labelUrlProtocolsSimple, checkBoxUrlProtocolsAll, _state.UrlProtocols);
            SetupDefaultAccessPoint(checkBoxAutoPlaySimple, labelAutoPlaySimple, checkBoxAutoPlayAll, _state.AutoPlay);
            SetupDefaultAccessPoint(checkBoxContextMenuSimple, labelContextMenuSimple, checkBoxContextMenuAll, _state.ContextMenu);
            SetupDefaultAccessPoint(checkBoxDefaultProgramsSimple, labelDefaultProgramsSimple, checkBoxDefaultProgramsAll, _state.DefaultProgram);

            // File type associations cannot be set programmatically on Windows 8, so hide the option
            _switchToBasicMode += () => { if (WindowsUtils.IsWindows8) labelFileTypesSimple.Visible = checkBoxFileTypesSimple.Visible = false; };

            _state.LoadDefaultAccessPoints();
            SetDefaultAccessPoints();
        }

        /// <summary>
        /// Hooks up event handlers for <see cref="DesktopIntegration.AccessPoints.DefaultAccessPoint"/>s.
        /// </summary>
        /// <typeparam name="T">The specific kind of <see cref="DesktopIntegration.AccessPoints.DefaultAccessPoint"/> to handle.</typeparam>
        /// <param name="checkBoxSimple">The simple mode checkbox for this type of <see cref="DesktopIntegration.AccessPoints.CommandAccessPoint"/>.</param>
        /// <param name="labelSimple">A simple mode description for this type of <see cref="DesktopIntegration.AccessPoints.CommandAccessPoint"/>.</param>
        /// <param name="checkBoxSelectAll">The "select all" checkbox for this type of <see cref="DesktopIntegration.AccessPoints.CommandAccessPoint"/>.</param>
        /// <param name="model">A model represeting the underlying <see cref="Store.Model.Capabilities.DefaultCapability"/>s and their selection states.</param>
        private void SetupDefaultAccessPoint<T>(CheckBox checkBoxSimple, Label labelSimple, CheckBox checkBoxSelectAll, BindingList<T> model)
            where T : CapabilityModel
        {
            bool suppressEvent = false;
            checkBoxSelectAll.CheckedChanged += delegate { if (!suppressEvent) model.SetAllUse(checkBoxSelectAll.Checked); };

            _switchToBasicMode += () => SetDefaultAccessPointCheckBox(checkBoxSimple, labelSimple, model);
            _switchToAdvancedMode += () =>
            {
                SetDefaultAccessPointCheckBox(checkBoxSimple, model);

                suppressEvent = true;
                checkBoxSelectAll.Checked = model.All(element => element.Use);
                suppressEvent = false;
            };
        }

        /// <summary>
        /// Set data in DataGrids in bulk for better performance (remove empty tabs).
        /// </summary>
        private void SetDefaultAccessPoints()
        {
            if (_state.FileTypes.Any()) dataGridFileTypes.DataSource = _state.FileTypes;
            else tabControl.TabPages.Remove(tabPageFileTypes);
            if (_state.UrlProtocols.Any()) dataGridUrlProtocols.DataSource = _state.UrlProtocols;
            else tabControl.TabPages.Remove(tabPageUrlProtocols);
            if (_state.AutoPlay.Any()) dataGridAutoPlay.DataSource = _state.AutoPlay;
            else tabControl.TabPages.Remove(tabPageAutoPlay);
            if (_state.ContextMenu.Any()) dataGridContextMenu.DataSource = _state.ContextMenu;
            else tabControl.TabPages.Remove(tabPageContextMenu);
            if (_state.DefaultProgram.Any()) dataGridDefaultPrograms.DataSource = _state.DefaultProgram;
            else tabControl.TabPages.Remove(tabPageDefaultPrograms);
        }
        #endregion

        #region Basic mode
        /// <summary>
        /// Displays the basic configuration view. Transfers data from models to checkboxes.
        /// </summary>
        /// <remarks>Must be called after form setup.</remarks>
        private Action _switchToBasicMode;

        private void buttonBasicMode_Click(object sender, EventArgs e)
        {
            _switchToBasicMode();

            buttonAdvancedMode.Visible = panelBasic.Visible = true;
            buttonBasicMode.Visible = checkBoxAutoUpdate.Visible = checkBoxCapabilities.Visible = tabControl.Visible = false;
        }

        /// <summary>
        /// Configures the visibility and check state of a <see cref="CheckBox"/> for simple <see cref="DesktopIntegration.AccessPoints.CommandAccessPoint"/> configuration.
        /// </summary>
        /// <typeparam name="T">The specific kind of <see cref="DesktopIntegration.AccessPoints.CommandAccessPoint"/> to handle.</typeparam>
        /// <param name="checkBox">The <see cref="CheckBox"/> to configure.</param>
        /// <param name="label">A description for the <paramref name="checkBox"/>.</param>
        /// <param name="accessPoints">The currently applied <see cref="DesktopIntegration.AccessPoints.CommandAccessPoint"/>.</param>
        /// <param name="getSuggestions">Retrieves a list of default <see cref="DesktopIntegration.AccessPoints.CommandAccessPoint"/> suggested by the system.</param>
        private static void SetCommandAccessPointCheckBox<T>(CheckBox checkBox, Label label, IEnumerable<T> accessPoints, Func<IEnumerable<T>> getSuggestions)
            where T : DesktopIntegration.AccessPoints.CommandAccessPoint
        {
            checkBox.Checked = accessPoints.Any();
            label.Visible = checkBox.Visible = getSuggestions().Any();
        }

        /// <summary>
        /// Configures the visibility and check state of a <see cref="CheckBox"/> for simple <see cref="DesktopIntegration.AccessPoints.DefaultAccessPoint"/> configuration.
        /// </summary>
        /// <typeparam name="T">The specific kind of <see cref="DesktopIntegration.AccessPoints.DefaultAccessPoint"/> to handle.</typeparam>
        /// <param name="checkBox">The <see cref="CheckBox"/> to configure.</param>
        /// <param name="label">A description for the <paramref name="checkBox"/>.</param>
        /// <param name="model">A model represeting the underlying <see cref="Store.Model.Capabilities.DefaultCapability"/>s and their selection states.</param>
        private static void SetDefaultAccessPointCheckBox<T>(CheckBox checkBox, Label label, BindingList<T> model)
            where T : CapabilityModel
        {
            checkBox.Checked = model.Any(element => element.Use);
            label.Visible = checkBox.Visible = model.Any();
        }
        #endregion

        #region Advanced mode
        /// <summary>
        /// Displays the advabced configuration view. Transfers data from checkboxes to models.
        /// </summary>
        /// <remarks>Must be called before form close.</remarks>
        private Action _switchToAdvancedMode;

        private void buttonAdvancedMode_Click(object sender, EventArgs e)
        {
            _switchToAdvancedMode();

            buttonBasicMode.Visible = checkBoxAutoUpdate.Visible = checkBoxCapabilities.Visible = tabControl.Visible = true;
            buttonAdvancedMode.Visible = panelBasic.Visible = false;
        }

        /// <summary>
        /// Sets the state of a <see cref="CheckBox"/> for simple <see cref="DesktopIntegration.AccessPoints.CommandAccessPoint"/> configuration.
        /// </summary>
        /// <typeparam name="T">The specific kind of <see cref="DesktopIntegration.AccessPoints.CommandAccessPoint"/> to handle.</typeparam>
        /// <param name="checkBox">The <see cref="CheckBox"/> to read.</param>
        /// <param name="current">The currently applied <see cref="DesktopIntegration.AccessPoints.CommandAccessPoint"/>.</param>
        /// <param name="getSuggestions">Retrieves a list of default <see cref="DesktopIntegration.AccessPoints.CommandAccessPoint"/> suggested by the system.</param>
        private static void SetCommandAccessPointCheckBox<T>(CheckBox checkBox, ICollection<T> current, Func<IEnumerable<T>> getSuggestions)
            where T : DesktopIntegration.AccessPoints.CommandAccessPoint
        {
            if (!checkBox.Visible) return;

            if (checkBox.Checked)
            {
                if (current.Count == 0)
                {
                    foreach (var entry in getSuggestions())
                        current.Add(entry);
                }
            }
            else current.Clear();
        }

        /// <summary>
        /// Applies the state of a <see cref="CheckBox"/> for simple <see cref="DesktopIntegration.AccessPoints.DefaultAccessPoint"/> configuration.
        /// </summary>
        /// <typeparam name="T">The specific kind of <see cref="DesktopIntegration.AccessPoints.DefaultAccessPoint"/> to handle.</typeparam>
        /// <param name="checkBox">The <see cref="CheckBox"/> to read.</param>
        /// <param name="model">A model represeting the underlying <see cref="Store.Model.Capabilities.DefaultCapability"/>s and their selection states.</param>
        private static void SetDefaultAccessPointCheckBox<T>(CheckBox checkBox, BindingList<T> model)
            where T : CapabilityModel
        {
            if (!checkBox.Visible) return;
            if (checkBox.Checked)
            {
                if (!model.Any(element => element.Use)) model.SetAllUse(true);
            }
            else model.SetAllUse(false);
        }
        #endregion
    }
}
