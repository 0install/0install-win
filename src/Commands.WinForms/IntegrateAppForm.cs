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
using Common.Collections;
using Common.Controls;
using ZeroInstall.Commands.WinForms.CapabilityModels;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.Model;
using ZeroInstall.Model.Capabilities;
using DefaultProgram = ZeroInstall.Model.Capabilities.DefaultProgram;
using FileType = ZeroInstall.Model.Capabilities.FileType;
using UrlProtocol = ZeroInstall.Model.Capabilities.UrlProtocol;

namespace ZeroInstall.Commands.WinForms
{
    /// <summary>
    /// A dialog form used for displaying and applying desktop integration options for applications.
    /// </summary>
    public partial class IntegrateAppForm : OKCancelDialog
    {
        #region Variables
        /// <summary>
        /// The <see cref="IIntegrationManager"/> to be used by this form.
        /// </summary>
        private readonly IIntegrationManager _integrationManager;

        /// <summary>
        /// The <see cref="InterfaceFeed"/> to be used by this form.
        /// </summary>
        private readonly InterfaceFeed _target;

        /// <summary>
        /// List of the <see cref="Model.Capabilities.DefaultProgram"/>s handled by this form.
        /// </summary>
        private readonly BindingList<DefaultProgramModel> _defaultProgramList = new BindingList<DefaultProgramModel>();

        /// <summary>
        /// List of the <see cref="Model.Capabilities.FileType"/>s handled by this form.
        /// </summary>
        private readonly BindingList<FileTypeModel> _fileTypeList = new BindingList<FileTypeModel>();

        /// <summary>
        /// List of the <see cref="Model.Capabilities.UrlProtocol"/>s handled by this form.
        /// </summary>
        private readonly BindingList<UrlProtocolModel> _urlProtocolList = new BindingList<UrlProtocolModel>();
        #endregion

        /// <summary>
        /// Creates an instance of the form.
        /// </summary>
        /// <param name="integrationManager">The <see cref="IIntegrationManager"/> to be used by this form.</param>
        /// <param name="target">The <see cref="InterfaceFeed"/> to be used by this form.</param>
        private IntegrateAppForm(IIntegrationManager integrationManager, InterfaceFeed target)
        {
            InitializeComponent();

            _integrationManager = integrationManager;
            _target = target;
        }

        /// <summary>
        /// Displays the form as a modal dialog.
        /// </summary>
        /// <param name="integrationManager">The integration manager used to apply selected integration options.</param>
        /// <param name="target">The application to be integrated.</param>
        public static void ShowDialog(IIntegrationManager integrationManager, InterfaceFeed target)
        {
            #region Sanity checks
            if (integrationManager == null) throw new ArgumentNullException("integrationManager");
            #endregion

            using (var form = new IntegrateAppForm(integrationManager, target))
                form.ShowDialog();
        }

        /// <summary>
        /// Loads the <see cref="Capability"/>s of <see cref="_target"/> into the controls of this form.
        /// </summary>
        /// <param name="sender">not used.</param>
        /// <param name="e">not used.</param>
        private void IntegrateAppForm_Load(object sender, EventArgs e)
        {
            foreach (CapabilityList capabilityList in _target.Feed.CapabilityLists.FindAll(list => list.Architecture.IsCompatible(Architecture.CurrentSystem)))
            {
                // file types
                foreach (FileType fileType in EnumerableUtils.OfType<FileType>(capabilityList.Entries))
                    _fileTypeList.Add(new FileTypeModel(fileType, IsCapabillityUsed<DesktopIntegration.AccessPoints.FileType>(fileType)));

                // url protocols
                foreach (UrlProtocol urlProtocol in EnumerableUtils.OfType<UrlProtocol>(capabilityList.Entries))
                    _urlProtocolList.Add(new UrlProtocolModel(urlProtocol, IsCapabillityUsed<DesktopIntegration.AccessPoints.UrlProtocol>(urlProtocol)));

                // TODO: show only if --global
                // default program
                foreach (DefaultProgram defaultProgram in EnumerableUtils.OfType<DefaultProgram>(capabilityList.Entries))
                    _defaultProgramList.Add(new DefaultProgramModel(defaultProgram, IsCapabillityUsed<DesktopIntegration.AccessPoints.DefaultProgram>(defaultProgram)));
            }

            dataGridViewFileType.DataSource = _fileTypeList;
            dataGridViewUrlProtocols.DataSource = _urlProtocolList;
            dataGridViewDefaultPrograms.DataSource = _defaultProgramList;
        }

