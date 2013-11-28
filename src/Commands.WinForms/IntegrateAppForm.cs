/*
 * Copyright 2010-2013 Bastian Eicher, Simon E. Silva Lauinger
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
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using Common;
using Common.Collections;
using Common.Controls;
using Common.Utils;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Model;
using ZeroInstall.Model.Capabilities.FontendModels;

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
        /// A list of <see cref="ZeroInstall.DesktopIntegration.AccessPoints.MenuEntry"/>s as displayed by the <see cref="dataGridStartMenu"/>.
        /// </summary>
        private readonly BindingList<DesktopIntegration.AccessPoints.MenuEntry> _menuEntries = new BindingList<DesktopIntegration.AccessPoints.MenuEntry>();

        /// <summary>
        /// A list of <see cref="ZeroInstall.DesktopIntegration.AccessPoints.DesktopIcon"/>s as displayed by the <see cref="dataGridDesktop"/>.
        /// </summary>
        private readonly BindingList<DesktopIntegration.AccessPoints.DesktopIcon> _desktopIcons = new BindingList<DesktopIntegration.AccessPoints.DesktopIcon>();

        /// <summary>
        /// A list of <see cref="ZeroInstall.DesktopIntegration.AccessPoints.AppAlias"/>es as displayed by the <see cref="dataGridAliases"/>.
        /// </summary>
        private readonly BindingList<DesktopIntegration.AccessPoints.AppAlias> _aliases = new BindingList<DesktopIntegration.AccessPoints.AppAlias>();

        private readonly BindingList<FileTypeModel> _fileTypesBinding = new BindingList<FileTypeModel>();
        private readonly BindingList<UrlProtocolModel> _urlProtocolsBinding = new BindingList<UrlProtocolModel>();
        private readonly BindingList<AutoPlayModel> _autoPlayBinding = new BindingList<AutoPlayModel>();
        private readonly BindingList<ContextMenuModel> _contextMenuBinding = new BindingList<ContextMenuModel>();
        private readonly BindingList<DefaultProgramModel> _defaultProgramBinding = new BindingList<DefaultProgramModel>();
        #endregion

        #region Startup
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

        /// <summary>
        /// Loads the <see cref="ZeroInstall.Model.Capabilities.Capability"/>s of <see cref="_appEntry"/> into the controls of this form.
        /// </summary>
        private void IntegrateAppForm_Load(object sender, EventArgs e)
        {
            WindowsUtils.SetForegroundWindow(this);

            checkBoxAutoUpdate.Checked = _appEntry.AutoUpdate;
            checkBoxCapabilities.Visible = !_appEntry.CapabilityLists.IsEmpty;
            checkBoxCapabilities.Checked = (_appEntry.AccessPoints == null) || _appEntry.AccessPoints.Entries.OfType<DesktopIntegration.AccessPoints.CapabilityRegistration>().Any();

            SetupCommandAccessPoints();
            SetupDefaultAccessPoints();
            _switchToBasicMode();
        }
        #endregion

        //--------------------//

        #region Commmand access points
        /// <summary>
        /// Sets up the UI elements for configuring <see cref="ZeroInstall.DesktopIntegration.AccessPoints.CommandAccessPoint"/>s.
        /// </summary>
        /// <remarks>Users create <see cref="ZeroInstall.DesktopIntegration.AccessPoints.CommandAccessPoint"/>s themselves based on <see cref="EntryPoint"/>s.</remarks>
        private void SetupCommandAccessPoints()
        {
            SetupCommandAccessPoint(checkBoxStartMenuSimple, labelStartMenuSimple, _menuEntries, () => _feed.SuggestMenuEntries());
            SetupCommandAccessPoint(checkBoxDesktopSimple, labelDesktopSimple, _desktopIcons, () => _feed.SuggestDesktopIcons());
            SetupCommandAccessPoint(checkBoxAliasesSimple, labelAliasesSimple, _aliases, () => _feed.SuggestAliases());

            SetupCommandComboBoxes();
            LoadCommandAccessPoints();
        }

        /// <summary>
        /// Fills the <see cref="DataGridViewComboBoxColumn"/>s with data from <see cref="Feed.EntryPoints"/> .
        /// </summary>
        private void SetupCommandComboBoxes()
        {
            var commands = _feed.EntryPoints.Map(entryPoint => entryPoint.Command);
            commands.UpdateOrAdd(Command.NameRun);
            var commandsArray = commands.ToArray();

            // ReSharper disable CoVariantArrayConversion
            dataGridStartMenuColumnCommand.Items.AddRange(commandsArray);
            dataGridDesktopColumnCommand.Items.AddRange(commandsArray);
            dataGridAliasesColumnCommand.Items.AddRange(commandsArray);
            // ReSharper restore CoVariantArrayConversion
        }

        /// <summary>
        /// Reads the <see cref="ZeroInstall.DesktopIntegration.AccessPoints.CommandAccessPoint"/>s from <see cref="AppEntry.AccessPoints"/> or uses suggestion methods to fill in defaults.
        /// </summary>
        private void LoadCommandAccessPoints()
        {
            if (_appEntry.AccessPoints == null)
            { // Fill in default values for first integration
                foreach (var entry in _feed.SuggestMenuEntries()) _menuEntries.Add(entry);
                foreach (var desktopIcon in _feed.SuggestDesktopIcons()) _desktopIcons.Add(desktopIcon);
                foreach (var alias in _feed.SuggestAliases()) _aliases.Add(alias);
            }
            else
            { // Distribute existing CommandAccessPoints among type-specific binding lists
                new PerTypeDispatcher<DesktopIntegration.AccessPoints.AccessPoint>(true)
                {
                    // Add clones to DataGrids so that user modifications can still be canceled
                    (DesktopIntegration.AccessPoints.MenuEntry menuEntry) => _menuEntries.Add((DesktopIntegration.AccessPoints.MenuEntry)menuEntry.Clone()),
                    (DesktopIntegration.AccessPoints.DesktopIcon desktopIcon) => _desktopIcons.Add((DesktopIntegration.AccessPoints.DesktopIcon)desktopIcon.Clone()),
                    (DesktopIntegration.AccessPoints.AppAlias appAlias) => _aliases.Add((DesktopIntegration.AccessPoints.AppAlias)appAlias.Clone()),
                }.Dispatch(_appEntry.AccessPoints.Entries);
            }

            // Apply data to DataGrids in bulk for better performance
            dataGridStartMenu.DataSource = _menuEntries;
            dataGridDesktop.DataSource = _desktopIcons;
            dataGridAliases.DataSource = _aliases;
        }

        /// <summary>
        /// Hooks up event handlers for <see cref="ZeroInstall.DesktopIntegration.AccessPoints.CommandAccessPoint"/>s.
        /// </summary>
        /// <typeparam name="T">The specific kind of <see cref="ZeroInstall.DesktopIntegration.AccessPoints.CommandAccessPoint"/> to handle.</typeparam>
        /// <param name="checkBoxSimple">The simple mode checkbox for this type of <see cref="ZeroInstall.DesktopIntegration.AccessPoints.CommandAccessPoint"/>.</param>
        /// <param name="labelSimple">A simple mode description for this type of <see cref="ZeroInstall.DesktopIntegration.AccessPoints.CommandAccessPoint"/>.</param>
        /// <param name="accessPoints">A list of applied <see cref="ZeroInstall.DesktopIntegration.AccessPoints.CommandAccessPoint"/>.</param>
        /// <param name="getSuggestions">Retrieves a list of default <see cref="ZeroInstall.DesktopIntegration.AccessPoints.CommandAccessPoint"/> suggested by the system.</param>
        private void SetupCommandAccessPoint<T>(CheckBox checkBoxSimple, Label labelSimple, ICollection<T> accessPoints, Func<IEnumerable<T>> getSuggestions)
            where T : DesktopIntegration.AccessPoints.CommandAccessPoint
        {
            _switchToBasicMode += () => SetCommandAccessPointCheckBox(checkBoxSimple, labelSimple, accessPoints, getSuggestions);
            _switchToAdvancedMode += () => ApplyCommandAccessPointCheckBox(checkBoxSimple, accessPoints, getSuggestions);
        }
        #endregion

        #region Default access points
        /// <summary>
        /// Sets up the UI elements for configuring <see cref="ZeroInstall.DesktopIntegration.AccessPoints.DefaultAccessPoint"/>s.
        /// </summary>
        /// <remarks>Users enable or disable predefined <see cref="ZeroInstall.DesktopIntegration.AccessPoints.DefaultAccessPoint"/>s based on <see cref="ZeroInstall.Model.Capabilities.DefaultCapability"/>s.</remarks>
        private void SetupDefaultAccessPoints()
        {
            SetupDefaultAccessPoint(checkBoxFileTypesSimple, labelFileTypesSimple, checkBoxFileTypesAll, _fileTypesBinding);
            SetupDefaultAccessPoint(checkBoxUrlProtocolsSimple, labelUrlProtocolsSimple, checkBoxUrlProtocolsAll, _urlProtocolsBinding);
            SetupDefaultAccessPoint(checkBoxAutoPlaySimple, labelAutoPlaySimple, checkBoxAutoPlayAll, _autoPlayBinding);
            SetupDefaultAccessPoint(checkBoxContextMenuSimple, labelContextMenuSimple, checkBoxContextMenuAll, _contextMenuBinding);
            SetupDefaultAccessPoint(checkBoxDefaultProgramsSimple, labelDefaultProgramsSimple, checkBoxDefaultProgramsAll, _defaultProgramBinding);

            // File type associations cannot be set programmatically on Windows 8, so hide the option
            _switchToBasicMode += () => { if (WindowsUtils.IsWindows8) labelFileTypesSimple.Visible = checkBoxFileTypesSimple.Visible = false; };

            LoadDefaultAccessPoints();
        }

        /// <summary>
        /// Hooks up event handlers for <see cref="ZeroInstall.DesktopIntegration.AccessPoints.DefaultAccessPoint"/>s.
        /// </summary>
        /// <typeparam name="T">The specific kind of <see cref="ZeroInstall.DesktopIntegration.AccessPoints.DefaultAccessPoint"/> to handle.</typeparam>
        /// <param name="checkBoxSimple">The simple mode checkbox for this type of <see cref="ZeroInstall.DesktopIntegration.AccessPoints.CommandAccessPoint"/>.</param>
        /// <param name="labelSimple">A simple mode description for this type of <see cref="ZeroInstall.DesktopIntegration.AccessPoints.CommandAccessPoint"/>.</param>
        /// <param name="checkBoxSelectAll">The "select all" checkbox for this type of <see cref="ZeroInstall.DesktopIntegration.AccessPoints.CommandAccessPoint"/>.</param>
        /// <param name="model">A model represeting the underlying <see cref="ZeroInstall.Model.Capabilities.DefaultCapability"/>s and their selection states.</param>
        private void SetupDefaultAccessPoint<T>(CheckBox checkBoxSimple, Label labelSimple, CheckBox checkBoxSelectAll, BindingList<T> model)
            where T : CapabilityModel
        {
            bool suppressEvent = false;
            checkBoxSelectAll.CheckedChanged += delegate { if (!suppressEvent) CapabilityModelSetAll(model, checkBoxSelectAll.Checked); };

            _switchToBasicMode += () => SetDefaultAccessPointCheckBox(checkBoxSimple, labelSimple, model);
            _switchToAdvancedMode += () =>
            {
                ApplyDefaultAccessPointCheckBox(checkBoxSimple, model);

                suppressEvent = true;
                checkBoxSelectAll.Checked = model.All(element => element.Use);
                suppressEvent = false;
            };
        }

        /// <summary>
        /// Reads the <see cref="ZeroInstall.Model.Capabilities.DefaultCapability"/>s from <see cref="Feed.CapabilityLists"/> and creates a coressponding model for turning <see cref="ZeroInstall.DesktopIntegration.AccessPoints.DefaultAccessPoint"/> on and off.
        /// </summary>
        private void LoadDefaultAccessPoints()
        {
            foreach (var capabilityList in _appEntry.CapabilityLists.Where(list => list.Architecture.IsCompatible(Architecture.CurrentSystem)))
            {
                var dispatcher = new PerTypeDispatcher<Model.Capabilities.Capability>(true)
                {
                    (Model.Capabilities.FileType fileType) =>
                    {
                        var model = new FileTypeModel(fileType, IsCapabillityUsed<DesktopIntegration.AccessPoints.FileType>(fileType));
                        _fileTypesBinding.Add(model);
                        _capabilityModels.Add(model);
                    },
                    (Model.Capabilities.UrlProtocol urlProtocol) =>
                    {
                        var model = new UrlProtocolModel(urlProtocol, IsCapabillityUsed<DesktopIntegration.AccessPoints.UrlProtocol>(urlProtocol));
                        _urlProtocolsBinding.Add(model);
                        _capabilityModels.Add(model);
                    },
                    (Model.Capabilities.AutoPlay autoPlay) =>
                    {
                        var model = new AutoPlayModel(autoPlay, IsCapabillityUsed<DesktopIntegration.AccessPoints.AutoPlay>(autoPlay));
                        _autoPlayBinding.Add(model);
                        _capabilityModels.Add(model);
                    },
                    (Model.Capabilities.ContextMenu contextMenu) =>
                    {
                        var model = new ContextMenuModel(contextMenu, IsCapabillityUsed<DesktopIntegration.AccessPoints.ContextMenu>(contextMenu));
                        _contextMenuBinding.Add(model);
                        _capabilityModels.Add(model);
                    }
                };
                if (_integrationManager.MachineWide)
                {
                    dispatcher.Add((Model.Capabilities.DefaultProgram defaultProgram) =>
                    {
                        var model = new DefaultProgramModel(defaultProgram, IsCapabillityUsed<DesktopIntegration.AccessPoints.DefaultProgram>(defaultProgram));
                        _defaultProgramBinding.Add(model);
                        _capabilityModels.Add(model);
                    });
                }

                dispatcher.Dispatch(capabilityList.Entries);
            }

            // Apply data to DataGrids in bulk for better performance (remove empty tabs)
            if (_fileTypesBinding.Any()) dataGridFileTypes.DataSource = _fileTypesBinding;
            else tabControl.TabPages.Remove(tabPageFileTypes);
            if (_urlProtocolsBinding.Any()) dataGridUrlProtocols.DataSource = _urlProtocolsBinding;
            else tabControl.TabPages.Remove(tabPageUrlProtocols);
            if (_autoPlayBinding.Any()) dataGridAutoPlay.DataSource = _autoPlayBinding;
            else tabControl.TabPages.Remove(tabPageAutoPlay);
            if (_contextMenuBinding.Any()) dataGridContextMenu.DataSource = _contextMenuBinding;
            else tabControl.TabPages.Remove(tabPageContextMenu);
            if (_defaultProgramBinding.Any()) dataGridDefaultPrograms.DataSource = _defaultProgramBinding;
            else tabControl.TabPages.Remove(tabPageDefaultPrograms);
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Checks whether a <see cref="ZeroInstall.Model.Capabilities.DefaultCapability"/> is already used by the user.
        /// </summary>
        /// <typeparam name="T">The specific kind of <see cref="ZeroInstall.DesktopIntegration.AccessPoints.DefaultAccessPoint"/> to handle.</typeparam>
        /// <param name="toCheck">The <see cref="ZeroInstall.Model.Capabilities.Capability"/> to check for usage.</param>
        /// <returns><see langword="true"/>, if <paramref name="toCheck"/> is already in usage.</returns>
        private bool IsCapabillityUsed<T>(Model.Capabilities.DefaultCapability toCheck)
            where T : DesktopIntegration.AccessPoints.DefaultAccessPoint
        {
            if (_appEntry.AccessPoints == null) return false;

            return _appEntry.AccessPoints.Entries.OfType<T>().Any(accessPoint => accessPoint.Capability == toCheck.ID);
        }

        /// <summary>
        /// Sets all <see cref="CapabilityModel.Use"/> values within a list/model to a specific value.
        /// </summary>
        /// <typeparam name="T">The specific kind of <see cref="ZeroInstall.DesktopIntegration.AccessPoints.DefaultAccessPoint"/> to handle.</typeparam>
        /// <param name="model">A model represeting the underlying <see cref="ZeroInstall.Model.Capabilities.DefaultCapability"/>s and their selection states.</param>
        /// <param name="value">The value to set.</param>
        private static void CapabilityModelSetAll<T>(BindingList<T> model, bool value)
            where T : CapabilityModel
        {
            foreach (var element in model.Where(element => !element.Capability.ExplicitOnly))
                element.Use = value;
            model.ResetBindings();
        }
        #endregion

        //--------------------//

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
        /// Configures the visibility and check state of a <see cref="CheckBox"/> for simple <see cref="ZeroInstall.DesktopIntegration.AccessPoints.CommandAccessPoint"/> configuration.
        /// </summary>
        /// <typeparam name="T">The specific kind of <see cref="ZeroInstall.DesktopIntegration.AccessPoints.CommandAccessPoint"/> to handle.</typeparam>
        /// <param name="checkBox">The <see cref="CheckBox"/> to configure.</param>
        /// <param name="label">A description for the <paramref name="checkBox"/>.</param>
        /// <param name="accessPoints">The currently applied <see cref="ZeroInstall.DesktopIntegration.AccessPoints.CommandAccessPoint"/>.</param>
        /// <param name="getSuggestions">Retrieves a list of default <see cref="ZeroInstall.DesktopIntegration.AccessPoints.CommandAccessPoint"/> suggested by the system.</param>
        private static void SetCommandAccessPointCheckBox<T>(CheckBox checkBox, Label label, IEnumerable<T> accessPoints, Func<IEnumerable<T>> getSuggestions)
            where T : DesktopIntegration.AccessPoints.CommandAccessPoint
        {
            checkBox.Checked = accessPoints.Any();
            label.Visible = checkBox.Visible = getSuggestions().Any();
        }

        /// <summary>
        /// Configures the visibility and check state of a <see cref="CheckBox"/> for simple <see cref="ZeroInstall.DesktopIntegration.AccessPoints.DefaultAccessPoint"/> configuration.
        /// </summary>
        /// <typeparam name="T">The specific kind of <see cref="ZeroInstall.DesktopIntegration.AccessPoints.DefaultAccessPoint"/> to handle.</typeparam>
        /// <param name="checkBox">The <see cref="CheckBox"/> to configure.</param>
        /// <param name="label">A description for the <paramref name="checkBox"/>.</param>
        /// <param name="model">A model represeting the underlying <see cref="ZeroInstall.Model.Capabilities.DefaultCapability"/>s and their selection states.</param>
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
        /// Applies the state of a <see cref="CheckBox"/> for simple <see cref="ZeroInstall.DesktopIntegration.AccessPoints.CommandAccessPoint"/> configuration.
        /// </summary>
        /// <typeparam name="T">The specific kind of <see cref="ZeroInstall.DesktopIntegration.AccessPoints.CommandAccessPoint"/> to handle.</typeparam>
        /// <param name="checkBox">The <see cref="CheckBox"/> to read.</param>
        /// <param name="current">The currently applied <see cref="ZeroInstall.DesktopIntegration.AccessPoints.CommandAccessPoint"/>.</param>
        /// <param name="getSuggestions">Retrieves a list of default <see cref="ZeroInstall.DesktopIntegration.AccessPoints.CommandAccessPoint"/> suggested by the system.</param>
        private static void ApplyCommandAccessPointCheckBox<T>(CheckBox checkBox, ICollection<T> current, Func<IEnumerable<T>> getSuggestions)
            where T : DesktopIntegration.AccessPoints.CommandAccessPoint
        {
            if (!checkBox.Visible) return;
            if (checkBox.Checked)
            {
                if (current.Count == 0) foreach (var entry in getSuggestions()) current.Add(entry);
            }
            else current.Clear();
        }

        /// <summary>
        /// Applies the state of a <see cref="CheckBox"/> for simple <see cref="ZeroInstall.DesktopIntegration.AccessPoints.DefaultAccessPoint"/> configuration.
        /// </summary>
        /// <typeparam name="T">The specific kind of <see cref="ZeroInstall.DesktopIntegration.AccessPoints.DefaultAccessPoint"/> to handle.</typeparam>
        /// <param name="checkBox">The <see cref="CheckBox"/> to read.</param>
        /// <param name="model">A model represeting the underlying <see cref="ZeroInstall.Model.Capabilities.DefaultCapability"/>s and their selection states.</param>
        private static void ApplyDefaultAccessPointCheckBox<T>(CheckBox checkBox, BindingList<T> model)
            where T : CapabilityModel
        {
            if (!checkBox.Visible) return;
            if (checkBox.Checked)
            {
                if (!model.Any(element => element.Use)) CapabilityModelSetAll(model, true);
            }
            else CapabilityModelSetAll(model, false);
        }
        #endregion

        #region Apply
        /// <summary>
        /// Integrates all <see cref="ZeroInstall.Model.Capabilities.Capability"/>s chosen by the user.
        /// </summary>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            // Hide so that the underlying progress tracker is visible
            Visible = false;

            var toAdd = new List<DesktopIntegration.AccessPoints.AccessPoint>();
            var toRemove = new List<DesktopIntegration.AccessPoints.AccessPoint>();

            _appEntry.AutoUpdate = checkBoxAutoUpdate.Checked;
            (checkBoxCapabilities.Checked ? toAdd : toRemove).Add(new DesktopIntegration.AccessPoints.CapabilityRegistration());

            if (buttonAdvancedMode.Visible) _switchToAdvancedMode(); // Must do this to apply changes made in simple view
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
                // TODO: More comprehensive conflict handling
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                Visible = true; // Allow user to fix input
                return;
            }
            #endregion

            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// Determines changes to <see cref="ZeroInstall.DesktopIntegration.AccessPoints.CommandAccessPoint"/>s specified by the user.
        /// </summary>
        /// <param name="toAdd">List to add <see cref="ZeroInstall.DesktopIntegration.AccessPoints.AccessPoint"/>s to be added to.</param>
        /// <param name="toRemove">List to add <see cref="ZeroInstall.DesktopIntegration.AccessPoints.AccessPoint"/>s to be removed to.</param>
        private void HandleCommandAccessPointChanges(ICollection<DesktopIntegration.AccessPoints.AccessPoint> toAdd, ICollection<DesktopIntegration.AccessPoints.AccessPoint> toRemove)
        {
            // Build lists with current integration state
            var currentMenuEntries = new List<DesktopIntegration.AccessPoints.MenuEntry>();
            var currentDesktopIcons = new List<DesktopIntegration.AccessPoints.DesktopIcon>();
            var currentAliases = new List<DesktopIntegration.AccessPoints.AppAlias>();
            if (_appEntry.AccessPoints != null)
            {
                new PerTypeDispatcher<DesktopIntegration.AccessPoints.AccessPoint>(true)
                {
                    (DesktopIntegration.AccessPoints.MenuEntry menuEntry) => currentMenuEntries.Add(menuEntry),
                    (DesktopIntegration.AccessPoints.DesktopIcon desktopIcon) => currentDesktopIcons.Add(desktopIcon),
                    (DesktopIntegration.AccessPoints.AppAlias alias) => currentAliases.Add(alias)
                }.Dispatch(_appEntry.AccessPoints.Entries);
            }

            // Determine differences between current and desired state
            EnumerableUtils.Merge(_menuEntries, currentMenuEntries, toAdd.Add, toRemove.Add);
            EnumerableUtils.Merge(_desktopIcons, currentDesktopIcons, toAdd.Add, toRemove.Add);
            EnumerableUtils.Merge(_aliases, currentAliases, toAdd.Add, toRemove.Add);
        }

        /// <summary>
        /// Determines changes to <see cref="ZeroInstall.DesktopIntegration.AccessPoints.DefaultAccessPoint"/>s specified by the user.
        /// </summary>
        /// <param name="toAdd">List to add <see cref="ZeroInstall.DesktopIntegration.AccessPoints.AccessPoint"/>s to be added to.</param>
        /// <param name="toRemove">List to add <see cref="ZeroInstall.DesktopIntegration.AccessPoints.AccessPoint"/>s to be removed to.</param>
        private void HandleDefaultAccessPointChanges(ICollection<DesktopIntegration.AccessPoints.AccessPoint> toAdd, ICollection<DesktopIntegration.AccessPoints.AccessPoint> toRemove)
        {
            foreach (var model in _capabilityModels.Where(model => model.Changed))
            {
                var accessPoint = DesktopIntegration.AccessPoints.DefaultAccessPoint.FromCapability(model.Capability);
                if (model.Use) toAdd.Add(accessPoint);
                else toRemove.Add(accessPoint);
            }
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
    }
}
