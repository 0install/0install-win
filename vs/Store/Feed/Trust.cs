/*
 * Copyright 2010 Bastian Eicher
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml.Serialization;
using Common;
using Common.Storage;
using ZeroInstall.Model;

namespace ZeroInstall.Store.Feed
{
    /// <summary>
    /// A database of trusted GPG signatures for <see cref="Feed"/>s.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [XmlRoot("trusted-keys", Namespace = "http://zero-install.sourceforge.net/2007/injector/trust")]
    public sealed class Trust
    {
        #region Variables
        private static readonly string
            _portablePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"trustdb.xml"),
            _profilePath = Path.Combine(Locations.GetUserSettingsDir(Path.Combine("0install.net", "injector")), @"trustdb.xml");

        /// <summary>Was the trust data loaded from the application's directory?</summary>
        private bool _loadedFromAppDir;
        #endregion

        #region Properties
        // Order is preserved, duplicate entries are not allowed
        private readonly C5.HashedArrayList<Key> _keys = new C5.HashedArrayList<Key>();
        /// <summary>
        /// A list of <see cref="Domain"/>s this key is valid for.
        /// </summary>
        [XmlElement("key")]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.HashedArrayList<Key> Keys { get { return _keys; } }
        #endregion

        //--------------------//

        #region Storage
        
        #region Load
        /// <summary>
        /// Loads the current trust data from an XML file.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Any problems when loading the trust data should be ignored")]
        public static Trust Load()
        {
            // ToDo: Combine data from multiple sources

            if (File.Exists(_portablePath))
            { // Try to load trust data file from the application's directory
                try
                {
                    var current = XmlStorage.Load<Trust>(_portablePath);
                    current._loadedFromAppDir = true;
                    Log.Info("Loaded trust data from installation directory");
                    return current;
                }
                catch (Exception ex)
                {
                    Log.Warn("Failed to load trust data from installation directory: " + ex.Message + "\nReverting to defaults");
                }
            }
            else
            { // Then fall back to the user profile
                try
                {
                    var current = XmlStorage.Load<Trust>(_profilePath);
                    Log.Info("Loaded trust data from user profile");
                    return current;
                }
                catch (Exception ex)
                {
                    Log.Warn("Failed to load trust data from user profile: " + ex.Message + "\nReverting to defaults");
                }
            }

            // Fall back to default values if both fail
            return new Trust();
        }
        #endregion

        #region Save
        /// <summary>
        /// Saves the current trust data to an XML file.
        /// </summary>
        public void Save()
        {
            try
            {
                if (_loadedFromAppDir)
                {
                    XmlStorage.Save(_portablePath, this);
                    Log.Info("Saved trust data to working directory");
                }
                else
                {
                    XmlStorage.Save(_profilePath, this);
                    Log.Info("Saved trust data to user profile");
                }
            }
            catch (IOException ex)
            {
                Log.Warn("Failed to save trust data: " + ex.Message);
            }
        }
        #endregion

        #endregion

        #region Values
        // ToDo: Add trust data
        #endregion
    }
}