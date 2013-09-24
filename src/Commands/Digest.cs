/*
 * Copyright 2010-2013 Bastian Eicher
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
using Common.Storage;
using NDesk.Options;
using ZeroInstall.Backend;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Implementation.Archive;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Calculate the manifest digest of a directory or archive.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class Digest : FrontendCommand
    {
        #region Constants
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "digest";
        #endregion

        #region Variables
        /// <summary>The hashing algorithm used to generate the manifest.</summary>
        private ManifestFormat _algorithm = ManifestFormat.Sha1New;

        private bool _printManifest, _printDigest;
        #endregion

        #region Properties
        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionDigest; } }

        /// <inheritdoc/>
        protected override string Usage { get { return "(DIRECTORY | ARCHIVE [SUBDIR])"; } }

        /// <inheritdoc/>
        public override string ActionTitle { get { return Resources.ActionDigest; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public Digest(Resolver resolver) : base(resolver)
        {
            Options.Add("manifest", () => Resources.OptionManifest, unused => _printManifest = true);
            Options.Add("digest", () => Resources.OptionDigest, unused => _printDigest = true);
            Options.Add("algorithm=", () => Resources.OptionAlgorithm + "\n" + SupportedValues(ManifestFormat.All),
                delegate(string algorithm)
                {
                    try
                    {
                        _algorithm = ManifestFormat.FromPrefix(algorithm);
                    }
                        #region Error handling
                    catch (ArgumentException ex)
                    {
                        // Wrap exception since only certain exception types are allowed
                        throw new OptionException(ex.Message, "algorithm");
                    }
                    #endregion
                });
        }
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        public override int Execute()
        {
            Resolver.Handler.ShowProgressUI();

            var manifest = GenerateManifest(
                AdditionalArgs[0],
                (AdditionalArgs.Count == 2) ? AdditionalArgs[1] : null);

            Resolver.Handler.Output("Manifest digest", GetOutput(manifest));
            return 0;
        }
        #endregion

        #region Helpers
        private Manifest GenerateManifest(string path, string subdir)
        {
            if (Directory.Exists(path))
            {
                if (!string.IsNullOrEmpty(subdir)) throw new OptionException(Resources.TooManyArguments, "");

                return Manifest.Generate(path, _algorithm, Resolver.Handler);
            }
            else if (File.Exists(path))
            {
                using (var tempDir = new TemporaryDirectory("0install"))
                {
                    using (var extractor = Extractor.CreateExtractor(path, Archive.GuessMimeType(path), tempDir))
                    {
                        extractor.SubDir = subdir;
                        Resolver.Handler.RunTask(extractor);
                    }

                    return Manifest.Generate(tempDir, _algorithm, Resolver.Handler);
                }
            }
            else throw new FileNotFoundException(string.Format(Resources.FileOrDirNotFound, path));
        }

        private string GetOutput(Manifest manifest)
        {
            if (_printManifest)
            {
                string result = manifest.ToString().TrimEnd('\n');
                if (_printDigest) result += "\n" + manifest.CalculateDigest();
                return result;
            }
            else return manifest.CalculateDigest();
        }
        #endregion
    }
}
