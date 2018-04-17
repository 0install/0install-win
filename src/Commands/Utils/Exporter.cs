// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.IO;
using System.Linq;
using System.Web;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Native;
using NanoByte.Common.Net;
using NanoByte.Common.Streams;
using NanoByte.Common.Tasks;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Implementations.Archives;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Commands.Utils
{
    /// <summary>
    /// Exports feeds and implementations listed in a <see cref="Selections"/> document.
    /// </summary>
    public class Exporter
    {
        private readonly Selections _selections;
        private readonly Architecture _architecture;
        private readonly string _destination;

        /// <summary>
        /// Creates a new exporter.
        /// </summary>
        /// <param name="selections">A list of <see cref="ImplementationSelection"/>s to check for referenced feeds.</param>
        /// <param name="architecture">The target architecture to use for bootstrap binaries.</param>
        /// <param name="destination">The path of the directory to export to.</param>
        public Exporter([NotNull] Selections selections, Architecture architecture, [NotNull] string destination)
        {
            _selections = selections ?? throw new ArgumentNullException(nameof(selections));
            _architecture = architecture;
            _destination = destination ?? throw new ArgumentNullException(nameof(destination));

            Directory.CreateDirectory(_destination);
        }

        /// <summary>
        /// Creates a new exporter.
        /// </summary>
        /// <param name="selections">A list of <see cref="ImplementationSelection"/>s to check for referenced feeds.</param>
        /// <param name="requirements">The <see cref="Requirements"/> used to generate the <see cref="Selections"/>.</param>
        /// <param name="destination">The path of the directory to export to.</param>
        /// <exception cref="IOException">The directory <paramref name="destination"/> could not be createdor.</exception>
        /// <exception cref="UnauthorizedAccessException">Creating the directory <paramref name="destination"/> is not permitted.</exception>
        public Exporter([NotNull] Selections selections, [NotNull] Requirements requirements, [NotNull] string destination)
            : this(selections, requirements.GetNormalizedAlternatives().First().Architecture, destination)
        {}

        /// <summary>
        /// Exports all feeds listed in a <see cref="Selections"/> document along with any OpenPGP public key files required for validation.
        /// </summary>
        /// <param name="feedCache">Used to get local feed files.</param>
        /// <param name="openPgp">Used to get export keys feeds were signed with.</param>
        /// <exception cref="UnauthorizedAccessException">The file could not be read or written.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the directory is not permitted.</exception>
        /// <exception cref="IOException">A feed or GnuPG could not be read from the cache.</exception>
        public void ExportFeeds([NotNull] IFeedCache feedCache, [NotNull] IOpenPgp openPgp)
        {
            #region Sanity checks
            if (feedCache == null) throw new ArgumentNullException(nameof(feedCache));
            if (openPgp == null) throw new ArgumentNullException(nameof(openPgp));
            #endregion

            string contentDir = Path.Combine(_destination, "content");
            Directory.CreateDirectory(contentDir);

            var feedUris = _selections.Implementations.Select(x => x.FromFeed ?? x.InterfaceUri).Distinct().ToList();

            foreach (var feedUri in feedUris)
            {
                string filePath = Path.Combine(contentDir, feedUri.PrettyEscape());
                if (!filePath.EndsWith(".xml")) filePath += ".xml";

                Log.Info("Exporting feed " + feedUri.ToStringRfc());
                File.Copy(feedCache.GetPath(feedUri), filePath, overwrite: true);
            }

            foreach (var signature in feedUris.SelectMany(feedCache.GetSignatures).OfType<ValidSignature>().Distinct())
            {
                Log.Info("Exporting GPG key " + signature.FormatKeyID());
                openPgp.DeployPublicKey(signature, contentDir);
            }
        }

        /// <summary>
        /// Exports all implementations listed in a <see cref="Selections"/> document as archives.
        /// </summary>
        /// <param name="store">Used to get cached implementations.</param>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about download and IO tasks.</param>
        /// <exception cref="OperationCanceledException">The user canceled the task.</exception>
        /// <exception cref="UnauthorizedAccessException">The file could not be read or written.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the directory is not permitted.</exception>
        /// <exception cref="IOException">An implementation archive could not be creates.</exception>
        public void ExportImplementations([NotNull] IStore store, [NotNull] ITaskHandler handler)
        {
            #region Sanity checks
            if (store == null) throw new ArgumentNullException(nameof(store));
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            #endregion

            string contentDir = Path.Combine(_destination, "content");
            Directory.CreateDirectory(contentDir);

            foreach (var digest in _selections.Implementations.Select(x => x.ManifestDigest).Where(x => x.Best != null).Distinct())
            {
                string sourcePath = store.GetPath(digest);
                if (sourcePath == null)
                {
                    Log.Warn("Implementation " + digest + " missing from cache");
                    continue;
                }

                using (var generator = ArchiveGenerator.Create(sourcePath, Path.Combine(contentDir, digest.Best + ".tbz2"), Archive.MimeTypeTarBzip))
                    handler.RunTask(generator);
            }
        }

        /// <summary>
        /// Deploys a bootstrap file for importing exported feeds and implementations.
        /// </summary>
        public void DeployImportScript()
        {
            string fileName = (_architecture.OS == OS.Windows) ? "import.cmd" : "import.sh";
            string target = Path.Combine(_destination, fileName);

            typeof(Exporter).CopyEmbeddedToFile(fileName, target);
            if (UnixUtils.IsUnix)
                UnixUtils.SetExecutable(target, executable: true);
        }

        /// <summary>
        /// Deploys a bootstrap file for importing exported feeds and implementations.
        /// </summary>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about download and IO tasks.</param>
        public void DeployBootstrapRun([NotNull] ITaskHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            #endregion

            DeployBootstrap(handler, mode: "run");
        }

        /// <summary>
        /// Deploys a bootstrap file for importing exported feeds and implementations.
        /// </summary>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about download and IO tasks.</param>
        public void DeployBootstrapIntegrate([NotNull] ITaskHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            #endregion

            DeployBootstrap(handler, mode: "integrate");
        }

        private void DeployBootstrap([NotNull] ITaskHandler handler, [NotNull] string mode)
        {
            string appName = _selections.Name ?? "application";
            string fileName = (_architecture.OS == OS.Windows)
                ? mode + " " + appName + ".exe"
                : mode + "-" + appName.ToLowerInvariant().Replace(" ", "-") + ".sh";

            var source = new Uri("https://0install.de/bootstrap/" +
                                 "?platform=" + (_architecture.OS == OS.Windows ? "windows" : "linux") +
                                 "&mode=" + mode +
                                 "&name=" + HttpUtility.UrlEncode(appName) +
                                 "&uri=" + HttpUtility.UrlEncode(_selections.InterfaceUri.ToStringRfc()));
            string target = Path.Combine(_destination, fileName);

            handler.RunTask(new DownloadFile(source, target));
            if (UnixUtils.IsUnix)
                UnixUtils.SetExecutable(target, executable: true);
        }
    }
}
