/*
 * Copyright 2010-2014 Bastian Eicher
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

namespace ZeroInstall.Store
{
    /// <summary>
    /// Contains extension methods for <see cref="IInteractionHandler"/>s.
    /// </summary>
    public static class HandlerExtensions
    {
        /// <summary>
        /// Calls <see cref="IInteractionHandler.Output"/> only when <see cref="IInteractionHandler.Batch"/> is <see langword="false"/>.
        /// </summary>
        public static void OutputLow(this IInteractionHandler handler, string title, string message)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            if (!handler.Batch) handler.Output(title, message);
        }
    }
}
