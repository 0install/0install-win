using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace Common.Tasks
{
    /// <summary>
    /// Propagates notification that operations should be canceled.<br/>
    /// Once a token has been signaled it remains in that state and cannot be reused.
    /// </summary>
    public sealed class CancellationToken
    {
        private volatile bool _isCancellationRequested; // Volatile justification: Write access is locked, many reads

        /// <summary>
        /// Indicates whether <see cref="RequestCancellation"/> has been called.
        /// </summary>
        public bool IsCancellationRequested { get { return _isCancellationRequested; } }

        /// <summary>
        /// Raised the first time <see cref="RequestCancellation"/> is called. Subsequent calls will not raise this event again.
        /// </summary>
        /// <remarks>
        ///   <para>This event is raised from a background thread. Wrap via <see cref="Control.Invoke(System.Delegate)"/> to update UI elements.</para>
        ///   <para>Handling this blocks the task, therefore observers should handle the event quickly.</para>
        /// </remarks>
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        public event Action CancellationRequested;

        private readonly object _lock = new object();

        /// <summary>
        /// Notifies all listening entities that their operations should be canceled.
        /// </summary>
        public void RequestCancellation()
        {
            lock (_lock)
            {
                if (_isCancellationRequested) return; // Don't trigger more than once
                _isCancellationRequested = true;
                if (CancellationRequested != null) CancellationRequested();
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
