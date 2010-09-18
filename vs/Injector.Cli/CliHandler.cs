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
using Common.CliControls;

namespace ZeroInstall.Injector.Cli
{
    /// <summary>
    /// Uses the stderr stream to ask the user questions.
    /// </summary>
    public class CliHandler : IHandler
    {
        /// <inheritdoc />
        public bool AcceptNewKey(string information)
        {
            Console.Error.WriteLine(information);

            while (true)
            {
                Console.Error.Write("Trust [Y/N] ");
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
            Console.Error.WriteLine("\nDownloading {0}...", download.Name);
            new TrackingProgressBar(download);
        }

        /// <inheritdoc />
        public void StartingExtraction(IProgress extraction)
        {
            Console.Error.WriteLine("\nExtracting...");
            new TrackingProgressBar(extraction);
        }

        /// <inheritdoc />
        public void StartingManifest(IProgress manifest)
        {
            Console.Error.WriteLine("\nGenerating manifest...");
            new TrackingProgressBar(manifest);
        }
    }
}
