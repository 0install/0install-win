// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using NanoByte.Common.Streams;
using ZeroInstall.Archives.Extractors;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Store.FileSystem;
using ZeroInstall.Store.Icons;
using ZeroInstall.Store.Implementations;

namespace ZeroInstall;

partial class BootstrapProcess
{
    /// <summary>
    /// Imports files embedded in the bootstrapper into Zero Install.
    /// </summary>
    private void ImportEmbedded()
    {
        const string prefix = "ZeroInstall.content.";
        var assembly = GetType().Assembly;

        foreach (string name in assembly.GetManifestResourceNames().Where(x => x.StartsWith(prefix)).Select(x => x[prefix.Length..]))
        {
            if (name.EndsWithIgnoreCase(".xml"))
            {
                Log.Info($"Importing embedded feed {name}");
                using var stream = assembly.GetManifestResourceStream(prefix + name)!;
                try
                {
                    FeedManager.ImportFeed(stream, keyCallback: id =>
                    {
                        Log.Info($"Reading embedded OpenPGP key {id}");
                        using var keyStream = assembly.GetManifestResourceStream(prefix + id + ".gpg");
                        return keyStream?.ReadAll();
                    });
                }
                catch (ReplayAttackException) // Newer version already in cache
                {}
            }
            else if (name.EndsWithIgnoreCase(".png") || name.EndsWithIgnoreCase(".ico"))
            {
                Log.Info($"Importing embedded icon {name}");
                using var stream = assembly.GetManifestResourceStream(prefix + name)!;
                ImportIcon(new FeedUri(name), stream);
            }
            else if (TryParseDigest(name) is {} manifestDigest)
            {
                Log.Info($"Importing embedded implementation {name}");
                var extractor = ArchiveExtractor.For(Archive.GuessMimeType(name), Handler);
                ImportImplementation(manifestDigest, builder =>
                {
                    using var stream = assembly.GetManifestResourceStream(prefix + name)!;
                    Handler.RunTask(new ReadStream($"Extracting embedded archive {name}", stream, x => extractor.Extract(builder, x)));
                });
            }
        }
    }

    /// <summary>
    /// Imports files from a directory into Zero Install.
    /// </summary>
    private void ImportDirectory()
    {
        string defaultContentDir = Path.Combine(Locations.InstallBase, "content");
        string? contentDir = _contentDir ?? (Directory.Exists(defaultContentDir) ? defaultContentDir : null);
        if (contentDir == null) return;

        foreach (string path in Directory.GetFiles(contentDir))
        {
            if (path.EndsWithIgnoreCase(".xml"))
            {
                Log.Info($"Importing feed from {path}");
                try
                {
                    FeedManager.ImportFeed(path);
                }
                catch (ReplayAttackException) // Newer version already in cache
                {}
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

    private void ImportIcon(FeedUri uri, Stream stream)
    {
        var store = new IconStore(_machineWide
            ? Locations.GetSaveSystemConfigPath("0install.net", isFile: false, "desktop-integration", "icons")
            : Locations.GetSaveConfigPath("0install.net", isFile: false, "desktop-integration", "icons"), Config, Handler);
        store.Import(new() {Href = uri}, stream);
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
