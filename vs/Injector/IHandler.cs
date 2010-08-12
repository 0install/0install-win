/*
 * Copyright 2010 Bastian Eicher
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

using ZeroInstall.DownloadBroker;
using ZeroInstall.Store.Feed;

namespace ZeroInstall.Injector
{
    /// <summary>
    /// Callback methods to be used when the the user needs to be asked any questions or informed about progress.
    /// </summary>
    /// <remarks>
    /// All callbacks are called from the original thread.
    /// Thread-safety messures are needed only if the process was started on a background thread and is intended to update a UI.
    /// </remarks>
    public interface IHandler : IFeedHandler, IFetchHandler
    {
    }
}
