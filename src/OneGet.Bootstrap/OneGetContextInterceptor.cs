// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using LinFu.DynamicProxy;
using NanoByte.Common;
using PackageManagement.Sdk;

namespace ZeroInstall.OneGet;

/// <summary>
/// Forwards requests to an <see cref="IOneGetContext"/> implementation from a deployed or cached Zero Install instance.
/// </summary>
public class OneGetContextInterceptor : IInterceptor
{
    private readonly Request _request;

    /// <summary>
    /// Creates a new <see cref="IOneGetContext"/> interceptor.
    /// </summary>
    /// <param name="request">The OneGet request interface to pass to the constructor of the target <see cref="IOneGetContext"/> implementation.</param>
    public OneGetContextInterceptor(Request request)
    {
        _request = request ?? throw new ArgumentNullException(nameof(request));
    }

    private object? _context;

    // NOTE: Static/shared lock, to avoid multiple deployments being started in parallel
    private static readonly object _initLock = new();

    public object Intercept(InvocationInfo info)
    {
        #region Sanity checks
        if (info == null) throw new ArgumentNullException(nameof(info));
        #endregion

        // Double-checked locking
        if (_context == null)
        {
            lock (_initLock)
            {
                _context ??= InitExternalContext();
            }
        }

        _request.Debug("Forwarding to other Zero Install instance: {0}", info.TargetMethod.Name);
        return DuckType(_context, info);
    }

    /// <summary>
    /// Initializes an <see cref="IOneGetContext"/> loaded from an external OneGet provider assembly.
    /// </summary>
    private object InitExternalContext()
    {
        string providerDirectory = GetProviderDirectory();
        _request.Verbose("Loading Zero Install OneGet provider from {0}", providerDirectory);

        var assembly = Assembly.LoadFrom(Path.Combine(providerDirectory, "ZeroInstall.OneGet.dll"));
        var requestType = assembly.GetType("PackageManagement.Sdk.Request", throwOnError: true);
        var requestProxy = new ProxyFactory().CreateProxy(requestType, new RequestInterceptor(_request));
        var contextType = assembly.GetType("ZeroInstall.OneGet.OneGetContext", throwOnError: true);
        return Activator.CreateInstance(contextType, requestProxy);
    }

    /// <summary>
    /// Gets the path of the directory to load the Zero Install OneGet provider assembly from.
    /// </summary>
    /// <returns>The full path of the directory containing the provider assembly.</returns>
    private string GetProviderDirectory()
    {
        using var handler = new OneGetHandler(_request);
        var bootstrap = new BootstrapProcess(handler, gui: false);
        var startInfo = bootstrap.ZeroInstallDeployed() ?? bootstrap.ZeroInstallCached();
        return Path.GetDirectoryName(startInfo.FileName)!;
    }

    /// <summary>
    /// Duck-types from <see cref="Request"/> to an external type with the same method signatures.
    /// </summary>
    private class RequestInterceptor : IInterceptor
    {
        private readonly Request _request;

        public RequestInterceptor(Request request)
        {
            _request = request;
        }

        public object Intercept(InvocationInfo info) => DuckType(_request, info);
    }

    /// <summary>
    /// Forwards a method invocation <paramref name="info"/> to a <paramref name="target"/> using duck-typing.
    /// </summary>
    private static object DuckType(object target, InvocationInfo info)
    {
        var method = target.GetType().GetMethod(
            info.TargetMethod.Name,
            info.TargetMethod.GetParameters().Select(x => x.ParameterType).ToArray());
        if (method == null) throw new InvalidOperationException("Unable to find suitable method for duck-typing: " + info.TargetMethod.Name);

        try
        {
            return method.Invoke(target, info.Arguments);
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            throw ex.InnerException.Rethrow();
        }
    }
}
