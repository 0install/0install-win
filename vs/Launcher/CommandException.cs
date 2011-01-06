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
using System.Runtime.Serialization;
using System.Security.Permissions;
using ZeroInstall.Launcher.Properties;
using ZeroInstall.Model;

namespace ZeroInstall.Launcher
{
    /// <summary>
    /// Indicates <see cref="Launcher"/> had a problem locating or processing a <see cref="Command"/>.
    /// </summary>
    [Serializable]
    public sealed class CommandException : Exception
    {
        #region Properties
        /// <summary>
        /// The ID (URI or file path) of the interface that is missing a main executable.
        /// </summary>
        public string InterfaceID { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new missing main exception.
        /// </summary>
        /// <param name="interfaceID">The ID (URI or file path) of the interface that is missing a main executable.</param>
        public CommandException(string interfaceID)
            : base(string.Format(Resources.MissingMain, interfaceID))
        {
            InterfaceID = interfaceID;
        }

        /// <summary>
        /// Creates a new missing main exception.
        /// </summary>
        public CommandException()
            : base(string.Format(Resources.MissingMain, "unknown"))
        {}

        /// <summary>
        /// Creates a new missing main exception.
        /// </summary>
        public CommandException(string message, Exception innerException) : base(message, innerException)
        {}

        /// <summary>
        /// Deserializes an exception.
        /// </summary>
        private CommandException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            #region Sanity checks
            if (info == null) throw new ArgumentNullException("info");
            #endregion

            InterfaceID = info.GetString("InterfaceID");
        }
        #endregion

        #region Serialization
        /// <inheritdoc/>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            #region Sanity checks
            if (info == null) throw new ArgumentNullException("info");
            #endregion

            info.AddValue("InterfaceID", InterfaceID);

            base.GetObjectData(info, context);
        }
        #endregion
    }
}
