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
using Common;

namespace ZeroInstall.Injector.Cli
{
    /// <summary>
    /// Uses the stderr stream to ask the user questions.
    /// </summary>
    public class CliFeedHandler : IHandler
    {
        /// <inheritdoc />
        public bool AcceptNewKey(string information)
        {
            Console.Error.WriteLine(information);

            while (true)
            {
                Console.Error.Write("[Y/N] ");
                switch ((Console.ReadLine() ?? "").ToLower())
                {
                    case "y":
                    case "yes":
                        return true;
                    case "n":
                    case "no":
                        return false;
                }
            }
        }

        /// <inheritdoc />
        public void StartingDownload(IProgress download)
        {
            // ToDo: Implement
        }

        /// <inheritdoc />
        public void StartingExtraction(IProgress extraction)
        {
            // ToDo: Implement
        }

        /// <inheritdoc />
        public void StartingManifest(IProgress manifest)
        {
            // ToDo: Implement
        }
    }
}
