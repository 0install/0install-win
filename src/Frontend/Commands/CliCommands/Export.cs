/*
 * Copyright 2010-2016 Bastian Eicher
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
using JetBrains.Annotations;
using NanoByte.Common.Tasks;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Commands.Utils;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Exports all feeds and implementations required to launch the program specified by URI.
    /// </summary>
    public sealed class Export : Download
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "export";

        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionExport; } }

        /// <inheritdoc/>
        protected override string Usage { get { return "[OPTIONS] URI DIRECTORY"; } }

        /// <inheritdoc/>
        protected override int AdditionalArgsMin { get { return 2; } }

        /// <inheritdoc/>
        protected override int AdditionalArgsMax { get { return 2; } }
        #endregion

        #region State
        private bool _noImplementations;

        /// <inheritdoc/>
        public Export([NotNull] ICommandHandler handler) : base(handler)
        {
            //Options.Remove("xml");
            //Options.Remove("show");

            Options.Add("no-implementations", () => Resources.OptionNoImplementations, _ => _noImplementations = true);
        }
        #endregion

        /// <inheritdoc/>
        public override ExitCode Execute()
        {
            string outputPath;
            try
            {
                outputPath = Path.GetFullPath(AdditionalArgs[0]);
                Directory.CreateDirectory(outputPath);
            }
                #region Error handling
            catch (ArgumentException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new UriFormatException(ex.Message);
            }
            catch (NotSupportedException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new UriFormatException(ex.Message);
            }
            #endregion

            Solve();

            var exporter = new Exporter(Selections, outputPath);
            exporter.ExportFeeds(FeedCache, OpenPgp);
            if (!_noImplementations)
            {
                DownloadUncachedImplementations();
                exporter.ExportImplementations(Store, Handler);
            }
            exporter.DeployImportScript();

            SelfUpdateCheck();

            return ShowOutput();
        }

        protected override ExitCode ShowOutput()
        {
            Handler.OutputLow(Resources.ExportComplete, Resources.AllComponentsExported);

            return ExitCode.OK;
        }
    }
}
