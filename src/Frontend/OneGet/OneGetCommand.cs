/*
 * Copyright 2010-2015 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 *
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Streams;
using PackageManagement.Sdk;
using ZeroInstall.Commands;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Preferences;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.OneGet
{
    /// <summary>
    /// Provides functionality for implementing a OneGet provider. Used as a backend by <see cref="PackageProvider"/>. Once instance per <see cref="Request"/>.
    /// </summary>
    public sealed class OneGetCommand : CommandBase, IDisposable
    {
        private readonly Request _request;

        /// <summary>
        /// Creates a new OneGet command.
        /// </summary>
        /// <param name="request">The OneGet request callback object.</param>
        public OneGetCommand([NotNull] Request request) : base(new OneGetHandler(request))
        {
            _request = request;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Handler.Dispose();
        }

        #region Options
        private const string
            RefreshKey = "Refresh",
            MachineWideKey = "MachineWide",
            DownloadLaterKey = "DownloadLater",
            SearchMirrorKey = "SearchMirror",
            AllVersionsKey = "AllVersions";

        public void GetDynamicOptions(string category)
        {
            switch ((category ?? string.Empty).ToLowerInvariant())
            {
                case "package":
                    _request.YieldDynamicOption(RefreshKey, Constants.OptionType.Switch, false);
                    _request.YieldDynamicOption(SearchMirrorKey, Constants.OptionType.Switch, false);
                    break;

                case "source":
                    break;

                case "install":
                    _request.YieldDynamicOption(RefreshKey, Constants.OptionType.Switch, false);
                    _request.YieldDynamicOption(MachineWideKey, Constants.OptionType.Switch, false);
                    _request.YieldDynamicOption(DownloadLaterKey, Constants.OptionType.Switch, false);
                    break;
            }
        }

        private bool Refresh { get { return _request.OptionKeys.Contains(RefreshKey); } }

        private bool DownloadLater { get { return _request.OptionKeys.Contains(DownloadLaterKey); } }

        private bool MachineWide { get { return _request.OptionKeys.Contains(MachineWideKey); } }

        private bool SearchMirror { get { return _request.OptionKeys.Contains(SearchMirrorKey); } }

        private bool AllVersions { get { return _request.OptionKeys.Contains(AllVersionsKey); } }
        #endregion

        public void ResolvePackageSources()
        {
            var registerdSources = Services.Feeds.CatalogManager.GetSources();

            if (_request.Sources.Any())
            {
                foreach (var uri in _request.Sources.TrySelect<string, FeedUri, UriFormatException>(x => new FeedUri(x)))
                {
                    bool isRegistered = registerdSources.Contains(uri);
                    _request.YieldPackageSource(uri.ToStringRfc(), uri.ToStringRfc(), isTrusted: isRegistered, isRegistered: isRegistered, isValidated: false);
                }
            }
            else
            {
                foreach (var uri in registerdSources)
                    _request.YieldPackageSource(uri.ToStringRfc(), uri.ToStringRfc(), isTrusted: true, isRegistered: true, isValidated: false);
            }
        }

        public void AddPackageSource([NotNull] FeedUri location)
        {
            CatalogManager.AddSource(location);
        }

        public void RemovePackageSource([NotNull] FeedUri location)
        {
            CatalogManager.RemoveSource(location);
        }

        public void FindPackage([CanBeNull] string name, [CanBeNull] ImplementationVersion requiredVersion, [CanBeNull] ImplementationVersion minimumVersion, [CanBeNull] ImplementationVersion maximumVersion)
        {
            FeedManager.Refresh = Refresh;

            VersionRange versionRange;
            if (requiredVersion != null) versionRange = new VersionRange(requiredVersion);
            else if (minimumVersion != null || maximumVersion != null) versionRange = new VersionRange(minimumVersion, maximumVersion);
            else versionRange = null;

            if (SearchMirror) MirrorSearch(name, versionRange);
            else CatalogSearch(name, versionRange);
        }

        private void MirrorSearch([CanBeNull] string name, [CanBeNull] VersionRange versionRange)
        {
            foreach (var result in SearchQuery.Perform(Config, name).Results)
            {
                var requirements = new Requirements(result.Uri);
                if (versionRange != null) requirements.ExtraRestrictions[requirements.InterfaceUri] = versionRange;
                Yield(requirements, result.ToPseudoFeed());
            }
        }

        private void CatalogSearch([CanBeNull] string name, [CanBeNull] VersionRange versionRange)
        {
            foreach (var feed in GetCatalogResults(name))
            {
                if (AllVersions)
                {
                    foreach (var implementation in FeedManager.GetFeedFresh(feed.Uri).Elements.OfType<Implementation>())
                    {
                        var requirements = new Requirements(feed.Uri) {ExtraRestrictions = {{feed.Uri, new VersionRange(implementation.Version)}}};
                        Yield(requirements, feed, implementation);
                    }
                }
                else
                {
                    var requirements = new Requirements(feed.Uri);
                    if (versionRange != null) requirements.ExtraRestrictions[requirements.InterfaceUri] = versionRange;
                    Yield(requirements, feed);
                }
            }
        }

        [NotNull, ItemNotNull]
        private IEnumerable<Feed> GetCatalogResults([CanBeNull] string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                Log.Info("Returning entire catalog");
                return (CatalogManager.GetCached() ?? CatalogManager.GetOnlineSafe()).Feeds;
            }

            Log.Info("Searching for short-name match in Catalog: " + query);
            var feed = FindByShortName(query);
            if (feed == null)
            {
                Log.Info("Searching for partial match in Catalog: " + query);
                return CatalogManager.GetCachedSafe().Search(query);
            }
            else return new[] {feed};
        }

        public void FindPackageBy(string identifier)
        {
            FeedManager.Refresh = Refresh;

            var uri = GetCanonicalUri(identifier);
            var requirements = new Requirements(uri);

            Yield(requirements);
        }

        public void DownloadPackage([NotNull] string fastPackageReference, [NotNull] string location)
        {
            Directory.CreateDirectory(location);
            typeof(OneGetCommand).WriteEmbeddedFile("import.bat", Path.Combine(location, "import.bat"));

            FeedCache = new DiskFeedCache(Path.Combine(location, "interfaces"), OpenPgp);
            Store = new DirectoryStore(Path.Combine(location, "implementations"), useWriteProtection: false);
            FeedManager.Refresh = true;

            var requirements = ParseReference(fastPackageReference);
            var selections = Solve(requirements);
            Fetcher.Fetch(SelectionsManager.GetUncachedImplementations(selections));

            SelfUpdateCheck();
        }

        public void InstallPackage([NotNull] string fastPackageReference)
        {
            FeedManager.Refresh = Refresh;

            var requirements = ParseReference(fastPackageReference);
            var selections = Solve(requirements);

            if (Config.NetworkUse == NetworkLevel.Full && !DownloadLater) Fetcher.Fetch(SelectionsManager.GetUncachedImplementations(selections));
            ApplyIntegration(requirements);
            ApplyVersionRestrictions(requirements, selections);

            SelfUpdateCheck();
        }

        private Selections Solve(Requirements requirements)
        {
            var selections = Solver.Solve(requirements);

            if (FeedManager.ShouldRefresh || SelectionsManager.GetUncachedSelections(selections).Any())
            {
                FeedManager.Stale = false;
                FeedManager.Refresh = true;
                selections = Solver.Solve(requirements);
                FeedManager.Refresh = false;
            }

            return selections;
        }

        private void ApplyIntegration(Requirements requirements)
        {
            Log.Info("Applying desktop integration");
            var feed = FeedManager.GetFeed(requirements.InterfaceUri);
            using (var integrationManager = new CategoryIntegrationManager(Handler, MachineWide))
            {
                var appEntry = integrationManager.AddApp(new FeedTarget(requirements.InterfaceUri, feed));
                integrationManager.AddAccessPointCategories(appEntry, feed, CategoryIntegrationManager.StandardCategories);
            }
        }

        private static void ApplyVersionRestrictions(Requirements requirements, Selections selections)
        {
            if (requirements.ExtraRestrictions.Count == 0) return;

            // TODO
            Log.Warn(string.Format("You have applied a version restriction to this app. Zero Install will continue to apply this restriction to any future updates. You will need to run '0install select --customize {0}' to undo this.", requirements.InterfaceUri));

            foreach (var restriction in requirements.ExtraRestrictions)
            {
                var selection = selections.GetImplementation(restriction.Key);
                if (selection != null)
                {
                    var pref = FeedPreferences.LoadForSafe(restriction.Key);
                    pref.Implementations.Clear();
                    pref[selection.ID].UserStability = Stability.Preferred;
                    pref.SaveFor(restriction.Key);
                }
            }
        }

        public void UninstallPackage([NotNull] string fastPackageReference)
        {
            var requirements = ParseReference(fastPackageReference);

            using (var integrationManager = new IntegrationManager(Handler, MachineWide))
                integrationManager.RemoveApp(integrationManager.AppList[requirements.InterfaceUri]);
        }

        public void GetInstalledPackages([CanBeNull] string name)
        {
            var appList = AppList.LoadSafe(MachineWide);
            foreach (var entry in appList.Search(name))
                Yield(entry.EffectiveRequirements);
        }

        public void GetPackageDetails(string fastPackageReference)
        {
            FeedManager.Refresh = true;

            var requirements = ParseReference(fastPackageReference);
            Yield(requirements);
        }

        private static Requirements ParseReference(string fastPackageReference)
        {
            return JsonStorage.FromJsonString<Requirements>(fastPackageReference);
        }

        private void Yield([NotNull] Requirements requirements, [CanBeNull] Feed feed = null, [CanBeNull] ImplementationBase implementation = null)
        {
            if (implementation == null)
            {
                var selections = Solver.TrySolve(requirements);
                if (selections != null) implementation = selections.Implementations[0];
            }
            if (feed == null) feed = FeedManager.GetFeed(requirements.InterfaceUri);

            var sourceUri = feed.CatalogUri ?? feed.Uri;
            _request.YieldSoftwareIdentity(
                fastPath: requirements.ToJsonString(),
                name: feed.Name,
                version: (implementation == null || implementation.Version == null) ? null : implementation.Version.ToString(),
                versionScheme: null,
                summary: feed.Summaries.GetBestLanguage(CultureInfo.CurrentUICulture),
                source: (sourceUri == null) ? null : sourceUri.ToStringRfc(),
                searchKey: feed.Name,
                fullPath: null,
                packageFileName: feed.Name);
        }
    }
}
