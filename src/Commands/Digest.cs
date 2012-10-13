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
using Common.Storage;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Injector;
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
        public const string Name = "digest";
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
        public Digest(Policy policy) : base(policy)
        {
            Options.Add("manifest", Resources.OptionManifest, unused => _printManifest = true);
            Options.Add("digest", Resources.OptionDigest, unused => _printDigest = true);
            Options.Add("algorithm=", Resources.OptionAlgorithm + "\n" + SupportedValues(ManifestFormat.All),
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
                        throw new OptionException(ex.Message, "algorithm", ex);
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
            if (!IsParsed) throw new InvalidOperationException(Resources.NotParsed);

            string path;
            string subdir;
            switch (AdditionalArgs.Count)
            {
                case 0:
                    throw new OptionException(Resources.MissingArguments, "");

                case 1:
                    path = AdditionalArgs[0];
                    subdir = null;
                    break;

                case 2:
                    path = AdditionalArgs[0];
                    subdir = AdditionalArgs[1];
                    break;

                default:
                    throw new OptionException(Resources.TooManyArguments, "");
            }

            Policy.Handler.ShowProgressUI();

            Manifest manifest;
            if (Directory.Exists(path))
            { // Manifest for directory
                if (!string.IsNullOrEmpty(subdir)) throw new OptionException(Resources.TooManyArguments, "");

                manifest = Manifest.Generate(path, _algorithm, Policy.Handler, null);
            }
            else if (File.Exists(path))
            { // Manifest for archive
                using (var tempDir = new TemporaryDirectory("0install"))
                {
                    using (var extractor = Extractor.CreateExtractor(null, path, 0, tempDir.Path))
                    {
                        extractor.SubDir = subdir;
                        Policy.Handler.RunTask(extractor, null);
                    }

                    manifest = Manifest.Generate(tempDir.Path, _algorithm, Policy.Handler, null);
                }
            }
            else throw new FileNotFoundException(string.Format(Resources.FileOrDirNotFound, path));

            string output;
            if (_printManifest)
            {
                output = manifest.ToString().TrimEnd('\n');
                if (_printDigest) output += "\n" + manifest.CalculateDigest();
            }
            else output = manifest.CalculateDigest();
            Policy.Handler.Output("Manifest digest", output);
            return 0;
        }
        #endregion
    }
}
