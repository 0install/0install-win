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
    /// Imports files from a directory into Zero Install.
    /// </summary>
    private void ImportDirectory()
    {
        string defaultContentDir = Path.Combine(Locations.InstallBase, "content");
        string? contentDir = _contentDir ?? (Directory.Exists(defaultContentDir) ? defaultContentDir : null);
        if (contentDir == null) return;

        foreach (string path in Directory.GetFiles(contentDir))
        {
            using var stream = File.OpenRead(path);
            ImportFile(Path.GetFileName(path), stream, keyCallback: id =>
            {
                string keyPath = Path.Combine(Path.GetDirectoryName(path) ?? "", id + ".gpg");
                if (!File.Exists(keyPath)) return null;

                Log.Info($"Loading OpenPGP key {id} from {keyPath}");
                return new(File.ReadAllBytes(keyPath));
            });
        }
    }

    /// <summary>
    /// Imports files embedded in the bootstrapper into Zero Install.
    /// </summary>
    private void ImportEmbedded()
    {
        const string prefix = "ZeroInstall.content.";
        var assembly = GetType().Assembly;

        foreach (string name in assembly.GetManifestResourceNames().Where(x => x.StartsWith(prefix)).Select(x => x[prefix.Length..]))
        {
            using var stream = assembly.GetManifestResourceStream(prefix + name)!;
            ImportFile(name, stream, keyCallback: id =>
            {
                using var keyStream = assembly.GetManifestResourceStream(prefix + id + ".gpg");
                if (keyStream == null) return null;

                Log.Info($"Reading embedded OpenPGP key {id}");
                return keyStream.ReadAll();
            });
        }
    }

    /// <summary>
    /// Imports a file into Zero Install.
    /// </summary>
    /// <param name="name">The file path or resource name.</param>
    /// <param name="stream">The contents of the file.</param>
    /// <param name="keyCallback">Callback for reading a specific OpenPGP public key file.</param>
    private void ImportFile(string name, Stream stream, OpenPgpKeyCallback keyCallback)
    {
        if (name.EndsWithIgnoreCase(".xml"))
        {
            Log.Info($"Importing feed from {name}");
            try
            {
                FeedManager.ImportFeed(stream, keyCallback);
            }
            catch (ReplayAttackException) // Newer version already in cache
            {}
        }
        else if (name.EndsWithIgnoreCase(".png") || name.EndsWithIgnoreCase(".ico"))
        {
            var uri = Uri.IsWellFormedUriString(name, UriKind.Absolute) ? new FeedUri(name) : FeedUri.Unescape(name);
            Log.Info($"Importing icon {uri} from {name}");
            ImportIcon(uri, stream);
        }
        else if (name.EndsWithIgnoreCase(".exe") && name.Split(['_'], count: 2) is [var dir, var file])
        {
            Log.Info($"Importing stub EXE {file} from {name}");
            ImportStubExe(dir, file, stream);
        }
        else if (TryParseDigest(name) is {} manifestDigest)
        {
            Log.Info($"Importing implementation from {name}");
            var extractor = ArchiveExtractor.For(Archive.GuessMimeType(name), Handler);
            ImportImplementation(manifestDigest, builder => Handler.RunTask(new ReadStream($"Extracting archive {name}", stream, x => extractor.Extract(builder, x))));
        }
    }

    private void ImportIcon(FeedUri uri, Stream stream)
    {
        string[] resource = ["desktop-integration", "icons"];
        var store = new IconStore(_machineWide
                ? Locations.GetSaveSystemConfigPath("0install.net", isFile: false, resource)
                : Locations.GetSaveConfigPath("0install.net", isFile: false, resource),
            Config, Handler);

        store.Import(new() {Href = uri}, stream);
    }

    private void ImportStubExe(string dir, string file, Stream stream)
    {
        string[] resource = ["desktop-integration", "stubs", dir, file];
        string path = _machineWide
            ? Locations.GetSaveSystemConfigPath("0install.net", isFile: true, resource)
            : Locations.GetSaveConfigPath("0install.net", isFile: true, resource);

        try
        {
            if (File.Exists(path)) new FileInfo(path).IsReadOnly = false;
            stream.CopyToFile(path);
            new FileInfo(path).IsReadOnly = true; // Make read-only to prevent ZeroInstall.DesktopIntegration from overwriting with on-demand generated stub
        }
        #region Error handling
        catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException)
        {
            Log.Warn($"Failed to deploy '{file}' to '{path}'", ex);
        }
        #endregion
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
