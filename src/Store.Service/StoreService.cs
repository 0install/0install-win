// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Collections;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Lifetime;
using System.Runtime.Serialization.Formatters;
using System.Security;
using System.ServiceProcess;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Service.Properties;

namespace ZeroInstall.Store.Service;

/// <summary>
/// Represents a Windows service.
/// </summary>
public sealed partial class StoreService : ServiceBase
{
    public StoreService()
    {
        InitializeComponent();
    }

    /// <summary>IPC channel for providing services to clients.</summary>
    private IChannelReceiver? _serverChannel;

    /// <summary>IPC channel for launching callbacks in clients.</summary>
    private IChannelSender? _clientChannel;

    /// <summary>The store to provide to clients as a service.</summary>
    private MarshalByRefObject? _store;

    /// <summary>The IPC remoting reference for <see cref="_store"/>.</summary>
    private ObjRef? _objRef;

    private const int IncorrectFunction = 1, AccessDenied = 5, InvalidHandle = 6, UnableToWriteToDevice = 29;

    public void Start() => OnStart(Array.Empty<string>());

    protected override void OnStart(string[] args)
    {
        try
        {
            if (!EventLog.SourceExists(eventLog.Source))
                EventLog.CreateEventSource(eventLog.Source, eventLog.Log);
            Log.Handler += LogHandler;
        }
        #region Sanity checks
        catch (Exception ex) when (ex is SecurityException or InvalidOperationException)
        {
            Log.Error("Failed to set up event source logging", ex);
        }
        #endregion

        if (Locations.IsPortable)
        {
            Log.Error(Resources.NoPortableMode);
            ExitCode = IncorrectFunction;
            Stop();
            return;
        }

        try
        {
            _serverChannel = new IpcServerChannel(
                new Hashtable
                {
                    ["name"] = ServiceImplementationStore.IpcPort,
                    ["portName"] = ServiceImplementationStore.IpcPort,
                    ["secure"] = true
                },
                new BinaryServerFormatterSinkProvider {TypeFilterLevel = TypeFilterLevel.Full}, // Allow deserialization of custom types
                ServiceImplementationStore.IpcAcl);
            _clientChannel = new IpcClientChannel(
                new Hashtable
                {
                    ["name"] = ServiceImplementationStore.IpcCallbackPort
                },
                new BinaryClientFormatterSinkProvider());

            ChannelServices.RegisterChannel(_serverChannel, ensureSecurity: false);
            ChannelServices.RegisterChannel(_clientChannel, ensureSecurity: false);
            _store = new CompositeImplementationSink(
                ImplementationStores.GetDirectories(serviceMode: true)
                                    .Select(path => new ImplementationSink(path))
                                    .ToList());
            _objRef = RemotingServices.Marshal(_store, nameof(IImplementationSink), typeof(IImplementationSink));

            (RemotingServices.GetLifetimeService(_store) as ILease)?.Renew(TimeSpan.FromDays(365));
        }
        #region Error handling
        catch (IOException ex)
        {
            Log.Error("Failed to open cache directory:" + Environment.NewLine + ex);
            ExitCode = UnableToWriteToDevice;
            Stop();
        }
        catch (UnauthorizedAccessException ex)
        {
            Log.Error("Failed to open cache directory:" + Environment.NewLine + ex);
            ExitCode = AccessDenied;
            Stop();
        }
        catch (RemotingException ex)
        {
            Log.Error("Failed to open IPC connection:" + Environment.NewLine + ex);
            ExitCode = InvalidHandle;
            Stop();
        }
        catch (SecurityException ex)
        {
            Log.Error("Failed to open IPC connection:" + Environment.NewLine + ex);
            ExitCode = InvalidHandle;
            Stop();
        }
        #endregion
    }

    protected override void OnStop()
    {
        if (_objRef != null) RemotingServices.Unmarshal(_objRef);
        try
        {
            ChannelServices.UnregisterChannel(_clientChannel);
            ChannelServices.UnregisterChannel(_serverChannel);
        }
        catch (RemotingException)
        {}

        _serverChannel = null;
        _clientChannel = null;

        Log.Handler -= LogHandler;
    }

    /// <summary>
    /// Writes <see cref="NanoByte.Common.Log"/> messages to the Windows Event Log.
    /// </summary>
    /// <param name="severity">The type/severity of the entry.</param>
    /// <param name="message">The message text of the entry.</param>
    /// <param name="exception">An optional exception associated with the entry.</param>
    private void LogHandler(LogSeverity severity, string message, Exception? exception)
    {
        var entryType = GetEntryType(severity);
        if (!entryType.HasValue) return;

        const int maxMessageLength = 16000;
        if (message.Length > maxMessageLength)
            message = message[..maxMessageLength] + Environment.NewLine + "[MESSAGE TRIMMED DUE TO LENGTH]";

        try
        {
            eventLog.WriteEntry(message, entryType.Value);
        }
        catch (Win32Exception)
        {}
        catch (InvalidOperationException)
        {}
    }

    private static EventLogEntryType? GetEntryType(LogSeverity severity)
        => severity switch
        {
            LogSeverity.Info => EventLogEntryType.Information,
            LogSeverity.Warn => EventLogEntryType.Warning,
            LogSeverity.Error => EventLogEntryType.Error,
            _ => null
        };
}
