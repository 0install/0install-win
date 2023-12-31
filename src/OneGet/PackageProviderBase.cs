// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Security;
using NanoByte.Common.Info;
using PackageManagement.Sdk;

namespace ZeroInstall.OneGet;

/// <summary>
/// Common base for OneGet package providers. Implements OneGet duck-typing interface.
/// </summary>
public abstract class PackageProviderBase
{
    public abstract string PackageProviderName { get; }

    public string ProviderVersion => AppInfo.CurrentLibrary.Version ?? "1.0.0-pre";

    public void OnUnhandledException(string methodName, Exception exception)
    {
        Log.Error($"Unexpected exception thrown in {PackageProviderName}::{methodName}", exception);
    }

    public void InitializeProvider(Request request) => request.Debug("Calling '{0}::InitializeProvider'", PackageProviderName);

    public void GetFeatures(Request request)
    {
        request.Debug("Calling '{0}::GetFeatures'", PackageProviderName);
        request.Yield(new Dictionary<string, string[]>
        {
            [Constants.Features.SupportedExtensions] = ["xml"],
            [Constants.Features.SupportedSchemes] = ["http", "https", "file"]
        });
    }

    public void GetDynamicOptions(string? category, Request request)
    {
        request.Debug("Calling '{0}::GetDynamicOptions'", PackageProviderName);
        switch ((category ?? "").ToLowerInvariant())
        {
            case "package":
                request.YieldDynamicOption("Refresh", Constants.OptionType.Switch, isRequired: false);
                request.YieldDynamicOption("AllVersions", Constants.OptionType.Switch, isRequired: false);
                request.YieldDynamicOption("GlobalSearch", Constants.OptionType.Switch, isRequired: false);
                break;

            case "install":
                request.YieldDynamicOption("Refresh", Constants.OptionType.Switch, isRequired: false);
                request.YieldDynamicOption("DeferDownload", Constants.OptionType.Switch, isRequired: false);
                request.YieldDynamicOption("Scope", Constants.OptionType.String, isRequired: false, permittedValues: new[] {"CurrentUser", "AllUsers"});
                break;

            case "source":
                break;
        }
    }

    /// <summary>
    /// Creates a <see cref="IOneGetContext"/> instance and executes a delegate on it, handling common exception types.
    /// </summary>
    private void Do(Request request, Action<IOneGetContext> action)
    {
        try
        {
            using var context = BuildContext(request);
            action(context);
        }
        #region Error handling
        catch (OperationCanceledException)
        {}
        catch (FormatException ex)
        {
            request.Error(ErrorCategory.InvalidArgument, "", ex.Message);
        }
        catch (WebException ex)
        {
            request.Error(ErrorCategory.ConnectionError, "", ex.Message);
        }
        catch (NotSupportedException ex)
        {
            request.Error(ErrorCategory.NotImplemented, "", ex.Message);
        }
        catch (IOException ex)
        {
            request.Error(ErrorCategory.OpenError, "", ex.Message);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or SecurityException)
        {
            request.Error(ErrorCategory.PermissionDenied, "", ex.Message);
        }
        catch (InvalidDataException ex)
        {
            request.Error(ErrorCategory.InvalidData, "", ex.Message);
        }
        catch (Exception ex)
        {
            request.Error(ErrorCategory.MetadataError, "", ex.Message);
        }
        #endregion
    }

    /// <summary>
    /// Creates a <see cref="IOneGetContext"/> for a given <see cref="Request"/>.
    /// </summary>
    protected abstract IOneGetContext BuildContext(Request request);

    public void ResolvePackageSources(Request request)
    {
        request.Debug("Calling '{0}::ResolvePackageSources'", PackageProviderName);
        Do(request, x => x.ResolvePackageSources());
    }

    public void AddPackageSource(string name, string location, bool trusted, Request request)
    {
        request.Debug("Calling '{0}::AddPackageSource'", PackageProviderName);
        if (string.IsNullOrEmpty(location))
        {
            request.Error(ErrorCategory.InvalidArgument, "location", "Location parameter missing");
            return;
        }
        Do(request, x => x.AddPackageSource(location));
    }

    public void RemovePackageSource(string name, Request request)
    {
        request.Debug("Calling '{0}::RemovePackageSource'", PackageProviderName);
        if (string.IsNullOrEmpty(name))
        {
            request.Error(ErrorCategory.InvalidArgument, "name", "Name parameter missing");
            return;
        }

        Do(request, x => x.RemovePackageSource(name));
    }

    public void FindPackage(string name, string requiredVersion, string minimumVersion, string maximumVersion, int id, Request request)
    {
        request.Debug("Calling '{0}::FindPackage'", PackageProviderName);
        Do(request, x => x.FindPackage(name, requiredVersion, minimumVersion, maximumVersion));
    }

    public void FindPackageByFile(string file, int id, Request request)
    {
        request.Debug("Calling '{0}::FindPackageByFile'", PackageProviderName);
        if (string.IsNullOrEmpty(file))
        {
            request.Error(ErrorCategory.InvalidArgument, "file", "File parameter missing");
            return;
        }
        Do(request, x => x.FindPackageBy(file));
    }

    public void FindPackageByUri(Uri uri, int id, Request request)
    {
        request.Debug("Calling '{0}::FindPackageByUri'", PackageProviderName);
        if (uri == null)
        {
            request.Error(ErrorCategory.InvalidArgument, "uri", "Uri parameter missing");
            return;
        }
        Do(request, x => x.FindPackageBy(uri.OriginalString));
    }

    public void DownloadPackage(string fastPackageReference, string location, Request request)
    {
        request.Debug("Calling '{0}::DownloadPackage'", PackageProviderName);
        if (string.IsNullOrEmpty(location))
        {
            request.Error(ErrorCategory.InvalidArgument, "location", "Location parameter missing");
            return;
        }
        Do(request, x => x.DownloadPackage(fastPackageReference, location));
    }

    public void InstallPackage(string fastPackageReference, Request request)
    {
        request.Debug("Calling '{0}::InstallPackage'", PackageProviderName);
        Do(request, x => x.InstallPackage(fastPackageReference));
    }

    public void UninstallPackage(string fastPackageReference, Request request)
    {
        request.Debug("Calling '{0}::UninstallPackage'", PackageProviderName);
        Do(request, x => x.UninstallPackage(fastPackageReference));
    }

    public void GetInstalledPackages(string name, string requiredVersion, string minimumVersion, string maximumVersion, Request request)
    {
        request.Debug("Calling '{0}::GetInstalledPackages'", PackageProviderName);
        Do(request, x => x.GetInstalledPackages(name));
    }

    public void GetPackageDetails(string fastPackageReference, Request request)
    {
        request.Debug("Calling '{0}::GetPackageDetails'", PackageProviderName);
        Do(request, x => x.GetPackageDetails(fastPackageReference));
    }
}