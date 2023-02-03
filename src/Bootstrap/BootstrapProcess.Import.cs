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
    /// Imports files embedded as resources into Zero Install.
    /// </summary>
    /// <param name="prefix">The resource prefix/namespace of the files to import.</param>
    private void ImportEmbedded(string prefix)
    {
        var assembly = GetType().Assembly;
        var files = assembly.GetManifestResourceNames().Where(x => x.StartsWith(prefix)).Select(x => x[prefix.Length..]).ToList();
        if (files.Count == 0) return;

        void WriteToFile(string resourceName, string path)
        {
            using var stream = assembly.GetManifestResourceStream(prefix + resourceName)!;
            stream.CopyToFile(path);
        }

        // First populate a directory with all embedded GPG files, because they may be needed for feed imports to succeed in the next step
        using var feedDirectory = new TemporaryDirectory("0install-embedded-feeds");
        foreach (string name in files.Where(x => x.EndsWithIgnoreCase(".gpg")))
            WriteToFile(name, Path.Combine(feedDirectory, name));

        foreach (string name in files)
        {
            if (name.EndsWithIgnoreCase(".xml"))
            {
                Log.Info($"Importing embedded feed {name}");
                string path = Path.Combine(feedDirectory, FeedUri.Escape(name));
                WriteToFile(name, path);
                ImportFeed(path);
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
                ImportImplementation(manifestDigest, builder => Handler.RunTask(new SimpleTask("Extracting files", () =>
                {
                    using var stream = assembly.GetManifestResourceStream(prefix + name)!;
                    extractor.Extract(builder, stream);
                })));
            }
        }
    }

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
