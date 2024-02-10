// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using NanoByte.Common.Native;
using PackageManagement.Sdk;
using ZeroInstall.Commands;
using ZeroInstall.Commands.Basic.Exporters;
using ZeroInstall.Commands.Desktop;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Model.Preferences;
using ZeroInstall.Model.Selection;
using ZeroInstall.Services;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.OneGet;

/// <summary>
/// Provides an execution context for handling a single OneGet <see cref="Request"/>.
/// </summary>
/// <param name="request">The OneGet request callback object.</param>
public sealed class OneGetContext(Request request) : ScopedOperation(new OneGetHandler(request)), IOneGetContext
{
    /// <inheritdoc/>
    public void Dispose() => Handler.Dispose();

    private bool Refresh => request.OptionKeys.Contains("Refresh");
    private bool AllVersions => request.OptionKeys.Contains("AllVersions");
    private bool GlobalSearch => request.OptionKeys.Contains("GlobalSearch");
    private bool DeferDownload => request.OptionKeys.Contains("DeferDownload");
    private bool MachineWide => StringUtils.EqualsIgnoreCase(request.GetOptionValue("Scope"), "AllUsers");

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
        var registeredSources = CatalogManager.GetSources();

        if (request.Sources.Any())
        {
            foreach (var uri in request.Sources.TrySelect(x => new FeedUri(x), (UriFormatException _) => {}))
            {
                bool isRegistered = registeredSources.Contains(uri);
                request.YieldPackageSource(uri.ToStringRfc(), uri.ToStringRfc(), isTrusted: isRegistered, isRegistered: isRegistered, isValidated: false);
            }
        }
        else
        {
            foreach (var uri in registeredSources)
                request.YieldPackageSource(uri.ToStringRfc(), uri.ToStringRfc(), isTrusted: true, isRegistered: true, isValidated: false);
        }
    }

    public void FindPackage(string? name, string? requiredVersion, string? minimumVersion, string? maximumVersion)
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

    private void MirrorSearch(string? name, VersionRange? versionRange)
    {
        foreach (var result in SearchResults.Query(Config, name))
        {
            var requirements = new Requirements(result.Uri);
            if (versionRange != null) requirements.ExtraRestrictions[requirements.InterfaceUri] = versionRange;
            Yield(requirements, result.ToPseudoFeed());
        }
    }

    private void CatalogSearch(string? name, VersionRange? versionRange)
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

    private IEnumerable<Feed> GetCatalogResults(string? query)
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
        else return [feed];
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
        foreach (var implementation in SelectionsManager.GetUncachedImplementations(selections))
            Fetcher.Fetch(implementation);

        var exporter = new Exporter(selections, requirements.Architecture, location);
        exporter.ExportFeeds(FeedCache, OpenPgp);
        exporter.ExportImplementations(ImplementationStore, Handler);
        if (FeedCache.GetFeed(requirements.InterfaceUri) is {} feed)
        {
            exporter.ExportIcons(
                [..feed.Icons, ..feed.SplashScreens],
                IconStores.DesktopIntegration(Config, Handler, machineWide: false));
        }
        exporter.DeployImportScript();
        exporter.DeployBootstrapIntegrate(Handler);

        Yield(requirements);

        BackgroundSelfUpdate();
    }

    public void InstallPackage(string fastPackageReference)
    {
        var requirements = ParseReference(fastPackageReference);
        try
        {
            Install(requirements);
            BackgroundSelfUpdate();
        }
        catch (UnsuitableInstallBaseException ex)
        {
            string installLocation = ZeroInstallDeployment.FindOther(ex.NeedsMachineWide)
                                  ?? DeployInstance(ex.NeedsMachineWide);

            // Since we cannot another copy of Zero Install from a different location into the same AppDomain, simply pretend we are running from a different source
            Locations.OverrideInstallBase(installLocation);
            Install(requirements);
        }
    }

    private void Install(Requirements requirements)
    {
        if (MachineWide && !WindowsUtils.IsAdministrator) throw new NotAdminException(Resources.MustBeAdminForMachineWide);
        if (MachineWide && !ZeroInstallInstance.IsMachineWide) throw new UnsuitableInstallBaseException(Resources.NoMachineWideIntegrationFromPerUser, MachineWide);
        if (!ZeroInstallInstance.IsDeployed) throw new UnsuitableInstallBaseException(Resources.NoIntegrationDeployRequired, MachineWide);

        FeedManager.Refresh = Refresh || !DeferDownload;

        var selections = Solve(requirements);

        ApplyIntegration(requirements);
        ApplyVersionRestrictions(requirements, selections);
        if (!DeferDownload)
        {
            foreach (var implementation in SelectionsManager.GetUncachedImplementations(selections))
                Fetcher.Fetch(implementation);
        }
        Yield(requirements);
    }

    private Selections Solve(Requirements requirements)
    {
        var selections = Solver.Solve(requirements);

        if (FeedManager.ShouldRefresh || SelectionsManager.GetUncached(selections.Implementations).Any())
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
        using var integrationManager = new CategoryIntegrationManager(Config, Handler, MachineWide);
        var appEntry = integrationManager.AddApp(new(requirements.InterfaceUri, feed));
        integrationManager.AddAccessPointCategories(appEntry, feed, CategoryIntegrationManager.StandardCategories);
    }

    private static void ApplyVersionRestrictions(Requirements requirements, Selections selections)
    {
        if (requirements.ExtraRestrictions.Count == 0) return;

        // TODO
        Log.Warn($"You have applied a version restriction to this app. Zero Install will continue to apply this restriction to any future updates. You will need to run '0install select --customize {requirements.InterfaceUri}' to undo this.");

        foreach (var (feedUri, _) in requirements.ExtraRestrictions)
        {
            var selection = selections.GetImplementation(feedUri);
            if (selection != null)
            {
                var pref = FeedPreferences.LoadForSafe(feedUri);
                pref.Implementations.Clear();
                pref[selection.ID].UserStability = Stability.Preferred;
                pref.SaveFor(feedUri);
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
            ? WindowsUtils.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
            : Path.Combine(WindowsUtils.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Programs");
        string installLocation = Path.Combine(programFiles, "Zero Install");

        Log.Info("Deploying Zero Install to " + installLocation);
        using (var manager = new SelfManager(installLocation, Handler, machineWide))
            manager.Deploy(libraryMode: true);

        return installLocation;
    }

    public void UninstallPackage(string fastPackageReference)
    {
        if (MachineWide && !WindowsUtils.IsAdministrator) throw new NotAdminException(Resources.MustBeAdminForMachineWide);

        var requirements = ParseReference(fastPackageReference);

        using var integrationManager = new IntegrationManager(Config, Handler, MachineWide);
        integrationManager.RemoveApp(integrationManager.AppList[requirements.InterfaceUri]);
    }

    public void GetPackageDetails(string fastPackageReference)
    {
        FeedManager.Refresh = true;

        var requirements = ParseReference(fastPackageReference);
        Yield(requirements);
    }

    private static Requirements ParseReference(string fastPackageReference) => JsonStorage.FromJsonString<Requirements>(fastPackageReference);

    private void Yield(Requirements requirements, Feed? feed = null, ImplementationBase? implementation = null)
    {
        EnsureAllowed(requirements.InterfaceUri);

        if (implementation == null)
        {
            var selections = Solver.TrySolve(requirements);
            if (selections != null) implementation = selections.MainImplementation;
        }
        feed ??= FeedManager[requirements.InterfaceUri];

        var sourceUri = feed.CatalogUri ?? feed.Uri;
        request.YieldSoftwareIdentity(
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
