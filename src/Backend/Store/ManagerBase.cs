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
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Tasks;

namespace ZeroInstall.Store
{
    /// <summary>
    /// Common base class for managers that need an <see cref="ITaskHandler"/> and <see cref="Mutex"/>-based locking.
    /// </summary>
    public abstract class ManagerBase : IDisposable
    {
        /// <summary>
        /// A callback object used when the the user needs to be asked questions or informed about download and IO tasks.
        /// </summary>
        protected readonly ITaskHandler Handler;

        /// <summary>
        /// Apply operations machine-wide instead of just for the current user.
        /// </summary>
        public bool MachineWide { get; private set; }

        /// <summary>
        /// Creates a new manager.
        /// </summary>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about download and IO tasks.</param>
        /// <param name="machineWide">Apply operations machine-wide instead of just for the current user.</param>
        protected ManagerBase([NotNull] ITaskHandler handler, bool machineWide = false)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            #endregion

            Handler = handler;
            MachineWide = machineWide;
        }

        /// <summary>
        /// The name of the cross-process mutex used by <see cref="AquireMutex"/>.
        /// </summary>
        protected abstract string MutexName { get; }

        private Mutex _mutex;

        /// <summary>
        /// Tries to aquire a mutex with the name <see cref="MutexName"/>. Call this at the end of your constructors.
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">Another process is already holding the mutex.</exception>
        protected void AquireMutex()
        {
            if (MachineWide)
            {
                var mutexSecurity = new MutexSecurity();
                mutexSecurity.AddAccessRule(new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow));
                bool createdNew;
                _mutex = new Mutex(false, @"Global\" + MutexName, out createdNew, mutexSecurity);
            }
            _mutex = new Mutex(false, MutexName);

            try
            {
                switch (WaitHandle.WaitAny(new[] {_mutex, Handler.CancellationToken.WaitHandle},
                    millisecondsTimeout: (Handler.Verbosity == Verbosity.Batch) ? 30000 : 1000, exitContext: false))
                {
                    case 0:
                        return;
                    case 1:
                        throw new OperationCanceledException();
                    default:
                    case WaitHandle.WaitTimeout:
                        throw new UnauthorizedAccessException("Another process is already holding the mutex " + MutexName + ".");
                }
            }
            catch (AbandonedMutexException ex)
            {
                // Abandoned mutexes also get owned, but indicate something may have gone wrong elsewhere
                Log.Warn(ex.Message);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        ~ManagerBase()
        {
            Dispose(false);
        }

        /// <summary>
        /// Releases the mutex.
        /// </summary>
        /// <param name="disposing"><c>true</c> if called manually and not by the garbage collector.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _mutex != null)
            {
                _mutex.ReleaseMutex();
                _mutex.Close();
            }
        }
    }
}
