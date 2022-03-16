// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Configuration;
using NanoByte.Common;
using NanoByte.Common.Streams;
using ZeroInstall.Model;

namespace ZeroInstall
{
    /// <summary>
    /// Represents configuration embedded into the executable itself.
    /// This is used to create a customized bootstrapper that uses Zero Install to run or integrate another application.
    /// </summary>
    /// <param name="SelfUpdateUri">The feed URI used to download and update Zero Install itself.</param>
    /// <param name="AppUri">The feed URI of the target application to bootstrap.</param>
    /// <param name="AppName">The name of the target application to bootstrap.</param>
    /// <param name="AppFingerprint">The GnuPG key fingerprint to trust for signing <paramref cref="AppUri"/>.</param>
    /// <param name="AppArgs">Additional command-line arguments to pass to the application.</param>
    /// <param name="IntegrateArgs">Command-line arguments to pass to <c>0install integrate</c>. <c>null</c> or empty to not call <c>0install integrate</c> at all.</param>
    public record EmbeddedConfig(string? SelfUpdateUri, FeedUri? AppUri, string? AppName, string? AppFingerprint, string? AppArgs, string? IntegrateArgs)
    {
        /// <summary>
        /// Loads the embedded configuration.
        /// </summary>
        public static EmbeddedConfig Load()
        {
            string[] lines = typeof(EmbeddedConfig).GetEmbeddedString("EmbeddedConfig.txt").SplitMultilineText();

            string? ReadConfig(string key, int lineNumber, string placeholder)
            {
                string setting = ConfigurationManager.AppSettings[key];
                if (!string.IsNullOrEmpty(setting))
                {
                    Log.Info($"AppSettings config: {key}: {setting}");
                    return setting;
                }

                string line = lines[lineNumber].TrimEnd();
                if (!string.IsNullOrEmpty(line) && !(line.Contains(placeholder) && line.StartsWith("--") && line.EndsWith("--")))
                {
                    Log.Info($"Embedded config: {key}: {line}");
                    return line;
                }

                return null;
            }

            return new(
                SelfUpdateUri: ReadConfig("self_update_uri", lineNumber: 0, placeholder: nameof(SelfUpdateUri)),
                AppUri: ReadConfig("app_uri", lineNumber: 1, nameof(AppUri))?.To(x => new FeedUri(x)),
                AppName: ReadConfig("app_name", lineNumber: 2, placeholder: nameof(AppName)),
                AppFingerprint: ReadConfig("app_fingerprint", lineNumber: 3, placeholder: nameof(AppFingerprint)),
                AppArgs: ReadConfig("app_args", lineNumber: 4, placeholder: nameof(AppArgs)),
                IntegrateArgs: ReadConfig("integrate_args", lineNumber: 5, placeholder: nameof(IntegrateArgs))
            );
        }
    }
}
