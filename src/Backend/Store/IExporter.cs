using JetBrains.Annotations;
using NanoByte.Common.Tasks;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Store
{
    /// <summary>
    /// Exports feeds and implementations.
    /// </summary>
    public interface IExporter
    {
        /// <summary>
        /// Exports all feeds listed in a <see cref="Selections"/> document along with any OpenPGP public key files required for validation.
        /// </summary>
        /// <param name="selections">A list of <see cref="ImplementationSelection"/>s to check for referenced feeds.</param>
        /// <param name="path">The path of the directory to export to.</param>
        void ExportFeeds([NotNull] Selections selections, [NotNull] string path);

        /// <summary>
        /// Exports all implementations listed in a <see cref="Selections"/> document as archives.
        /// </summary>
        /// <param name="selections">A list of <see cref="ImplementationSelection"/>s to export.</param>
        /// <param name="path">The path of the directory to export to.</param>
        /// <param name="handler">A callback object used when the the user needs to be informed about IO tasks.</param>
        void ExportImplementations([NotNull] Selections selections, [NotNull] string path, [NotNull] ITaskHandler handler);

        /// <summary>
        /// Deploys a bootstrap file for importing exported feeds and implementations.
        /// </summary>
        /// <param name="path">The path of the directory to deploy to.</param>
        void DeployBootstrap([NotNull] string path);
    }
}