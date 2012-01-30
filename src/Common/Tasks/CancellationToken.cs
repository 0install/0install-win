using System;

namespace Common.Tasks
{
    /// <summary>
    /// Propagates notification that operations should be canceled.
    /// </summary>
    public sealed class CancellationToken
    {
        private volatile bool _isCancellationRequested;

        /// <summary>
        /// Indicates whether <see cref="RequestCancellation"/> has been called.
        /// </summary>
        public bool IsCancellationRequested { get { return _isCancellationRequested; } }

        /// <summary>
        /// Raised the first time <see cref="RequestCancellation"/> is called. Subsequent calls will not raise this event again.
        /// </summary>
        public event SimpleEventHandler CancellationRequested;

        private readonly object _lock = new object();

        /// <summary>
        /// Notifies all listening entities that their operations should be canceled.
        /// </summary>
        public void RequestCancellation()
        {
            lock (_lock)
            {
                if (!IsCancellationRequested)
                {
                    _isCancellationRequested = true;
                    if (CancellationRequested != null) CancellationRequested();
                }
            }
        }

        /// <summary>
        /// Throws an <see cref="OperationCanceledException"/> if <see cref="RequestCancellation"/> has been called.
        /// </summary>
        /// <exception cref="OperationCanceledException">Thrown if <see cref="RequestCancellation"/> has been called.</exception>
        public void ThrowIfCancellationRequested()
        {
            if (_isCancellationRequested) throw new OperationCanceledException();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "CancellationToken {IsCancellationRequested=" + IsCancellationRequested + "}";
        }
    }
}
