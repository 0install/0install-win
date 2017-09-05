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
using JetBrains.Annotations;
using NanoByte.Common.Tasks;
using PackageManagement.Sdk;

namespace ZeroInstall.OneGet
{
    /// <summary>
    /// Represents a single progress bar provided by OneGet.
    /// </summary>
    public class OneGetProgress : MarshalByRefObject, IProgress<TaskSnapshot>
    {
        private readonly string _name;
        private readonly Request _request;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly int _activityId;

        public OneGetProgress([NotNull] string name, [NotNull] Request request, [NotNull] CancellationTokenSource cancellationTokenSource)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _request = request ?? throw new ArgumentNullException(nameof(request));
            _cancellationTokenSource = cancellationTokenSource ?? throw new ArgumentNullException(nameof(cancellationTokenSource));

            _request.Debug(name);
            _activityId = _request.StartProgress(0, name);
        }

        public void Report(TaskSnapshot value)
        {
            if (_request.IsCanceled) _cancellationTokenSource.Cancel();

            switch (value.State)
            {
                case TaskState.Canceled:
                case TaskState.IOError:
                case TaskState.WebError:
                    _request.CompleteProgress(_activityId, false);
                    break;

                case TaskState.Header:
                case TaskState.Data:
                    _request.Progress(_activityId, (int)(value.Value * 100), _name);
                    break;

                case TaskState.Complete:
                    _request.CompleteProgress(_activityId, true);
                    break;
            }
        }
    }
}