        /// <summary>
        /// Checks whether a <see cref="Capability" /> is already used by the user.
        /// </summary>
        /// <typeparam name="T">Type of the <see cref="ZeroInstall.DesktopIntegration.AccessPoints.DefaultAccessPoint"/> to which <paramref name="toCheck"/> belongs to.</typeparam>
        /// <param name="toCheck">The <see cref="Capability"/> to check for usage.</param>
        /// <returns><see langword="true"/>, if <paramref name="toCheck"/> is already in usage.</returns>
        private bool IsCapabillityUsed<T>(Capability toCheck) where T : DefaultAccessPoint
        {
            AppEntry appEntry;
            if (_integrationManager.AppList.Entries.Find(entry => entry.InterfaceID == _target.InterfaceID, out appEntry))
            {
                foreach (T accessPoint in EnumerableUtils.OfType<T>(appEntry.AccessPoints.Entries))
                    if (accessPoint.Capability == toCheck.ID) return true;
            }
            return false;
        }

        /// <summary>
        /// Integrates all <see cref="Capability"/>s chosen by the user.
        /// </summary>
        /// <param name="sender">not used.</param>
        /// <param name="e">not used.</param>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            // Hide so that the underlying progress tracker is visible
            Hide();

            // file type
            ApplyDefaultCapabilitiesIntegration<FileType, FileTypeModel, DesktopIntegration.AccessPoints.FileType>(_fileTypeList);
            // url protocol
            ApplyDefaultCapabilitiesIntegration<UrlProtocol, UrlProtocolModel, DesktopIntegration.AccessPoints.UrlProtocol>(_urlProtocolList);
            // default program
            ApplyDefaultCapabilitiesIntegration<DefaultProgram, DefaultProgramModel, DesktopIntegration.AccessPoints.DefaultProgram>(_defaultProgramList);

            // ToDo: Apply remaining integrations
        }

        /// <summary>
        /// Adds and/or removes the integration of a list of <see cref="Capability"/>s.
        /// </summary>
        /// <typeparam name="TCapability">Type of the <see cref="Capability"/>.</typeparam>
        /// <typeparam name="TModel">Type of the <see cref="CapabilityModel{T}"/>. Should be a type that holds an <typeparamref name="TCapability"/>.</typeparam>
        /// <typeparam name="TAccessPoint">Type of the <see cref="ZeroInstall.DesktopIntegration.AccessPoints.AccessPoint"/>. Should be a suitable type to <typeparamref name="TCapability"/>.</typeparam>
        /// <param name="capabilityModels">That shall be integrated or removed.</param>
        private void ApplyDefaultCapabilitiesIntegration<TCapability, TModel, TAccessPoint>(IEnumerable<TModel> capabilityModels)
            where TCapability : Capability
            where TModel : CapabilityModel<TCapability>
            where TAccessPoint : DefaultAccessPoint, new()
        {
            foreach (TModel capabilityModel in capabilityModels)
            {
                if (!capabilityModel.Changed) continue;

                if (capabilityModel.Use)
                    _integrationManager.AddAccessPoints(_target, new AccessPoint[] {new TAccessPoint {Capability = capabilityModel.Capability.ID}});
                else
                    _integrationManager.RemoveAccessPoints(_target.InterfaceID, new AccessPoint[] {new TAccessPoint {Capability = capabilityModel.Capability.ID}});
            }
        }
    }
}
