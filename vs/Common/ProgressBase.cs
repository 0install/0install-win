using System;
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
        public long BytesReceived
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
                    default: return BytesReceived / (double)BytesTotal;
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
        /// <summary>
        /// Starts executing the download in a background thread.
        /// </summary>
        /// <remarks>Calling this on a not <see cref="ProgressState.Ready"/> thread will have no effect.</remarks>
        public void Start()
        {
            lock (StateLock)
            {
                if (State != ProgressState.Ready) return;

                State = ProgressState.Started;
                Thread.Start();
            }
        }

        /// <summary>
        /// Stops executing the download.
        /// </summary>
        /// <param name="keepPartial">Set to <see langword="true"/> to keep partially downloaded files; <see langword="false"/> to delete them.</param>
        /// <remarks>Calling this on a not running thread will have no effect.</remarks>
        /// <exception cref="InvalidOperationException">Thrown if called while a synchronous download is running (launched via <see cref="RunSync"/>).</exception>
        public void Cancel(bool keepPartial)
        {
            lock (StateLock)
            {
                if (State == ProgressState.Ready || State >= ProgressState.Complete) return;

                Thread.Abort();
                Thread.Join();
                
                lock (StateLock)
                {
                    State = ProgressState.Ready;

                    if (keepPartial) return;

                    // Clean up left-over files
                    try
                    {
                        if (File.Exists(Target)) File.Delete(Target);
                        if (Directory.Exists(Target)) Directory.Delete(Target, true);
                    }
                    #region Error handling
                    catch (IOException ex)
                    {
                        Log.Warn("Unable to delete: " + Target + " (" + ex.Message + ")");
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Log.Warn("Unable to delete: " + Target + " (" + ex.Message + ")");
                    }
                    #endregion
                }
            }
        }

        /// <summary>
        /// Blocks until the download is completed or terminated.
        /// </summary>
        /// <remarks>Calling this on a not running thread will return immediately.</remarks>
        /// <exception cref="InvalidOperationException">Thrown if called while a synchronous download is running (launched via <see cref="RunSync"/>).</exception>
        public void Join()
        {
            lock (StateLock)
            {
                if (Thread == null || !Thread.IsAlive) return;
            }

            Thread.Join();
        }

        /// <summary>
        /// Runs the download synchronously instead of executing it in a background thread.
        /// </summary>
        /// <exception cref="WebException">Thrown if the download ended with <see cref="ProgressState.WebError"/>.</exception>
        /// <exception cref="IOException">Thrown if the download ended with <see cref="ProgressState.IOError"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="State"/> is not <see cref="ProgressState.Ready"/>.</exception>
        /// <exception cref="UserCancelException">Thrown if <see cref="Cancel"/> was called from another thread.</exception>
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
