// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.IO;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Storage;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Store.Implementations.Archives;
using ZeroInstall.Store.Implementations.Manifests;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Calculate the manifest digest of a directory or archive.
    /// </summary>
    public sealed class Digest : CliCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "digest";

        /// <inheritdoc/>
        public override string Description => Resources.DescriptionDigest;

        /// <inheritdoc/>
        public override string Usage => "(DIRECTORY | ARCHIVE [SUBDIR])";

        /// <inheritdoc/>
        protected override int AdditionalArgsMin => 1;

        /// <inheritdoc/>
        protected override int AdditionalArgsMax => 2;
        #endregion

        #region State
        /// <summary>The hashing algorithm used to generate the manifest.</summary>
        private ManifestFormat _algorithm = ManifestFormat.Sha1New;

        private bool _printManifest, _printDigest;

        /// <inheritdoc/>
        public Digest([NotNull] ICommandHandler handler)
            : base(handler)
        {
            Options.Add("manifest", () => Resources.OptionManifest, _ => _printManifest = true);
            Options.Add("digest", () => Resources.OptionDigest, _ => _printDigest = true);
            Options.Add("algorithm=", () => Resources.OptionAlgorithm + Environment.NewLine + SupportedValues(ManifestFormat.All),
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
                        throw new OptionException(ex.Message, algorithm);
                    }
                    #endregion
                });
        }
        #endregion

        /// <inheritdoc/>
        public override ExitCode Execute()
        {
            var manifest = GenerateManifest(
                AdditionalArgs[0],
                (AdditionalArgs.Count == 2) ? AdditionalArgs[1] : null);

            Handler.Output("Manifest digest", GetOutput(manifest));
            return ExitCode.OK;
        }

        #region Helpers
        private Manifest GenerateManifest(string path, string subdir)
        {
            if (Directory.Exists(path))
            {
                if (!string.IsNullOrEmpty(subdir)) throw new OptionException(Resources.TooManyArguments + Environment.NewLine + subdir.EscapeArgument(), null);

                var generator = new ManifestGenerator(path, _algorithm);
                Handler.RunTask(generator);
                return generator.Manifest;
            }
            else if (File.Exists(path))
            {
                using (var tempDir = new TemporaryDirectory("0install"))
                {
                    using (var extractor = ArchiveExtractor.Create(path, tempDir))
                    {
                        extractor.Extract = subdir;
                        Handler.RunTask(extractor);
                    }

                    var generator = new ManifestGenerator(tempDir, _algorithm);
                    Handler.RunTask(generator);
                    return generator.Manifest;
                }
            }
            else throw new FileNotFoundException(string.Format(Resources.FileOrDirNotFound, path));
        }

        private string GetOutput(Manifest manifest)
        {
            if (_printManifest)
            {
                string result = manifest.ToString();
                if (_printDigest) result += Environment.NewLine + manifest.CalculateDigest();
                return result;
            }
            else return manifest.CalculateDigest();
        }
        #endregion
    }
}
