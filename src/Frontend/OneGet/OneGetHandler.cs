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
using System.Linq;
using NanoByte.Common;
using NanoByte.Common.Tasks;
using PackageManagement.Sdk;

namespace ZeroInstall.OneGet
{
    /// <summary>
    /// Manages communication between <see cref="ITask"/>s and a OneGet <see cref="Request"/>.
    /// </summary>
    public class OneGetHandler : MarshalNoTimeout, ITaskHandler
    {
        private readonly Request _request;

        public OneGetHandler(Request request)
        {
            _request = request;

            Log.Handler += LogHandler;
        }

        #region Log handler
        /// <summary>
        /// Outputs <see cref="Log"/> messages using the OneGet <see cref="Request"/> object.
        /// </summary>
        /// <param name="severity">The type/severity of the entry.</param>
        /// <param name="message">The message text of the entry.</param>
        private void LogHandler(LogSeverity severity, string message)
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
        #endregion

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        /// <inheritdoc/>
        public CancellationToken CancellationToken { get { return _cancellationTokenSource.Token; } }

        /// <inheritdoc/>
        public void RunTask(ITask task)
        {
            #region Sanity checks
            if (task == null) throw new ArgumentNullException("task");
            #endregion

            task.Run(CancellationToken, new OneGetProgress(task.Name, _request, _cancellationTokenSource));
        }

        /// <inheritdoc/>
        public Verbosity Verbosity { get { return _request.IsInteractive ? Verbosity.Normal : Verbosity.Batch; } set { } }

        /// <inheritdoc/>
        public bool Ask(string question)
        {
            return _request.AskPermission(question);
        }

        /// <inheritdoc/>
        public void Output(string title, string message)
        {
            _request.Message(message);
        }

        /// <inheritdoc/>
        public void Output<T>(string title, IEnumerable<T> data)
        {
            string message = StringUtils.Join(Environment.NewLine, data.Select(x => x.ToString()));
            Output(title, message);
        }

        #region Dispose
        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Log.Handler -= LogHandler;

                _cancellationTokenSource.Dispose();
            }
        }
        #endregion
    }
}
