/*
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
using System.Linq;
using NanoByte.Common;
using NanoByte.Common.Tasks;
using PackageManagement.Sdk;

namespace ZeroInstall.OneGet
{
    /// <summary>
    /// Manages communication between <see cref="ITask"/>s and a OneGet <see cref="Request"/>.
    /// </summary>
    public class OneGetHandler : CliTaskHandler
    {
        private readonly Request _request;

        public OneGetHandler(Request request)
        {
            _request = request;
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
        public override Verbosity Verbosity { get { return _request.IsInteractive ? Verbosity.Normal : Verbosity.Batch; } set { } }

        /// <inheritdoc/>
        protected override bool Ask(string question, MsgSeverity severity) => _request.OptionKeys.Contains("Force") || _request.ShouldContinue(question, "Zero Install");

        /// <inheritdoc/>
        public override void Error(Exception exception) => _request.Warning(exception.Message);
    }
}
