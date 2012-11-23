/*
 * Copyright 2010-2012 Bastian Eicher, Simon E. Silva Lauinger
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using Common;
using Common.Collections;
using Common.Controls;
using Common.Utils;
using ZeroInstall.Commands.WinForms.CapabilityModels;
using ZeroInstall.Commands.WinForms.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Model;
using AccessPoints = ZeroInstall.DesktopIntegration.AccessPoints;
using Capabilities = ZeroInstall.Model.Capabilities;

namespace ZeroInstall.Commands.WinForms
{
    /// <summary>
    /// A dialog form used for displaying and applying desktop integration options for applications.
    /// </summary>
    public sealed partial class IntegrateAppForm : OKCancelDialog
    {
        #region Variables
        /// <summary>
        /// The integration manager used to apply selected integration options.
        /// </summary>
        private readonly IIntegrationManager _integrationManager;

        /// <summary>
        /// The application being integrated.
        /// </summary>
        private readonly AppEntry _appEntry;

        /// <summary>
        /// The feed providing additional metadata, icons, etc. for the application.
        /// </summary>
        private readonly Feed _feed;

        /// <summary>
        /// List of the <see cref="CapabilityModel"/>s handled by this form.
        /// </summary>
        private readonly List<CapabilityModel> _capabilityModels = new List<CapabilityModel>();

        /// <summary>
        /// A list of <see cref="AccessPoints.MenuEntry"/>s as displayed by the <see cref="dataGridStartMenu"/>.
        /// </summary>
        private readonly BindingList<AccessPoints.MenuEntry> _menuEntries = new BindingList<AccessPoints.MenuEntry>();

        /// <summary>
        /// A list of <see cref="AccessPoints.DesktopIcon"/>s as displayed by the <see cref="dataGridDesktop"/>.
        /// </summary>
        private readonly BindingList<AccessPoints.DesktopIcon> _desktopIcons = new BindingList<AccessPoints.DesktopIcon>();

        /// <summary>
        /// A list of <see cref="AccessPoints.AppAlias"/>es as displayed by the <see cref="dataGridAliases"/>.
        /// </summary>
        private readonly BindingList<AccessPoints.AppAlias> _aliases = new BindingList<AccessPoints.AppAlias>();

        private readonly BindingList<FileTypeModel> _fileTypesBinding = new BindingList<FileTypeModel>();
        private readonly BindingList<UrlProtocolModel> _urlProtocolsBinding = new BindingList<UrlProtocolModel>();
        private readonly BindingList<AutoPlayModel> _autoPlayBinding = new BindingList<AutoPlayModel>();
        private readonly BindingList<ContextMenuModel> _contextMenuBinding = new BindingList<ContextMenuModel>();
        private readonly BindingList<DefaultProgramModel> _defaultProgramBinding = new BindingList<DefaultProgramModel>();
        #endregion

        #region Constructor
        /// <summary>
        /// Creates an instance of the form.
        /// </summary>
        /// <param name="integrationManager">The integration manager used to apply selected integration options.</param>
        /// <param name="appEntry">The application being integrated.</param>
        /// <param name="feed">The feed providing additional metadata, icons, etc. for the application.</param>
        public IntegrateAppForm(IIntegrationManager integrationManager, AppEntry appEntry, Feed feed)
        {
            #region Sanity checks
            if (integrationManager == null) throw new ArgumentNullException("integrationManager");
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            if (feed == null) throw new ArgumentNullException("feed");
            #endregion

            InitializeComponent();

            _integrationManager = integrationManager;
            _appEntry = appEntry;
            _feed = feed;

            Text = string.Format(Resources.Integrate, appEntry.Name);
        }
        #endregion

        //--------------------//

        #region Startup
        /// <summary>
        /// Loads the <see cref="Capabilities.Capability"/>s of <see cref="_appEntry"/> into the controls of this form.
        /// </summary>
        private void IntegrateAppForm_Load(object sender, EventArgs e)
        {
            WindowsUtils.SetForegroundWindow(this);

            checkBoxAutoUpdate.Checked = _appEntry.AutoUpdate;
            checkBoxCapabilities.Visible = !_appEntry.CapabilityLists.IsEmpty;
            checkBoxCapabilities.Checked = (_appEntry.AccessPoints == null) || !_appEntry.AccessPoints.Entries.OfType<AccessPoints.CapabilityRegistration>().Any();

            SetupCommandAccessPoints();
            SetupDefaultAccessPoints();
            SwitchToSimpleView();
        }

        /// <summary>
        /// Sets up the UI elements for configuring <see cref="AccessPoints.CommandAccessPoint"/>s.
        /// </summary>
        private void SetupCommandAccessPoints()
        {
            var commands = _feed.EntryPoints.Map(entryPoint => entryPoint.Command);
            commands.UpdateOrAdd(Command.NameRun);
            var commandsArray = commands.ToArray();
            // ReSharper disable CoVariantArrayConversion
            dataGridStartMenuColumnCommand.Items.AddRange(commandsArray);
            dataGridDesktopColumnCommand.Items.AddRange(commandsArray);
            dataGridAliasesColumnCommand.Items.AddRange(commandsArray);
            // ReSharper restore CoVariantArrayConversion

            if (_appEntry.AccessPoints == null)
            { // Fill in useful default values
                foreach (var entry in SuggestMenuEntries()) _menuEntries.Add(entry);
                foreach (var desktopIcon in SuggestDesktopIcons()) _desktopIcons.Add(desktopIcon);
                foreach (var alias in SuggestAliases()) _aliases.Add(alias);
            }
            else ReadCommandAccessPoints();

            // Apply data to DataGrids in bulk for better performance
            dataGridStartMenu.DataSource = _menuEntries;
            dataGridDesktop.DataSource = _desktopIcons;
            dataGridAliases.DataSource = _aliases;
        }

        /// <summary>
        /// Determines the existing <see cref="AccessPoints.CommandAccessPoint"/>s.
        /// </summary>
        private void ReadCommandAccessPoints()
        {
            // Add clones to DataGrids so that user modifications can still be canceled
            new PerTypeDispatcher<AccessPoints.AccessPoint>(true)
            {
                (AccessPoints.MenuEntry menuEntry) => _menuEntries.Add((AccessPoints.MenuEntry)menuEntry.Clone()),
                (AccessPoints.DesktopIcon desktopIcon) => _desktopIcons.Add((AccessPoints.DesktopIcon)desktopIcon.Clone()),
                (AccessPoints.AppAlias appAlias) => _aliases.Add((AccessPoints.AppAlias)appAlias.Clone()),
            }.Dispatch(_appEntry.AccessPoints.Entries);
        }

        /// <summary>
        /// Sets up the UI elements for configuring <see cref="AccessPoints.DefaultAccessPoint"/>s.
        /// </summary>
        private void SetupDefaultAccessPoints()
        {
            foreach (var capabilityList in _appEntry.CapabilityLists)
            {
                if (!capabilityList.Architecture.IsCompatible(Architecture.CurrentSystem)) continue;

                var dispatcher = new PerTypeDispatcher<Capabilities.Capability>(true)
                {
                    // File types
                    (Capabilities.FileType fileType) =>
                    {
                        var model = new FileTypeModel(fileType, IsCapabillityUsed<AccessPoints.FileType>(fileType));
                        _fileTypesBinding.Add(model);
                        _capabilityModels.Add(model);
                    },

                    // URL protocols
                    (Capabilities.UrlProtocol urlProtocol) =>
                    {
                        var model = new UrlProtocolModel(urlProtocol, IsCapabillityUsed<AccessPoints.UrlProtocol>(urlProtocol));
                        _urlProtocolsBinding.Add(model);
                        _capabilityModels.Add(model);
                    },

                    // AutoPlay
                    (Capabilities.AutoPlay autoPlay) =>
                    {
                        var model = new AutoPlayModel(autoPlay, IsCapabillityUsed<AccessPoints.AutoPlay>(autoPlay));
                        _autoPlayBinding.Add(model);
                        _capabilityModels.Add(model);
                    },

                    // Context menu
                    (Capabilities.ContextMenu contextMenu) =>
                    {
                        var model = new ContextMenuModel(contextMenu, IsCapabillityUsed<AccessPoints.ContextMenu>(contextMenu));
                        _contextMenuBinding.Add(model);
                        _capabilityModels.Add(model);
                    }
                };

                if (_integrationManager.MachineWide)
                {
                    // Default program
                    dispatcher.Add((Capabilities.DefaultProgram defaultProgram) =>
                    {
                        var model = new DefaultProgramModel(defaultProgram, IsCapabillityUsed<AccessPoints.DefaultProgram>(defaultProgram));
                        _defaultProgramBinding.Add(model);
                        _capabilityModels.Add(model);
                    });
                }

                dispatcher.Dispatch(capabilityList.Entries);
            }

            // Apply data to DataGrids in bulk for better performance (remove empty tabs)
            if (_fileTypesBinding.Count != 0) dataGridFileTypes.DataSource = _fileTypesBinding;
            else tabControl.TabPages.Remove(tabPageFileTypes);
            if (_urlProtocolsBinding.Count != 0) dataGridUrlProtocols.DataSource = _urlProtocolsBinding;
            else tabControl.TabPages.Remove(tabPageUrlProtocols);
            if (_autoPlayBinding.Count != 0) dataGridAutoPlay.DataSource = _autoPlayBinding;
            else tabControl.TabPages.Remove(tabPageAutoPlay);
            if (_contextMenuBinding.Count != 0) dataGridContextMenu.DataSource = _contextMenuBinding;
            else tabControl.TabPages.Remove(tabPageContextMenu);
            if (_defaultProgramBinding.Count != 0 && _integrationManager.MachineWide) dataGridDefaultPrograms.DataSource = _defaultProgramBinding;
            else tabControl.TabPages.Remove(tabPageDefaultPrograms);
        }

        /// <summary>
        /// Checks whether a <see cref="Capabilities.DefaultCapability"/> is already used by the user.
        /// </summary>
        /// <typeparam name="T">Type of the <see cref="ZeroInstall.DesktopIntegration.AccessPoints.DefaultAccessPoint"/> to which <paramref name="toCheck"/> belongs to.</typeparam>
        /// <param name="toCheck">The <see cref="Capabilities.Capability"/> to check for usage.</param>
        /// <returns><see langword="true"/>, if <paramref name="toCheck"/> is already in usage.</returns>
        private bool IsCapabillityUsed<T>(Capabilities.DefaultCapability toCheck) where T : AccessPoints.DefaultAccessPoint
        {
            if (_appEntry.AccessPoints == null) return false;

            return _appEntry.AccessPoints.Entries.OfType<T>().Any(accessPoint => accessPoint.Capability == toCheck.ID);
        }
        #endregion

        #region Simple/advanced view
        private void buttonAdvanced_Click(object sender, EventArgs e)
        {
            // Toggle between simple and advanced view
            if (flowLayoutSimple.Visible) SwitchToAdvancedView();
            else SwitchToSimpleView();
        }

        /// <summary>
        /// Displays the simple configuration view. Initializes the checkboxes with data from the DataGrids.
        /// </summary>
        private void SwitchToSimpleView()
        {
            SetCommandAccessPointCheckBox(checkBoxStartMenuSimple, _menuEntries, SuggestMenuEntries);
            SetCommandAccessPointCheckBox(checkBoxDesktopSimple, _desktopIcons, SuggestDesktopIcons);
            SetCommandAccessPointCheckBox(checkBoxAliasesSimple, _aliases, SuggestAliases);
            SetDefaultAccessPointCheckBox(checkBoxFileTypesSimple, _fileTypesBinding);
            SetDefaultAccessPointCheckBox(checkBoxAutoPlaySimple, _autoPlayBinding);

            flowLayoutSimple.Visible = true;
            checkBoxAutoUpdate.Visible = checkBoxCapabilities.Visible = tabControl.Visible = false;
            buttonAdvanced.Text = Resources.AdvancedSettings;
        }

        /// <summary>
        /// Configures the visibility and check state of <see cref="AccessPoints.CommandAccessPoint"/> <see cref="CheckBox"/>.
        /// </summary>
        /// <typeparam name="T">The specific kind of <see cref="AccessPoints.CommandAccessPoint"/> to handle.</typeparam>
        /// <param name="checkBox">The <see cref="CheckBox"/> to configure.</param>
        /// <param name="current">The currently applied <see cref="AccessPoints.CommandAccessPoint"/>.</param>
        /// <param name="getSuggestions">Retrieves a list of default <see cref="AccessPoints.CommandAccessPoint"/> suggested by the system.</param>
        private static void SetCommandAccessPointCheckBox<T>(CheckBox checkBox, IEnumerable<T> current, Func<IEnumerable<T>> getSuggestions) where T : AccessPoints.CommandAccessPoint
        {
            checkBox.Checked = current.Any();
            checkBox.Visible = getSuggestions().Any();
        }

        /// <summary>
        /// Configures the visibility and check state of <see cref="AccessPoints.DefaultAccessPoint"/> <see cref="CheckBox"/>.
        /// </summary>
        /// <typeparam name="T">The specific kind of <see cref="AccessPoints.DefaultAccessPoint"/> to handle.</typeparam>
        /// <param name="checkBox">The <see cref="CheckBox"/> to configure.</param>
        /// <param name="model">A model represeting the underlying <see cref="Capabilities.DefaultCapability"/>s and their selection statet.</param>
        private static void SetDefaultAccessPointCheckBox<T>(CheckBox checkBox, BindingList<T> model) where T : CapabilityModel
        {
            checkBox.Checked = model.Any(element => element.Use);
            checkBox.Visible = model.Any();
        }

        /// <summary>
        /// Displays the advanced configuration view. Applies changes made in the basic view back to the DataGrids.
        /// </summary>
        private void SwitchToAdvancedView()
        {
            ApplyCommandAccessPointCheckBox(checkBoxStartMenuSimple, _menuEntries, SuggestMenuEntries);
            ApplyCommandAccessPointCheckBox(checkBoxDesktopSimple, _desktopIcons, SuggestDesktopIcons);
            ApplyCommandAccessPointCheckBox(checkBoxAliasesSimple, _aliases, SuggestAliases);
            ApplyDefaultAccessPointCheckBox(checkBoxFileTypesSimple, _fileTypesBinding);
            ApplyDefaultAccessPointCheckBox(checkBoxAutoPlaySimple, _autoPlayBinding);

            checkBoxAutoUpdate.Visible = checkBoxCapabilities.Visible = tabControl.Visible = true;
            flowLayoutSimple.Visible = false;
            buttonAdvanced.Text = Resources.Basic;
        }

        /// <summary>
        /// Applies the state of a <see cref="AccessPoints.CommandAccessPoint"/> <see cref="CheckBox"/>.
        /// </summary>
        /// <typeparam name="T">The specific kind of <see cref="AccessPoints.CommandAccessPoint"/> to handle.</typeparam>
        /// <param name="checkBox">The <see cref="CheckBox"/> to read.</param>
        /// <param name="current">The currently applied <see cref="AccessPoints.CommandAccessPoint"/>.</param>
        /// <param name="getSuggestions">Retrieves a list of default <see cref="AccessPoints.CommandAccessPoint"/> suggested by the system.</param>
        private static void ApplyCommandAccessPointCheckBox<T>(CheckBox checkBox, ICollection<T> current, Func<IEnumerable<T>> getSuggestions) where T : AccessPoints.CommandAccessPoint
        {
            if (checkBox.Checked)
            {
                if (current.Count == 0) foreach (var entry in getSuggestions()) current.Add(entry);
            }
            else current.Clear();
        }

        /// <summary>
        /// Applies the state of a <see cref="AccessPoints.DefaultAccessPoint"/> <see cref="CheckBox"/>.
        /// </summary>
        /// <typeparam name="T">The specific kind of <see cref="AccessPoints.DefaultAccessPoint"/> to handle.</typeparam>
        /// <param name="checkBox">The <see cref="CheckBox"/> to read.</param>
        /// <param name="model">A model represeting the underlying <see cref="Capabilities.DefaultCapability"/>s and their selection statet.</param>
        private static void ApplyDefaultAccessPointCheckBox<T>(CheckBox checkBox, BindingList<T> model) where T : CapabilityModel
        {
            if (checkBox.Checked)
            {
                if (!model.Any(element => element.Use)) CapabilityModelSetAll(model, true);
            }
            else CapabilityModelSetAll(model, false);
        }
        #endregion

        #region Suggestions
        /// <summary>
        /// Returns a list of suitable default <see cref="AccessPoints.MenuEntry"/>s.
        /// </summary>
        private IEnumerable<AccessPoints.MenuEntry> SuggestMenuEntries()
        {
            string category = _feed.Categories.FirstOrDefault();
            if (_feed.EntryPoints.IsEmpty)
            { // Only one entry point
                return new[] {new AccessPoints.MenuEntry {Name = _appEntry.Name, Category = category, Command = Command.NameRun}};
            }
            else
            { // Multiple entry points
                return
                    from entryPoint in _feed.EntryPoints
                    where !string.IsNullOrEmpty(entryPoint.Command)
                    select new AccessPoints.MenuEntry
                    {
                        // Try to get a localized name for the command
                        Name = entryPoint.Names.GetBestLanguage(CultureInfo.CurrentUICulture) ?? // If that fails...
                            ((entryPoint.Command == Command.NameRun)
                                // ... use the application's name
                                ? _appEntry.Name
                                // ... or the application's name and the command
                                : _appEntry.Name + " " + entryPoint.Command),
                        // Group all entry points in a single folder
                        Category = string.IsNullOrEmpty(category) ? _appEntry.Name : category + Path.DirectorySeparatorChar + _appEntry.Name,
                        Command = entryPoint.Command
                    };
            }
        }

        /// <summary>
        /// Returns a list of suitable default <see cref="AccessPoints.DesktopIcon"/>s.
        /// </summary>
        private IEnumerable<AccessPoints.DesktopIcon> SuggestDesktopIcons()
        {
            return new[] {new AccessPoints.DesktopIcon {Name = _appEntry.Name, Command = Command.NameRun}};
        }

        /// <summary>
        /// Returns a list of suitable default <see cref="AccessPoints.AppAlias"/>s.
        /// </summary>
        private IEnumerable<AccessPoints.AppAlias> SuggestAliases()
        {
            if (_feed.EntryPoints.IsEmpty)
            { // Only one entry point
                if (_feed.NeedsTerminal)
                {
                    // Try to guess reasonable alias name of command-line applications
                    return new[] {new AccessPoints.AppAlias {Name = _appEntry.Name.Replace(' ', '-').ToLower(), Command = Command.NameRun}};
                }
                else return new AccessPoints.AppAlias[0];
            }
            else
            { // Multiple entry points
                return
                    from entryPoint in _feed.EntryPoints
                    where !string.IsNullOrEmpty(entryPoint.Command) && (entryPoint.NeedsTerminal || _feed.NeedsTerminal)
                    select new AccessPoints.AppAlias
                    {
                        Name = entryPoint.BinaryName ?? entryPoint.Command,
                        Command = entryPoint.Command
                    };
            }
        }
        #endregion

        #region Select all checkboxes
        private void checkBoxFileTypesAll_CheckedChanged(object sender, EventArgs e)
        {
            CapabilityModelSetAll(_fileTypesBinding, checkBoxFileTypesAll.Checked);
        }

        private void checkBoxUrlProtocolsAll_CheckedChanged(object sender, EventArgs e)
        {
            CapabilityModelSetAll(_urlProtocolsBinding, checkBoxUrlProtocolsAll.Checked);
        }

        private void checkBoxAutoPlayAll_CheckedChanged(object sender, EventArgs e)
        {
            CapabilityModelSetAll(_autoPlayBinding, checkBoxAutoPlayAll.Checked);
        }

        private void checkBoxContextMenuAll_CheckedChanged(object sender, EventArgs e)
        {
            CapabilityModelSetAll(_autoPlayBinding, checkBoxAutoPlayAll.Checked);
        }

        private void checkBoxDefaultProgramsAll_CheckedChanged(object sender, EventArgs e)
        {
            CapabilityModelSetAll(_defaultProgramBinding, checkBoxDefaultProgramsAll.Checked);
        }

        /// <summary>
        /// Sets all <see cref="CapabilityModel.Use"/> values within a list/model to a specific value.
        /// </summary>
        /// <typeparam name="T">The specific kind of <see cref="AccessPoints.DefaultAccessPoint"/> to handle.</typeparam>
        /// <param name="model">A model represeting the underlying <see cref="Capabilities.DefaultCapability"/>s and their selection statet.</param>
        /// <param name="value">The value to set.</param>
        private static void CapabilityModelSetAll<T>(BindingList<T> model, bool value) where T : CapabilityModel
        {
            foreach (var element in model.Where(element => !element.Capability.ExplicitOnly))
                element.Use = value;
            model.ResetBindings();
        }
        #endregion

        #region Misc events
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

        #region Apply
        /// <summary>
        /// Integrates all <see cref="Capabilities.Capability"/>s chosen by the user.
        /// </summary>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            // Hide so that the underlying progress tracker is visible
            Visible = false;

            var toAdd = new List<AccessPoints.AccessPoint>();
            var toRemove = new List<AccessPoints.AccessPoint>();

            _appEntry.AutoUpdate = checkBoxAutoUpdate.Checked;
            (checkBoxCapabilities.Checked ? toAdd : toRemove).Add(new AccessPoints.CapabilityRegistration());

            SwitchToAdvancedView(); // Must do this to apply changes made in simple view
            HandleCommandAccessPointChanges(toAdd, toRemove);
            HandleDefaultAccessPointChanges(toAdd, toRemove);

            try
            {
                if (toRemove.Any()) _integrationManager.RemoveAccessPoints(_appEntry, toRemove);
                if (toAdd.Any()) _integrationManager.AddAccessPoints(_appEntry, _feed, toAdd);
            }
            #region Error handling
            catch (OperationCanceledException)
            {
                Visible = true; // Allow user to fix input
                return;
            }
            catch (InvalidDataException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                Visible = true; // Allow user to fix input
                return;
            }
            catch (WebException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                Visible = true; // Allow user to fix input
                return;
            }
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                Visible = true; // Allow user to fix input
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                Visible = true; // Allow user to fix input
                return;
            }
            catch (InvalidOperationException ex)
            {
                // ToDo: More comprehensive conflict handling
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                Visible = true; // Allow user to fix input
                return;
            }
            #endregion

            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// Determines changes to <see cref="AccessPoints.CommandAccessPoint"/>s specified by the user.
        /// </summary>
        /// <param name="toAdd">List to add <see cref="AccessPoints.AccessPoint"/>s to be added to.</param>
        /// <param name="toRemove">List to add <see cref="AccessPoints.AccessPoint"/>s to be removed to.</param>
        private void HandleCommandAccessPointChanges(ICollection<AccessPoints.AccessPoint> toAdd, ICollection<AccessPoints.AccessPoint> toRemove)
        {
            // Build lists with current integration state
            var currentMenuEntries = new List<AccessPoints.MenuEntry>();
            var currenDesktopIcons = new List<AccessPoints.DesktopIcon>();
            var currenAliases = new List<AccessPoints.AppAlias>();
            if (_appEntry.AccessPoints != null)
            {
                new PerTypeDispatcher<AccessPoints.AccessPoint>(true)
                {
                    (AccessPoints.MenuEntry menuEntry) => currentMenuEntries.Add(menuEntry),
                    (AccessPoints.DesktopIcon desktopIcon) => currenDesktopIcons.Add(desktopIcon),
                    (AccessPoints.AppAlias alias) => currenAliases.Add(alias)
                }.Dispatch(_appEntry.AccessPoints.Entries);
            }

            // Determine differences between current and desired state
            EnumerableUtils.Merge(_menuEntries, currentMenuEntries, toAdd.Add, toRemove.Add);
            EnumerableUtils.Merge(_desktopIcons, currenDesktopIcons, toAdd.Add, toRemove.Add);
            EnumerableUtils.Merge(_aliases, currenAliases, toAdd.Add, toRemove.Add);
        }

        /// <summary>
        /// Determines changes to <see cref="AccessPoints.DefaultAccessPoint"/>s specified by the user.
        /// </summary>
        /// <param name="toAdd">List to add <see cref="AccessPoints.AccessPoint"/>s to be added to.</param>
        /// <param name="toRemove">List to add <see cref="AccessPoints.AccessPoint"/>s to be removed to.</param>
        private void HandleDefaultAccessPointChanges(ICollection<AccessPoints.AccessPoint> toAdd, ICollection<AccessPoints.AccessPoint> toRemove)
        {
            foreach (var capabilityModel in _capabilityModels)
            {
                if (capabilityModel.Changed)
                {
                    var accessPoint = AccessPoints.DefaultAccessPoint.FromCapability(capabilityModel.Capability);
                    if (capabilityModel.Use) toAdd.Add(accessPoint);
                    else toRemove.Add(accessPoint);
                }
            }
        }
        #endregion
    }
}
