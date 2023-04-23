// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using NanoByte.Common.Streams;
using ZeroInstall.Store.Configuration;

namespace ZeroInstall;

/// <summary>
/// Represents configuration bundled with the bootstrapper.
/// This can be used to create a customized bootstrapper that uses Zero Install to run or integrate another application.
/// </summary>
/// <param name="KeyFingerprint">The GnuPG key fingerprint to trust for signing <see cref="Config.SelfUpdateUri"/> or <paramref cref="AppUri"/>.</param>
/// <param name="AppUri">The feed URI of the target application to bootstrap.</param>
/// <param name="AppName">The name of the target application to bootstrap.</param>
/// <param name="AppArgs">Additional command-line arguments to pass to the application.</param>
/// <param name="IntegrateArgs">Command-line arguments to pass to <c>0install integrate</c>. <c>null</c> or empty to not call <c>0install integrate</c> at all.</param>
/// <param name="CustomizablePath">Controls whether the user is offered to choose a custom path for storing implementations.</param>
public record BootstrapConfig(string? KeyFingerprint, FeedUri? AppUri, string? AppName, string? AppArgs, string? IntegrateArgs, bool CustomizablePath = false)
{
    public static BootstrapConfig Instance => _instance.Value;
    private static readonly Lazy<BootstrapConfig> _instance = new(Load, LazyThreadSafetyMode.PublicationOnly);

    private static BootstrapConfig Load()
    {
        string[] lines = typeof(BootstrapConfig).GetEmbeddedString("EmbeddedConfig.txt").SplitMultilineText();

        string? ReadConfig(string key, int lineNumber, string placeholder)
        {
            string line = lines[lineNumber].TrimEnd();
            if (!string.IsNullOrEmpty(line) && !(line.Contains(placeholder) && line.StartsWith("--") && line.EndsWith("--")))
            {
                Log.Info($"Bundled config: {key}: {line}");
                return line;
            }

            return null;
        }

        return new(
            KeyFingerprint: ReadConfig("key_fingerprint", lineNumber: 0, placeholder: nameof(KeyFingerprint)),
            AppUri: ReadConfig("app_uri", lineNumber: 1, nameof(AppUri))?.To(x => new FeedUri(x)),
            AppName: ReadConfig("app_name", lineNumber: 2, placeholder: nameof(AppName)),
            AppArgs: ReadConfig("app_args", lineNumber: 3, placeholder: nameof(AppArgs)),
            IntegrateArgs: ReadConfig("integrate_args", lineNumber: 4, placeholder: nameof(IntegrateArgs)),
            CustomizablePath: ReadConfig("customizable_path", lineNumber: 5, placeholder: nameof(CustomizablePath)) == "true"
        );
    }
}
