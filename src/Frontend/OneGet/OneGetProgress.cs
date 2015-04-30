using System;
using NanoByte.Common.Tasks;
using PackageManagement.Sdk;

namespace ZeroInstall.OneGet
{
    /// <summary>
    /// Represents a single progress bar provided by OneGet.
    /// </summary>
    public class OneGetProgress : MarshalByRefObject, NanoByte.Common.Tasks.IProgress<TaskSnapshot>
    {
        private readonly string _name;
        private readonly Request _request;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly int _activityId;

        public OneGetProgress(string name, Request request, CancellationTokenSource cancellationTokenSource)
        {
            _name = name;
            _request = request;
            _cancellationTokenSource = cancellationTokenSource;
            _activityId = _request.StartProgress(0, name);
        }

        public void Report(TaskSnapshot snapshot)
        {
            if (_request.IsCanceled) _cancellationTokenSource.Cancel();

            switch (snapshot.State)
            {
                case TaskState.Canceled:
                case TaskState.IOError:
                case TaskState.WebError:
                    _request.CompleteProgress(_activityId, false);
                    break;

                case TaskState.Header:
                case TaskState.Data:
                    _request.Progress(_activityId, (int)(snapshot.Value * 100), _name);
                    break;

                case TaskState.Complete:
                    _request.CompleteProgress(_activityId, true);
                    break;
            }
        }
    }
}
