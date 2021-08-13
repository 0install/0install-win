// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Threading;
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

        public OneGetProgress(string name, Request request, CancellationTokenSource cancellationTokenSource)
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
