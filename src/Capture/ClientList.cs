/*
 * Copyright 2011 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as Captureed by
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

namespace ZeroInstall.Capture
{
    /// <summary>
    /// Represents a list of clients for a specific type of service (e.g. web browsers).
    /// </summary>
    [Serializable]
    public struct ClientList
    {
        /// <summary>
        /// The name of the service (Mail, Media, etc.).
        /// </summary>
        public string ServiceName;

        /// <summary>
        /// The clients for the service.
        /// </summary>
        public string[] Clients;

        /// <summary>
        /// Creates a new client list
        /// </summary>
        /// <param name="serviceName">The name of the service (Mail, Media, etc.).</param>
        /// <param name="clients">The clients for the service.</param>
        public ClientList(string serviceName, string[] clients)
        {
            ServiceName = serviceName;
            Clients = clients;
        }

        /// <summary>
        /// Returns the client list in the form "ServiceName". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return ServiceName;
        }
    }
}
