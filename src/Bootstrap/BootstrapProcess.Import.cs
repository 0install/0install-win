// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using NanoByte.Common.Streams;
using ZeroInstall.Archives.Extractors;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Store.FileSystem;
using ZeroInstall.Store.Implementations;

namespace ZeroInstall;

partial class BootstrapProcess
{
    /// <summary>
    /// Imports files from a directory into Zero Install.
    /// </summary>
    private void ImportDirectory(string directoryPath)
    {
        foreach (string path in Directory.GetFiles(directoryPath))
        {
            if (path.EndsWithIgnoreCase(".xml"))
            {
                Log.Info($"Importing feed from {path}");
                ImportFeed(path);
            }
            else if (path.EndsWithIgnoreCase(".png") || path.EndsWithIgnoreCase(".ico"))
            {
                Log.Info($"Importing icon from {path}");
                using var stream = File.OpenRead(path);
                ImportIcon(FeedUri.Unescape(Path.GetFileName(path)), stream);
            }
            else if (TryParseDigest(path) is {} manifestDigest)
            {
                Log.Info($"Importing implementation from {path}");
                var extractor = ArchiveExtractor.For(Archive.GuessMimeType(Path.GetFileName(path)), Handler);
                ImportImplementation(manifestDigest, builder => _handler.RunTask(new ReadFile(path, stream => extractor.Extract(builder, stream))));
            }
        }
    }

    private void ImportFeed(string path)
    {
        try
        {
            FeedManager.ImportFeed(path);
        }
        catch (ReplayAttackException) // Newer version already in cache
        {}
    }

    private void ImportIcon(FeedUri uri, Stream stream)
    {
        string iconsDirectory = _machineWide
            ? Locations.GetSaveSystemConfigPath("0install.net", isFile: false, "desktop-integration", "icons")
            : Locations.GetSaveConfigPath("0install.net", isFile: false, "desktop-integration", "icons");

        using var atomic = new AtomicWrite(Path.Combine(iconsDirectory, uri.Escape()));
        stream.CopyToFile(atomic.WritePath);
        atomic.Commit();
    }

    private static ManifestDigest? TryParseDigest(string path)
    {
        var manifestDigest = new ManifestDigest();
        manifestDigest.TryParse(Path.GetFileNameWithoutExtension(path));
        return manifestDigest.AvailableDigests.Any() ? manifestDigest : null;
    }

    private void ImportImplementation(ManifestDigest manifestDigest, Action<IBuilder> action)
    {
        try
        {
            ImplementationStore.Add(manifestDigest, action);
        }
        catch (ImplementationAlreadyInStoreException)
        {}
    }
}
