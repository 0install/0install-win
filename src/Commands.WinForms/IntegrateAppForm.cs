/*
 * Copyright 2010-2011 Bastian Eicher, Simon E. Silva Lauinger
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
    public partial class IntegrateAppForm : OKCancelDialog
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
        private readonly ICollection<CapabilityModel> _capabilityModels = new LinkedList<CapabilityModel>();

        /// <summary>
        /// A list of <see cref="AccessPoints.MenuEntry"/>s as displayed by the <see cref="dataGridStartMenu"/>.
        /// </summary>
        private readonly BindingList<AccessPoints.MenuEntry> _menuEntries = new BindingList<AccessPoints.MenuEntry>();

        /// <summary>
        /// A list of <see cref="AccessPoints.DesktopIcon"/>s as displayed by the <see cref="dataGridDesktop"/>.
        /// </summary>
        private readonly BindingList<AccessPoints.DesktopIcon> _desktopIcons = new BindingList<AccessPoints.DesktopIcon>();
        #endregion

        #region Constructor
        /// <summary>
        /// Creates an instance of the form.
        /// </summary>
        /// <param name="integrationManager">The integration manager used to apply selected integration options.</param>
        /// <param name="appEntry">The application being integrated.</param>
        /// <param name="feed">The feed providing additional metadata, icons, etc. for the application.</param>
        private IntegrateAppForm(IIntegrationManager integrationManager, AppEntry appEntry, Feed feed)
        {
            InitializeComponent();

            _integrationManager = integrationManager;
            _appEntry = appEntry;
            _feed = feed;
        }
        #endregion

        #region Static access
        /// <summary>
        /// Displays the form as a modal dialog.
        /// </summary>
        /// <param name="integrationManager">The integration manager used to apply selected integration options.</param>
        /// <param name="appEntry">The application being integrated.</param>
        /// <param name="feed">The feed providing additional metadata, icons, etc. for the application.</param>
        public static void ShowDialog(IIntegrationManager integrationManager, AppEntry appEntry, Feed feed)
        {
            #region Sanity checks
            if (integrationManager == null) throw new ArgumentNullException("integrationManager");
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            if (feed == null) throw new ArgumentNullException("feed");
            #endregion

            using (var form = new IntegrateAppForm(integrationManager, appEntry, feed))
            {
                form.Text = string.Format(Resources.Integrate, appEntry.Name);
                form.ShowDialog();
            }
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
            checkBoxCapabilities.Checked = (_appEntry.AccessPoints == null) || !EnumerableUtils.IsEmpty(EnumerableUtils.OfType<AccessPoints.CapabilityRegistration>(_appEntry.AccessPoints.Entries));

            SetupCommandAccessPoints();
            SetupDefaultAccessPoints();
        }

        /// <summary>
        /// Sets up the UI elements for configuring <see cref="AccessPoints.CommandAccessPoint"/>s.
        /// </summary>
        private void SetupCommandAccessPoints()
        {
            var commands = _feed.EntryPoints.Map(entryPoint => entryPoint.Command);
            commands.Remove(Command.NameRun);
            commands.Insert(0, "");
            var commandsArray = commands.ToArray();
            dataGridStartMenuColumnCommand.Items.AddRange(commandsArray);
            dataGridDesktopColumnCommand.Items.AddRange(commandsArray);

            if (_appEntry.AccessPoints == null)
            { // Set useful defaults for first integration
                // Add icons for main entry point
                _desktopIcons.Add(new AccessPoints.DesktopIcon {Name = _appEntry.Name});
                if (_feed.EntryPoints.IsEmpty)
                    _menuEntries.Add(new AccessPoints.MenuEntry {Name = _appEntry.Name, Category = _appEntry.Name});

                // Add icons for additional entry points
                foreach (var entryPoint in _feed.EntryPoints)
                {
                    string entryPointName = entryPoint.Names.GetBestLanguage(CultureInfo.CurrentUICulture);
                    if (!string.IsNullOrEmpty(entryPoint.Command) && !string.IsNullOrEmpty(entryPointName))
                    {
                        _menuEntries.Add(new AccessPoints.MenuEntry
                        {
                            Name = entryPointName,
                            Category = _appEntry.Name,
                            // Don't explicitly write the "run" command 
                            Command = (entryPoint.Command == Command.NameRun ? null : entryPoint.Command)
                        });
                    }
                }
            }
            else
            { // Determine currently existing items
                foreach (var entry in EnumerableUtils.OfType<AccessPoints.MenuEntry>(_appEntry.AccessPoints.Entries))
                    _menuEntries.Add((AccessPoints.MenuEntry)entry.CloneAccessPoint());
                foreach (var entry in EnumerableUtils.OfType<AccessPoints.DesktopIcon>(_appEntry.AccessPoints.Entries))
                    _desktopIcons.Add((AccessPoints.DesktopIcon)entry.CloneAccessPoint());
            }

            // Apply data to DataGrids in bulk for better performance
            dataGridStartMenu.DataSource = _menuEntries;
            dataGridDesktop.DataSource = _desktopIcons;
        }

        /// <summary>
        /// Sets up the UI elements for configuring <see cref="AccessPoints.DefaultAccessPoint"/>s.
        /// </summary>
        private void SetupDefaultAccessPoints()
        {
            var fileTypesBinding = new BindingList<FileTypeModel>();
            var urlProtocolsBinding = new BindingList<UrlProtocolModel>();
            var autoPlayBinding = new BindingList<AutoPlayModel>();
            var contextMenuBinding = new BindingList<ContextMenuModel>();
            var defaultProgramBinding = new BindingList<DefaultProgramModel>();

            foreach (var capabilityList in _appEntry.CapabilityLists)
            {
                if (!capabilityList.Architecture.IsCompatible(Architecture.CurrentSystem)) continue;

                // File types
                foreach (var fileType in EnumerableUtils.OfType<Capabilities.FileType>(capabilityList.Entries))
                {
                    var model = new FileTypeModel(fileType, IsCapabillityUsed<AccessPoints.FileType>(fileType));
                    fileTypesBinding.Add(model);
                    _capabilityModels.Add(model);
                }

                // URL protocols
                foreach (var urlProtocol in EnumerableUtils.OfType<Capabilities.UrlProtocol>(capabilityList.Entries))
                {
                    var model = new UrlProtocolModel(urlProtocol, IsCapabillityUsed<AccessPoints.UrlProtocol>(urlProtocol));
                    urlProtocolsBinding.Add(model);
                    _capabilityModels.Add(model);
                }

                // AutoPlay
                foreach (var autoPlay in EnumerableUtils.OfType<Capabilities.AutoPlay>(capabilityList.Entries))
                {
                    var model = new AutoPlayModel(autoPlay, IsCapabillityUsed<AccessPoints.AutoPlay>(autoPlay));
                    autoPlayBinding.Add(model);
                    _capabilityModels.Add(model);
                }

                // Context menu
                foreach (var contextMenu in EnumerableUtils.OfType<Capabilities.ContextMenu>(capabilityList.Entries))
                {
                    var model = new ContextMenuModel(contextMenu, IsCapabillityUsed<AccessPoints.ContextMenu>(contextMenu));
                    contextMenuBinding.Add(model);
                    _capabilityModels.Add(model);
                }

                if (_integrationManager.SystemWide)
                {
                    // Default program
                    foreach (var defaultProgram in EnumerableUtils.OfType<Capabilities.DefaultProgram>(capabilityList.Entries))
                    {
                        var model = new DefaultProgramModel(defaultProgram, IsCapabillityUsed<AccessPoints.DefaultProgram>(defaultProgram));
                        defaultProgramBinding.Add(model);
                        _capabilityModels.Add(model);
                    }
                }
            }

            // Apply data to DataGrids in bulk for better performance (remove empty tabs)
            if (fileTypesBinding.Count != 0) dataGridFileType.DataSource = fileTypesBinding;
            else tabControl.TabPages.Remove(tabPageFileTypes);
            if (urlProtocolsBinding.Count != 0) dataGridUrlProtocols.DataSource = urlProtocolsBinding;
            else tabControl.TabPages.Remove(tabPageUrlProtocols);
            if (autoPlayBinding.Count != 0) dataGridAutoPlay.DataSource = autoPlayBinding;
            else tabControl.TabPages.Remove(tabPageAutoPlay);
            if (contextMenuBinding.Count != 0) dataGridContextMenu.DataSource = contextMenuBinding;
            else tabControl.TabPages.Remove(tabPageContextMenu);
            if (defaultProgramBinding.Count != 0 && _integrationManager.SystemWide) dataGridDefaultPrograms.DataSource = defaultProgramBinding;
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

            foreach (T accessPoint in EnumerableUtils.OfType<T>(_appEntry.AccessPoints.Entries))
                if (accessPoint.Capability == toCheck.ID) return true;
            return false;
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

            var toAdd = new C5.LinkedList<AccessPoints.AccessPoint>();
            var toRemove = new C5.LinkedList<AccessPoints.AccessPoint>();

            _appEntry.AutoUpdate = checkBoxAutoUpdate.Checked;
            (checkBoxCapabilities.Checked ? toAdd : toRemove).Add(new AccessPoints.CapabilityRegistration());

            HandleCommandAccessPoints(toAdd, toRemove);
            HandleDefaultAccessPoints(toAdd, toRemove);

            try
            {
                _integrationManager.RemoveAccessPoints(_appEntry, toRemove);
                _integrationManager.AddAccessPoints(_appEntry, _feed, toAdd);
            }
                #region Error handling
            catch (UserCancelException)
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
        private void HandleCommandAccessPoints(C5.IExtensible<AccessPoints.AccessPoint> toAdd, C5.IExtensible<AccessPoints.AccessPoint> toRemove)
        {
            var currentMenuEntries = new List<AccessPoints.MenuEntry>();
            var currenDesktopicons = new List<AccessPoints.DesktopIcon>();
            if (_appEntry.AccessPoints != null)
            {
                currentMenuEntries.AddRange(EnumerableUtils.OfType<AccessPoints.MenuEntry>(_appEntry.AccessPoints.Entries));
                currenDesktopicons.AddRange(EnumerableUtils.OfType<AccessPoints.DesktopIcon>(_appEntry.AccessPoints.Entries));
            }
            EnumerableUtils.Merge(_menuEntries, currentMenuEntries, menuEntry => toAdd.Add(menuEntry), menuEntry => toRemove.Add(menuEntry));
            EnumerableUtils.Merge(_desktopIcons, currenDesktopicons, desktopIcons => toAdd.Add(desktopIcons), desktopIcons => toRemove.Add(desktopIcons));
        }

        /// <summary>
        /// Determines changes to <see cref="AccessPoints.DefaultAccessPoint"/>s specified by the user.
        /// </summary>
        /// <param name="toAdd">List to add <see cref="AccessPoints.AccessPoint"/>s to be added to.</param>
        /// <param name="toRemove">List to add <see cref="AccessPoints.AccessPoint"/>s to be removed to.</param>
        private void HandleDefaultAccessPoints(C5.IExtensible<AccessPoints.AccessPoint> toAdd, C5.IExtensible<AccessPoints.AccessPoint> toRemove)
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

        #region DataGrids
        private void dataGridStartMenu_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataGridStartMenuColumnRemove.Index)
                _menuEntries.RemoveAt(e.RowIndex);
        }

        private void dataGridDesktop_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataGridDesktopColumnRemove.Index)
                _desktopIcons.RemoveAt(e.RowIndex);
        }
        #endregion
    }
}
