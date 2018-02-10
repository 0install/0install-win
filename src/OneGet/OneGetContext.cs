﻿/*
 * Copyright 2010-2016 Bastian Eicher
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
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using PackageManagement.Sdk;
using ZeroInstall.Commands;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Commands.Utils;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Services;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Preferences;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.OneGet
{
    /// <summary>
    /// Provides an execution context for handling a single OneGet <see cref="Request"/>.
    /// </summary>
    public sealed class OneGetContext : CommandBase, IOneGetContext
    {
        private readonly Request _request;

        /// <summary>
        /// Creates a new OneGet command.
        /// </summary>
        /// <param name="request">The OneGet request callback object.</param>
        public OneGetContext([NotNull] Request request)
            : base(new OneGetHandler(request)) => _request = request;

        /// <inheritdoc/>
        public void Dispose() => Handler.Dispose();

        private bool Refresh => _request.OptionKeys.Contains("Refresh");
        private bool AllVersions => _request.OptionKeys.Contains("AllVersions");
        private bool GlobalSearch => _request.OptionKeys.Contains("GlobalSearch");
        private bool DeferDownload => _request.OptionKeys.Contains("DeferDownload");
        private bool MachineWide => StringUtils.EqualsIgnoreCase(_request.GetOptionValue("Scope"), "AllUsers");

        public void AddPackageSource(string uri)
        {
            var feedUri = new FeedUri(uri);

            CatalogManager.DownloadCatalog(feedUri);

            if (CatalogManager.AddSource(feedUri))
                CatalogManager.GetOnlineSafe();
            else
                Log.Warn(string.Format(Resources.CatalogAlreadyRegistered, feedUri.ToStringRfc()));
        }

        public void RemovePackageSource(string uri)
        {
            var feedUri = new FeedUri(uri);

            if (!CatalogManager.RemoveSource(feedUri))
                Log.Warn(string.Format(Resources.CatalogNotRegistered, feedUri.ToStringRfc()));
        }

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

        public void FindPackage(string name, string requiredVersion, string minimumVersion, string maximumVersion)
        {
            FeedManager.Refresh = Refresh;

            VersionRange versionRange;
            if (requiredVersion != null) versionRange = new VersionRange(requiredVersion);
            else if (minimumVersion != null || maximumVersion != null)
            {
                var constraint = new Constraint();
                if (minimumVersion != null) constraint.NotBefore = new ImplementationVersion(minimumVersion);
                if (maximumVersion != null) constraint.Before = new ImplementationVersion(maximumVersion);
                versionRange = constraint;
            }
            else versionRange = null;

            if (GlobalSearch) MirrorSearch(name, versionRange);
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
                    foreach (var implementation in FeedManager.GetFresh(feed.Uri).Implementations)
                    {
                        var requirements = new Requirements(feed.Uri) {ExtraRestrictions = {{feed.Uri, implementation.Version}}};
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
                return GetCatalog().Feeds;
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
            Yield(new Requirements(GetCanonicalUri(identifier)));
        }

        public void GetInstalledPackages(string name)
        {
            var appList = AppList.LoadSafe(MachineWide);
            foreach (var entry in appList.Search(name))
                Yield(entry.EffectiveRequirements);
        }

        public void DownloadPackage(string fastPackageReference, string location)
        {
            var requirements = ParseReference(fastPackageReference);
            var selections = Solve(requirements);
            Fetcher.Fetch(SelectionsManager.GetUncachedImplementations(selections));

            var exporter = new Exporter(selections, requirements, location);
            exporter.ExportFeeds(FeedCache, OpenPgp);
            exporter.ExportImplementations(Store, Handler);
            exporter.DeployImportScript();
            exporter.DeployBootstrapIntegrate(Handler);

            Yield(requirements);

            SelfUpdateCheck();
        }

        public void InstallPackage(string fastPackageReference)
        {
            var requirements = ParseReference(fastPackageReference);
            try
            {
                Install(requirements);
                SelfUpdateCheck();
            }
            catch (UnsuitableInstallBaseException ex)
            {
                string installLocation = ProgramUtils.FindOtherInstance(ex.NeedsMachineWide);
                if (installLocation == null)
                {
                    if (Handler.Ask(Resources.AskDeployZeroInstall + Environment.NewLine + ex.Message,
                        defaultAnswer: true, alternateMessage: ex.Message))
                        installLocation = DeployInstance(ex.NeedsMachineWide);
                    else return;
                }

                // Since we cannot another copy of Zero Install from a different location into the same AppDomain, simply prentend we are running from a different source
                Locations.OverrideInstallBase(installLocation);
                Install(requirements);
            }
        }

        private void Install(Requirements requirements)
        {
            if (MachineWide && !WindowsUtils.IsAdministrator) throw new NotAdminException(Resources.MustBeAdminForMachineWide);
            if (MachineWide && ProgramUtils.IsRunningFromPerUserDir) throw new UnsuitableInstallBaseException(Resources.NoMachineWideIntegrationFromPerUser, MachineWide);
            if (ProgramUtils.IsRunningFromCache) throw new UnsuitableInstallBaseException(Resources.NoIntegrationFromCache, MachineWide);

            FeedManager.Refresh = Refresh || !DeferDownload;

            var selections = Solve(requirements);

            ApplyIntegration(requirements);
            ApplyVersionRestrictions(requirements, selections);
            if (!DeferDownload) Fetcher.Fetch(SelectionsManager.GetUncachedImplementations(selections));
            Yield(requirements);
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

            try
            {
                selections.Name = FeedCache.GetFeed(selections.InterfaceUri).Name;
            }
                #region Error handling
            catch (KeyNotFoundException)
            {
                // Fall back to using feed file name
                selections.Name = selections.InterfaceUri.ToString().GetRightPartAtLastOccurrence('/');
            }
            #endregion

            return selections;
        }

        private void ApplyIntegration(Requirements requirements)
        {
            Log.Info(Resources.DesktopIntegrationApply);
            var feed = FeedManager[requirements.InterfaceUri];
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
            Log.Warn($"You have applied a version restriction to this app. Zero Install will continue to apply this restriction to any future updates. You will need to run '0install select --customize {requirements.InterfaceUri}' to undo this.");

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

        /// <summary>
        /// Deploys a Zero Install instance to this machine.
        /// </summary>
        /// <param name="machineWide"><c>true</c> to deploy to a location for all users; <c>false</c> to deploy to a location for the current user only.</param>
        /// <returns>The director Zero Install was deployed to.</returns>
        private string DeployInstance(bool machineWide)
        {
            string programFiles = machineWide
                ? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Programs");
            string installLocation = Path.Combine(programFiles, "Zero Install");

            Log.Info("Deploying Zero Install to " + installLocation);
            using (var manager = new MaintenanceManager(installLocation, Handler, machineWide, portable: false))
                manager.Deploy();
            Log.Warn(Resources.Added0installToPath + Environment.NewLine + Resources.ReopenTerminal);

            return installLocation;
        }

        public void UninstallPackage(string fastPackageReference)
        {
            if (MachineWide && !WindowsUtils.IsAdministrator) throw new NotAdminException(Resources.MustBeAdminForMachineWide);

            var requirements = ParseReference(fastPackageReference);

            using (var integrationManager = new IntegrationManager(Handler, MachineWide))
                integrationManager.RemoveApp(integrationManager.AppList[requirements.InterfaceUri]);
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
                if (selections != null) implementation = selections.MainImplementation;
            }
            if (feed == null) feed = FeedManager[requirements.InterfaceUri];

            var sourceUri = feed.CatalogUri ?? feed.Uri;
            _request.YieldSoftwareIdentity(
                fastPath: requirements.ToJsonString(),
                name: feed.Name,
                version: implementation?.Version?.ToString(),
                versionScheme: null,
                summary: feed.Summaries.GetBestLanguage(CultureInfo.CurrentUICulture),
                source: sourceUri?.ToStringRfc(),
                searchKey: feed.Name,
                fullPath: null,
                packageFileName: feed.Name);
        }
    }
}
