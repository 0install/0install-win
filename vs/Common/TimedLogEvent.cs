using System;
using System.Diagnostics;

namespace Common
{
    /// <summary>
    /// Structure that allows you to log timed execution blocks.
    /// </summary>
    /// <example>
    ///   <code>using(new LogEvent("Message")) {}</code>
    /// </example>
    public struct TimedLogEvent : IDisposable
    {
        #region Variables
        private readonly Stopwatch _timer;
        private readonly string _entry;
        #endregion

        #region Event control
        /// <summary>
        /// Starts a new log event.
        /// </summary>
        /// <param name="entry">The entry for the log file. Elapsed time will automatically be appended.</param>
        public TimedLogEvent(string entry)
        {
            _entry = entry;
            _timer = Stopwatch.StartNew();
        }

        /// <summary>
        /// Ends the log event.
        /// </summary>
        public void Dispose()
        {
            _timer.Stop();
            Log.Info(_entry + " => " + (float)_timer.Elapsed.TotalSeconds + "s");
        }
        #endregion
    }
}
