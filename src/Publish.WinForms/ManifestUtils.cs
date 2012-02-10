/*
 * Copyright 2010-2012 Bastian Eicher
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
using System.IO;
using System.Windows.Forms;
using Common.Controls;
using Common.Tasks;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Publish.WinForms
{
    /// <summary>
    /// Provides helper methods relating to <see cref="Manifest"/> and <see cref="ManifestDigest"/>s
    /// </summary>
    public static class ManifestUtils
    {
        /// <summary>
        /// Generates a <see cref="ManifestDigest"/> object for a directory containing digests for all <see cref="ManifestFormat.Recommended"/>.
        /// </summary>
        /// <param name="owner">The parent window any displayed progress windows are modal to.</param>
        /// <param name="path">The path of the directory to analyze.</param>
        /// <returns>The combined <see cref="ManifestDigest"/> structure.</returns>
        /// <exception cref="OperationCanceledException">Thrown if the user canceled the process.</exception>
        /// <exception cref="IOException">Thrown if the task ended with <see cref="TaskState.IOError"/>.</exception>
        public static ManifestDigest CreateDigest(IWin32Window owner, string path)
        {
            var digest = new ManifestDigest();

            // Generate manifest for each available format...
            foreach (var format in ManifestFormat.Recommended)
            {
                // ... and add the resulting digest to the return value
                var generator = new ManifestGenerator(path, format);
                TrackingDialog.Run(owner, generator, null);
                ManifestDigest.ParseID(generator.Result.CalculateDigest(), ref digest);
            }

            return digest;
        }
    }
}
