// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using PackageManagement.Sdk;

namespace ZeroInstall.OneGet;

/// <summary>
/// Manages communication between <see cref="ITask"/>s and a OneGet <see cref="Request"/>.
/// </summary>
public class OneGetHandler : CliTaskHandler
{
    private readonly Request _request;

    public OneGetHandler(Request request)
    {
        _request = request;

        Verbosity = _request.IsInteractive ? Verbosity.Normal : Verbosity.Batch;
    }

    /// <summary>
    /// Outputs <see cref="Log"/> messages using the OneGet <see cref="Request"/> object.
    /// </summary>
    /// <param name="severity">The type/severity of the entry.</param>
    /// <param name="message">The message text of the entry.</param>
    protected override void LogHandler(LogSeverity severity, string message)
    {
        switch (severity)
        {
            case LogSeverity.Debug:
                _request.Debug(message);
                break;
            case LogSeverity.Info:
                _request.Verbose(message);
                break;
            case LogSeverity.Warn:
            case LogSeverity.Error:
                _request.Warning(message);
                break;
        }
    }

    /// <inheritdoc/>
    public override void RunTask(ITask task)
    {
        #region Sanity checks
        if (task == null) throw new ArgumentNullException(nameof(task));
        #endregion

        task.Run(CancellationToken, CredentialProvider, new OneGetProgress(task.Name, _request, CancellationTokenSource));
    }

    /// <inheritdoc/>
    protected override bool AskInteractive(string question, bool defaultAnswer)
        => _request.OptionKeys.Contains("Force") || _request.ShouldContinue(question, "Zero Install");

    /// <inheritdoc/>
    public override void Error(Exception exception) => _request.Warning(exception.Message);
}
