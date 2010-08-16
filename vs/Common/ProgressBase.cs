using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;
using Common.Helpers;

namespace Common
{
    /// <summary>
    /// Provides deafult implementations of threading code for file-related <see cref="IProgress"/> classes.
    /// </summary>
    /// <remarks>
    /// Sub-classes must initalize the <see cref="Thread"/> member.
    /// Calling <see cref="Cancel"/> will directly terminate the thread code without giving it any cleanup time.
    /// </remarks>
    public abstract class ProgressBase : IProgress
    {
        #region Events
        public event ProgressEventHandler StateChanged;

        private void OnStateChanged()
        {
            // Copy to local variable to prevent threading issues
            ProgressEventHandler stateChanged = StateChanged;
            if (stateChanged != null) stateChanged(this);
        }

        public event ProgressEventHandler BytesReceivedChanged;

        private void OnBytesReceivedChanged()
        {
            // Copy to local variable to prevent threading issues
            ProgressEventHandler bytesReceivedChanged = BytesReceivedChanged;
            if (bytesReceivedChanged != null) bytesReceivedChanged(this);
        }
        #endregion

        #region Variables
        /// <summary>Synchronization handle to prevent race conditions with thread startup/shutdown or <see cref="State"/> switched.</summary>
        protected readonly object StateLock = new object();

        /// <summary>The background thread used for executing the task. Sub-classes must initalize this member.</summary>
        protected Thread Thread;
        #endregion

        #region Properties
        private ProgressState _state;
        public ProgressState State
        {
            get { return _state; } protected set { UpdateHelper.Do(ref _state, value, OnStateChanged); }
        }

        public string ErrorMessage { get; protected set; }

        private long _bytesReceived;
        public long BytesProcessed
        {
            get { return _bytesReceived; } protected set { UpdateHelper.Do(ref _bytesReceived, value, OnBytesReceivedChanged); }
        }

        public long BytesTotal { get; protected set; }

        public double Progress
        {
            get
            {
                switch (BytesTotal)
                {
                    case -1: return -1;
                    case 0: return 1;
                    default: return BytesProcessed / (double)BytesTotal;
                }
            }
        }

        /// <summary>
        /// The local path to save the file or directory to.
        /// </summary>
        [Description("The local path to save the file or directory to.")]
        public string Target { get; protected set; }
        #endregion

        //--------------------//
        
        #region User control
        public void Start()
        {
            lock (StateLock)
            {
                if (State != ProgressState.Ready) return;

                State = ProgressState.Started;
                Thread.Start();
            }
        }

        public void Cancel()
        {
            lock (StateLock)
            {
                if (State == ProgressState.Ready || State >= ProgressState.Complete) return;

                Thread.Abort();
                Thread.Join();
                
                lock (StateLock)
                {
                    // Reset the state so the task can be started again
                    State = ProgressState.Ready;
                }
            }
        }

        public void Join()
        {
            lock (StateLock)
            {
                if (Thread == null || !Thread.IsAlive) return;
            }

            Thread.Join();
        }

        public void RunSync()
        {
            Start();
            Join();

            lock (StateLock)
            {
                switch (State)
                {
                    case ProgressState.Complete:
                        return;
                    case ProgressState.WebError:
                        throw new WebException(ErrorMessage);
                    case ProgressState.IOError:
                        throw new IOException(ErrorMessage);
                    default:
                        throw new UserCancelException();
                }
            }
        }
        #endregion
    }
}
