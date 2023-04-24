// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Reflection;
using System.Text;
using IniParser;
using IniParser.Exceptions;
using IniParser.Model;
using NanoByte.Common.Streams;
using ZeroInstall.Store.Configuration;

namespace ZeroInstall;

/// <summary>
/// Represents configuration bundled with the bootstrapper.
/// This can be used to create a customized bootstrapper that uses Zero Install to run or integrate another application.
/// </summary>
/// <param name="IniData">The raw INI data parsed from the config file.</param>
/// <param name="KeyFingerprint">The GnuPG key fingerprint to trust for signing <see cref="Config.SelfUpdateUri"/> or <paramref cref="AppUri"/>.</param>
/// <param name="AppUri">The feed URI of the target application to bootstrap.</param>
/// <param name="AppName">The name of the target application to bootstrap.</param>
/// <param name="AppArgs">Additional command-line arguments to pass to the application.</param>
/// <param name="IntegrateArgs">Command-line arguments to pass to <c>0install integrate</c>. <c>null</c> to not call <c>0install integrate</c> at all.</param>
/// <param name="CatalogUri">The URI of the catalog to replace the default catalog. Only applies if Zero Install is not already deployed.</param>
/// <param name="CustomizablePath">Offer the user to choose a custom path for storing implementations.</param>
public record BootstrapConfig(IniData IniData, string? KeyFingerprint, FeedUri? AppUri, string? AppName, string? AppArgs, string? IntegrateArgs, FeedUri? CatalogUri, bool CustomizablePath)
{
    /// <summary>
    /// Provides a single instance of the <see cref="BootstrapConfig"/> loaded from an embedded or bundled file.
    /// </summary>
    /// <exception cref="IOException">A problem occurred while reading the file.</exception>
    /// <exception cref="UnauthorizedAccessException">Read access to the file is not permitted.</exception>
    /// <exception cref="InvalidDataException">The file contains invalid config values.</exception>
    public static BootstrapConfig Instance => _instance.Value;

    private static readonly Lazy<BootstrapConfig> _instance = new(Load, LazyThreadSafetyMode.PublicationOnly);

    private static BootstrapConfig Load()
    {
        using var stream = GetStream();

        try
        {
            var iniData = new StreamIniDataParser().ReadData(new(stream, Encoding.UTF8));

            string? GetOption(string key)
                => iniData.TryGetKey("bootstrap" + iniData.SectionKeySeparator + key, out string value)
                && !string.IsNullOrEmpty(value)
                && !value.StartsWith(";")
                    ? value
                    : null;

            return new(
                iniData,
                KeyFingerprint: GetOption("key_fingerprint"),
                AppUri: GetOption("app_uri")?.To(x => new FeedUri(x)),
                AppName: GetOption("app_name"),
                AppArgs: GetOption("app_args"),
                IntegrateArgs: GetOption("integrate_args"),
                CatalogUri: GetOption("catalog_uri")?.To(x => new FeedUri(x)),
                CustomizablePath: string.Equals(GetOption("customizable_path"), "true", StringComparison.OrdinalIgnoreCase)
            );
        }
        #region Error handling
        catch (Exception ex) when (ex is ParsingException or FormatException)
        {
            // Wrap exception to add context information
            throw new InvalidDataException($"There was a problem parsing the bootstrap config.", ex);
        }
        #endregion
    }

    private static Stream GetStream()
    {
        string bundledPath = Assembly.GetEntryAssembly()!.Location[..^4] + ".ini";
        if (File.Exists(bundledPath))
        {
            Log.Debug($"Loading bootstrap config from file: {bundledPath}");
            return File.OpenRead(bundledPath);
        }
        else
        {
            Log.Debug($"Loading embedded bootstrap config");
            return typeof(BootstrapProcess).GetEmbeddedStream("config.ini");
        }
    }
}
