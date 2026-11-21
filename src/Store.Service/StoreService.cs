// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Diagnostics;
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

    /// <summary>Accepts implementations from clients via a named pipe.</summary>
    private StoreServiceServer? _server;

    private const int IncorrectFunction = 1, AccessDenied = 5, UnableToWriteToDevice = 29;

    public void Start() => OnStart([]);

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
            _server = new StoreServiceServer(ImplementationStores.GetDirectories(serviceMode: true));
            _server.Start();
        }
        #region Error handling
        catch (IOException ex)
        {
            Log.Error("Failed to open cache directory or named pipe:" + Environment.NewLine + ex);
            ExitCode = UnableToWriteToDevice;
            Stop();
        }
        catch (UnauthorizedAccessException ex)
        {
            Log.Error("Failed to open cache directory or named pipe:" + Environment.NewLine + ex);
            ExitCode = AccessDenied;
            Stop();
        }
        catch (SecurityException ex)
        {
            Log.Error("Failed to open cache directory or named pipe:" + Environment.NewLine + ex);
            ExitCode = AccessDenied;
            Stop();
        }
        #endregion
    }

    protected override void OnStop()
    {
        _server?.Dispose();
        _server = null;

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
