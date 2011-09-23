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
using System.ComponentModel;
using Common.Collections;
using Common.Controls;
using ZeroInstall.Commands.WinForms.CapabilityModels;
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
        private readonly C5.LinkedList<CapabilityModel> _capabilityModels = new C5.LinkedList<CapabilityModel>();
        #endregion

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
                form.ShowDialog();
        }

        /// <summary>
        /// Loads the <see cref="Capabilities.Capability"/>s of <see cref="_appEntry"/> into the controls of this form.
        /// </summary>
        /// <param name="sender">not used.</param>
        /// <param name="e">not used.</param>
        private void IntegrateAppForm_Load(object sender, EventArgs e)
        {
            var defaultProgramBinding = new BindingList<DefaultProgramModel>();
            var fileTypeBinding = new BindingList<FileTypeModel>();
            var urlProtocolBinding = new BindingList<UrlProtocolModel>();

            foreach (Capabilities.CapabilityList capabilityList in _appEntry.CapabilityLists.FindAll(list => list.Architecture.IsCompatible(Architecture.CurrentSystem)))
            {
                // file types
                foreach (var fileType in EnumerableUtils.OfType<Capabilities.FileType>(capabilityList.Entries))
                {
                    var model = new FileTypeModel(fileType, IsCapabillityUsed<AccessPoints.FileType>(fileType));
                    fileTypeBinding.Add(model);
                    _capabilityModels.Add(model);
                }

                // url protocols
                foreach (var urlProtocol in EnumerableUtils.OfType<Capabilities.UrlProtocol>(capabilityList.Entries))
                {
                    var model = new UrlProtocolModel(urlProtocol, IsCapabillityUsed<AccessPoints.UrlProtocol>(urlProtocol));
                    urlProtocolBinding.Add(model);
                    _capabilityModels.Add(model);
                }

                if (_integrationManager.SystemWide)
                {
                    // default program
                    foreach (var defaultProgram in EnumerableUtils.OfType<Capabilities.DefaultProgram>(capabilityList.Entries))
                    {
                        var model = new DefaultProgramModel(defaultProgram, IsCapabillityUsed<AccessPoints.DefaultProgram>(defaultProgram));
                        defaultProgramBinding.Add(model);
                        _capabilityModels.Add(model);
                    }
                }
            }

            // Apply data to DataGrids in bulk for better performance
            dataGridViewFileType.DataSource = fileTypeBinding;
            dataGridViewUrlProtocols.DataSource = urlProtocolBinding;
            dataGridViewDefaultPrograms.Visible = _integrationManager.SystemWide;
            dataGridViewDefaultPrograms.DataSource = defaultProgramBinding;
        }

        /// <summary>
        /// Checks whether a <see cref="Capabilities.Capability" /> is already used by the user.
        /// </summary>
        /// <typeparam name="T">Type of the <see cref="ZeroInstall.DesktopIntegration.AccessPoints.DefaultAccessPoint"/> to which <paramref name="toCheck"/> belongs to.</typeparam>
        /// <param name="toCheck">The <see cref="Capabilities.Capability"/> to check for usage.</param>
        /// <returns><see langword="true"/>, if <paramref name="toCheck"/> is already in usage.</returns>
        private bool IsCapabillityUsed<T>(Capabilities.Capability toCheck) where T : AccessPoints.DefaultAccessPoint
        {
            if (_appEntry.AccessPoints == null) return false;

            foreach (T accessPoint in EnumerableUtils.OfType<T>(_appEntry.AccessPoints.Entries))
                if (accessPoint.Capability == toCheck.ID) return true;
            return false;
        }

        /// <summary>
        /// Integrates all <see cref="Capabilities.Capability"/>s chosen by the user.
        /// </summary>
        /// <param name="sender">not used.</param>
        /// <param name="e">not used.</param>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            // Hide so that the underlying progress tracker is visible
            Hide();

            var toAdd = new C5.LinkedList<AccessPoints.AccessPoint>();
            var toRemove = new C5.LinkedList<AccessPoints.AccessPoint>();

            foreach (var capabilityModel in _capabilityModels)
            {
                if (capabilityModel.Changed)
                {
                    var accessPoint = AccessPoints.DefaultAccessPoint.FromCapability(capabilityModel.Capability);
                    if (capabilityModel.Use) toAdd.Add(accessPoint);
                    else toRemove.Add(accessPoint);
                }
            }

            _integrationManager.RemoveAccessPoints(_appEntry, toRemove);
            _integrationManager.AddAccessPoints(_appEntry, _feed, toAdd);
        }
    }
}
