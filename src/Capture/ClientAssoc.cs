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
    /// Represents the association of a client with a certain service (e.g. web browsers).
    /// </summary>
    [Serializable]
    public struct ClientAssoc : IComparable<ClientAssoc>
    {
        /// <summary>
        /// The name of the service (Mail, Media, etc.).
        /// </summary>
        public string Service;

        /// <summary>
        /// The name of the client.
        /// </summary>
        public string Client;

        /// <summary>
        /// Creates a new client association.
        /// </summary>
        /// <param name="service">The name of the service (Mail, Media, etc.).</param>
        /// <param name="client">The name of the client.</param>
        public ClientAssoc(string service, string client)
        {
            Service = service;
            Client = client;
        }

        /// <inheritdoc/>
        public int CompareTo(ClientAssoc other)
        {
            // Compare by Service first, then by Client if that was equal
            int serviceCompare = string.Compare(Service, other.Service);
            return (serviceCompare == 0) ? string.Compare(Client, other.Client) : serviceCompare;
        }

        /// <summary>
        /// Returns the client association in the form "Service = Client". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return Service + " = " + Client;
        }
    }
}
