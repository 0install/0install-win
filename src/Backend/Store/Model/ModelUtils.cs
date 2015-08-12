using System;
using System.IO;
using JetBrains.Annotations;
using NanoByte.Common.Storage;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Provides utility methods for interface and feed URIs.
    /// </summary>
    public static class ModelUtils
    {
        /// <summary>
        /// Determines whether a string contains a template variable (a substring enclosed in curly brackets, e.g {var}).
        /// </summary>
        public static bool ContainsTemplateVariables([NotNull] string value)
        {
            #region Sanity checks
            if (value == null) throw new ArgumentNullException(nameof(value));
            #endregion

            int openingBracket = value.IndexOf('{');
            if (openingBracket == -1) return false;
            return (value.IndexOf('}', openingBracket) != -1);
        }

        /// <summary>
        /// Turns a relative path into an absolute one, using the file containing the reference as the base.
        /// </summary>
        /// <param name="path">The potentially relative path; will remain untouched if absolute.</param>
        /// <param name="source">The file containing the reference; can be <c>null</c>.</param>
        /// <returns>An absolute path.</returns>
        /// <exception cref="UriFormatException"><paramref name="path"/> is relative and <paramref name="source"/> is a remote URI.</exception>
        [NotNull]
        public static string GetAbsolutePath([NotNull] string path, [CanBeNull] FeedUri source = null)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
            #endregion

            if (Path.IsPathRooted(path)) return path;
            if (source == null || !source.IsFile) throw new UriFormatException(string.Format(Resources.RelativePathInRemoteFeed, path));
            return Path.Combine(Path.GetDirectoryName(source.LocalPath) ?? "", FileUtils.UnifySlashes(path));
        }

        /// <summary>
        /// Turns a relative path into an absolute one, using the file containing the reference as the base.
        /// </summary>
        /// <param name="path">The potentially relative path; will remain untouched if absolute.</param>
        /// <param name="source">The file containing the reference; can be <c>null</c>.</param>
        /// <returns>An absolute path.</returns>
        /// <exception cref="UriFormatException"><paramref name="path"/> is relative and <paramref name="source"/> is a remote URI.</exception>
        [NotNull]
        public static string GetAbsolutePath([NotNull] string path, [CanBeNull] string source)
        {
            return GetAbsolutePath(path, string.IsNullOrEmpty(source) ? null : new FeedUri(source));
        }

        /// <summary>
        /// Turns a relative HREF into an absolute one, using the file containing the reference as the base.
        /// </summary>
        /// <param name="href">The potentially relative HREF; will remain untouched if absolute.</param>
        /// <param name="source">The file containing the reference; can be <c>null</c>.</param>
        /// <returns>An absolute HREF.</returns>
        /// <exception cref="UriFormatException"><paramref name="href"/> is relative and <paramref name="source"/> is a remote URI.</exception>
        [NotNull]
        public static Uri GetAbsoluteHref([NotNull] Uri href, [CanBeNull] FeedUri source = null)
        {
            #region Sanity checks
            if (href == null) throw new ArgumentNullException(nameof(href));
            #endregion

            if (href.IsAbsoluteUri) return href;
            if (source == null || !source.IsFile) throw new UriFormatException(string.Format(Resources.RelativeUriInRemoteFeed, href));
            return new Uri(new Uri(source.LocalPath, UriKind.Absolute), href);
        }

        /// <summary>
        /// Turns a relative HREF into an absolute one, using the file containing the reference as the base.
        /// </summary>
        /// <param name="href">The potentially relative HREF; will remain untouched if absolute.</param>
        /// <param name="source">The file containing the reference; can be <c>null</c>.</param>
        /// <returns>An absolute HREF.</returns>
        /// <exception cref="UriFormatException"><paramref name="href"/> is relative and <paramref name="source"/> is a remote URI.</exception>
        [NotNull]
        public static Uri GetAbsoluteHref([NotNull] Uri href, [CanBeNull] string source)
        {
            return GetAbsoluteHref(href, string.IsNullOrEmpty(source) ? null : new FeedUri(source));
        }
    }
}
