// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using ZeroInstall.Archives.Extractors;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Store.Implementations;

namespace ZeroInstall;

partial class BootstrapProcess
{
    /// <summary>A directory to search for feeds and archives to import.</summary>
    private string _contentDir = Path.Combine(Locations.InstallBase, "content");

    /// <summary>
    /// Imports bundled feeds and implementations/archives.
    /// </summary>
    private void ImportContent()
    {
        if (!Directory.Exists(_contentDir)) return;

        foreach (string path in Directory.GetFiles(_contentDir, "*.xml"))
        {
            Log.Info($"Importing feed from {path}");
            try
            {
                FeedManager.ImportFeed(path);
            }
            #region Error handling
            catch (ReplayAttackException)
            {
                Log.Info("Ignored feed because a newer version is already in cache");
            }
            #endregion
        }

        foreach (string path in Directory.GetFiles(_contentDir))
        {
            var manifestDigest = new ManifestDigest();
            manifestDigest.TryParse(Path.GetFileNameWithoutExtension(path));
            if (manifestDigest.AvailableDigests.Any())
            {
                Log.Info($"Importing implementation {manifestDigest.Best} from {path}");
                try
                {
                    var extractor = ArchiveExtractor.For(Archive.GuessMimeType(path), Handler);
                    ImplementationStore.Add(manifestDigest, builder => _handler.RunTask(new ReadFile(path, stream => extractor.Extract(builder, stream))));
                }
                #region Error handling
                catch (ImplementationAlreadyInStoreException)
                {}
                #endregion
            }
        }

        string iconsDir = _machineWide
            ? Locations.GetSaveSystemConfigPath("0install.net", isFile: false, "desktop-integration", "icons")
            : Locations.GetSaveConfigPath("0install.net", isFile: false, "desktop-integration", "icons");
        foreach (string path in Directory.GetFiles(_contentDir, "*.png").Concat(Directory.GetFiles(_contentDir, "*.ico")))
        {
            Log.Info($"Importing desktop integration icon from {path}");
            File.Copy(path, Path.Combine(iconsDir, Path.GetFileName(path)), overwrite: true);
        }
    }
}
