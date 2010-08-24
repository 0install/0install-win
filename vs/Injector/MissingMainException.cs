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
using ZeroInstall.Injector.Properties;
using ZeroInstall.Model;

namespace ZeroInstall.Injector
{
    /// <summary>
    /// Indicates an <see cref="ImplementationBase"/> that was supposed to be launched did not specify a main executable.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "This exception type has a specific signaling purpose and doesn't need custom Messages")]
    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Justification = "This exception type has a specific signaling purpose and doesn't need to be serializable")]
    public class MissingMainException : Exception
    {
        #region Properties
        /// <summary>
        /// The ID (URI or file path) of the interface that is missing a main executable.
        /// </summary>
        public string InterfaceID { get; private set; }

        /// <inheritdoc />
        public override string Message
        {
            get
            {
                return string.Format(Resources.MissingMain, InterfaceID);
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates anew missing main exception.
        /// </summary>
        /// <param name="interfaceID">The ID (URI or file path) of the interface that is missing a main executable.</param>
        public MissingMainException(string interfaceID)
        {
            InterfaceID = interfaceID;
        }
        #endregion
    }
}
