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
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Controls;
using NanoByte.Common.Dispatch;
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
        /// A list of <see cref="DesktopIntegration.AccessPoints.MenuEntry"/>s as displayed by the <see cref="dataGridStartMenu"/>.
        /// </summary>
        private readonly BindingList<DesktopIntegration.AccessPoints.MenuEntry> _menuEntries = new BindingList<DesktopIntegration.AccessPoints.MenuEntry>();

        /// <summary>
        /// A list of <see cref="DesktopIntegration.AccessPoints.DesktopIcon"/>s as displayed by the <see cref="dataGridDesktop"/>.
        /// </summary>
        private readonly BindingList<DesktopIntegration.AccessPoints.DesktopIcon> _desktopIcons = new BindingList<DesktopIntegration.AccessPoints.DesktopIcon>();

        /// <summary>
        /// A list of <see cref="DesktopIntegration.AccessPoints.AppAlias"/>es as displayed by the <see cref="dataGridAliases"/>.
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
        /// Loads the <see cref="Store.Model.Capabilities.Capability"/>s of <see cref="_appEntry"/> into the controls of this form.
        /// </summary>
        private void IntegrateAppForm_Load(object sender, EventArgs e)
        {
            WindowsUtils.SetForegroundWindow(this);

            checkBoxAutoUpdate.Checked = _appEntry.AutoUpdate;
            checkBoxCapabilities.Visible = (_appEntry.CapabilityLists.Count != 0);
            checkBoxCapabilities.Checked = (_appEntry.AccessPoints == null) || _appEntry.AccessPoints.Entries.OfType<DesktopIntegration.AccessPoints.CapabilityRegistration>().Any();

            SetupCommandAccessPoints();
            SetupDefaultAccessPoints();
            _switchToBasicMode();
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
            SetupCommandAccessPoint(checkBoxStartMenuSimple, labelStartMenuSimple, _menuEntries, () => Suggest.MenuEntries(_feed));
            SetupCommandAccessPoint(checkBoxDesktopSimple, labelDesktopSimple, _desktopIcons, () => Suggest.DesktopIcons(_feed));
            SetupCommandAccessPoint(checkBoxAliasesSimple, labelAliasesSimple, _aliases, () => Suggest.Aliases(_feed));

            SetupCommandComboBoxes();
            LoadCommandAccessPoints();
            ShowCommandAccessPoints();
        }

        /// <summary>
        /// Fills the <see cref="DataGridViewComboBoxColumn"/>s with data from <see cref="Feed.EntryPoints"/> .
        /// </summary>
        private void SetupCommandComboBoxes()
        {
            var commands = _feed.EntryPoints
                .Select(entryPoint => entryPoint.Command)
                .Append(Command.NameRun).Distinct()
                .Cast<object>().ToArray();

            dataGridStartMenuColumnCommand.Items.AddRange(commands);
            dataGridDesktopColumnCommand.Items.AddRange(commands);
            dataGridAliasesColumnCommand.Items.AddRange(commands);
        }

        /// <summary>
        /// Reads the <see cref="DesktopIntegration.AccessPoints.CommandAccessPoint"/>s from <see cref="AppEntry.AccessPoints"/> or uses suggestion methods to fill in defaults.
        /// </summary>
        private void LoadCommandAccessPoints()
        {
            if (_appEntry.AccessPoints == null)
            { // Fill in default values for first integration
                foreach (var entry in Suggest.MenuEntries(_feed)) _menuEntries.Add(entry);
                foreach (var desktopIcon in Suggest.DesktopIcons(_feed)) _desktopIcons.Add(desktopIcon);
                foreach (var alias in Suggest.Aliases(_feed)) _aliases.Add(alias);
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
        }

        /// <summary>
        /// Apply data to DataGrids in bulk for better performance
        /// </summary>
        private void ShowCommandAccessPoints()
        {
            dataGridStartMenu.DataSource = _menuEntries;
            dataGridDesktop.DataSource = _desktopIcons;
            dataGridAliases.DataSource = _aliases;
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
            _switchToAdvancedMode += () => ApplyCommandAccessPointCheckBox(checkBoxSimple, accessPoints, getSuggestions);
        }
        #endregion

        #region Default access points
        /// <summary>
        /// Sets up the UI elements for configuring <see cref="DesktopIntegration.AccessPoints.DefaultAccessPoint"/>s.
        /// </summary>
        /// <remarks>Users enable or disable predefined <see cref="DesktopIntegration.AccessPoints.DefaultAccessPoint"/>s based on <see cref="Store.Model.Capabilities.DefaultCapability"/>s.</remarks>
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
            DisplayDefaultAccessPoints();
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
        /// Reads the <see cref="Store.Model.Capabilities.DefaultCapability"/>s from <see cref="Feed.CapabilityLists"/> and creates a coressponding model for turning <see cref="DesktopIntegration.AccessPoints.DefaultAccessPoint"/> on and off.
        /// </summary>
        private void LoadDefaultAccessPoints()
        {
            foreach (var capabilityList in _appEntry.CapabilityLists.Where(x => x.Architecture.IsCompatible()))
            {
                var dispatcher = new PerTypeDispatcher<Store.Model.Capabilities.Capability>(true)
                {
                    (Store.Model.Capabilities.FileType fileType) =>
                    {
                        var model = new FileTypeModel(fileType, IsCapabillityUsed<DesktopIntegration.AccessPoints.FileType>(fileType));
                        _fileTypesBinding.Add(model);
                        _capabilityModels.Add(model);
                    },
                    (Store.Model.Capabilities.UrlProtocol urlProtocol) =>
                    {
                        var model = new UrlProtocolModel(urlProtocol, IsCapabillityUsed<DesktopIntegration.AccessPoints.UrlProtocol>(urlProtocol));
                        _urlProtocolsBinding.Add(model);
                        _capabilityModels.Add(model);
                    },
                    (Store.Model.Capabilities.AutoPlay autoPlay) =>
                    {
                        var model = new AutoPlayModel(autoPlay, IsCapabillityUsed<DesktopIntegration.AccessPoints.AutoPlay>(autoPlay));
                        _autoPlayBinding.Add(model);
                        _capabilityModels.Add(model);
                    },
                    (Store.Model.Capabilities.ContextMenu contextMenu) =>
                    {
                        var model = new ContextMenuModel(contextMenu, IsCapabillityUsed<DesktopIntegration.AccessPoints.ContextMenu>(contextMenu));
                        _contextMenuBinding.Add(model);
                        _capabilityModels.Add(model);
                    }
                };
                if (_integrationManager.MachineWide)
                {
                    dispatcher.Add((Store.Model.Capabilities.DefaultProgram defaultProgram) =>
                    {
                        var model = new DefaultProgramModel(defaultProgram, IsCapabillityUsed<DesktopIntegration.AccessPoints.DefaultProgram>(defaultProgram));
                        _defaultProgramBinding.Add(model);
                        _capabilityModels.Add(model);
                    });
                }

                dispatcher.Dispatch(capabilityList.Entries);
            }
        }

        /// <summary>
        /// Apply data to DataGrids in bulk for better performance (remove empty tabs).
        /// </summary>
        private void DisplayDefaultAccessPoints()
        {
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
        /// Checks whether a <see cref="Store.Model.Capabilities.DefaultCapability"/> is already used by the user.
        /// </summary>
        /// <typeparam name="T">The specific kind of <see cref="DesktopIntegration.AccessPoints.DefaultAccessPoint"/> to handle.</typeparam>
        /// <param name="toCheck">The <see cref="Store.Model.Capabilities.Capability"/> to check for usage.</param>
        /// <returns><see langword="true"/>, if <paramref name="toCheck"/> is already in usage.</returns>
        private bool IsCapabillityUsed<T>(Store.Model.Capabilities.DefaultCapability toCheck)
            where T : DesktopIntegration.AccessPoints.DefaultAccessPoint
        {
            if (_appEntry.AccessPoints == null) return false;

            return _appEntry.AccessPoints.Entries.OfType<T>().Any(accessPoint => accessPoint.Capability == toCheck.ID);
        }

        /// <summary>
        /// Sets all <see cref="CapabilityModel.Use"/> values within a list/model to a specific value.
        /// </summary>
        /// <typeparam name="T">The specific kind of <see cref="DesktopIntegration.AccessPoints.DefaultAccessPoint"/> to handle.</typeparam>
        /// <param name="model">A model represeting the underlying <see cref="Store.Model.Capabilities.DefaultCapability"/>s and their selection states.</param>
        /// <param name="value">The value to set.</param>
        private static void CapabilityModelSetAll<T>(BindingList<T> model, bool value)
            where T : CapabilityModel
        {
            foreach (var element in model.Except(element => element.Capability.ExplicitOnly))
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
        /// Applies the state of a <see cref="CheckBox"/> for simple <see cref="DesktopIntegration.AccessPoints.CommandAccessPoint"/> configuration.
        /// </summary>
        /// <typeparam name="T">The specific kind of <see cref="DesktopIntegration.AccessPoints.CommandAccessPoint"/> to handle.</typeparam>
        /// <param name="checkBox">The <see cref="CheckBox"/> to read.</param>
        /// <param name="current">The currently applied <see cref="DesktopIntegration.AccessPoints.CommandAccessPoint"/>.</param>
        /// <param name="getSuggestions">Retrieves a list of default <see cref="DesktopIntegration.AccessPoints.CommandAccessPoint"/> suggested by the system.</param>
        private static void ApplyCommandAccessPointCheckBox<T>(CheckBox checkBox, ICollection<T> current, Func<IEnumerable<T>> getSuggestions)
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
        /// Integrates all <see cref="Store.Model.Capabilities.Capability"/>s chosen by the user.
        /// </summary>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            var toAdd = new List<DesktopIntegration.AccessPoints.AccessPoint>();
            var toRemove = new List<DesktopIntegration.AccessPoints.AccessPoint>();
            CollectChanges(toAdd, toRemove);
            ApplyChanges(toRemove, toAdd);
        }

        private void CollectChanges(List<DesktopIntegration.AccessPoints.AccessPoint> toAdd, List<DesktopIntegration.AccessPoints.AccessPoint> toRemove)
        {
            _appEntry.AutoUpdate = checkBoxAutoUpdate.Checked;
            (checkBoxCapabilities.Checked ? toAdd : toRemove).Add(new DesktopIntegration.AccessPoints.CapabilityRegistration());

            // Apply changes made in "Simple View"
            if (buttonAdvancedMode.Visible) _switchToAdvancedMode();

            CollectCommandAccessPointChanges(toAdd, toRemove);
            CollectDefaultAccessPointChanges(toAdd, toRemove);
        }

        private void CollectCommandAccessPointChanges(ICollection<DesktopIntegration.AccessPoints.AccessPoint> toAdd, ICollection<DesktopIntegration.AccessPoints.AccessPoint> toRemove)
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
            Merge.TwoWay(theirs: _menuEntries, mine: currentMenuEntries, added: toAdd.Add, removed: toRemove.Add);
            Merge.TwoWay(theirs: _desktopIcons, mine: currentDesktopIcons, added: toAdd.Add, removed: toRemove.Add);
            Merge.TwoWay(theirs: _aliases, mine: currentAliases, added: toAdd.Add, removed: toRemove.Add);
        }

        private void CollectDefaultAccessPointChanges(ICollection<DesktopIntegration.AccessPoints.AccessPoint> toAdd, ICollection<DesktopIntegration.AccessPoints.AccessPoint> toRemove)
        {
            foreach (var model in _capabilityModels.Where(model => model.Changed))
            {
                var accessPoint = model.Capability.ToAcessPoint();
                if (model.Use) toAdd.Add(accessPoint);
                else toRemove.Add(accessPoint);
            }
        }

        private void ApplyChanges(List<DesktopIntegration.AccessPoints.AccessPoint> toRemove, List<DesktopIntegration.AccessPoints.AccessPoint> toAdd)
        {
            // Hide so that the underlying progress tracker is visible
            Visible = false;

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
