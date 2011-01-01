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

using System;
using Common;
using Common.Cli;

namespace ZeroInstall.Launcher.Cli
{
    /// <summary>
    /// Uses the stderr stream to inform the user of progress and ask questions.
    /// </summary>
    public class CliHandler : Store.Implementation.CliHandler, IHandler
    {
        /// <inheritdoc />
        public bool AcceptNewKey(string information)
        {
            if (Batch) return false;

            Log.Info(information);

            // Loop until the user has made a valid choice
            while (true)
            {
                switch ((CliUtils.ReadString("Trust [Y/N] ") ?? "n").ToLower())
                {
                    case "y":
                    case "yes":
                        return true;
                    case "n":
                    case "no":
                        return false;
                }
            }
        }

        /// <inheritdoc />
        public void RunDownloadTask(ITask task)
        {
            #region Sanity checks
            if (task == null) throw new ArgumentNullException("task");
            #endregion

            if (Batch) return;

            Log.Info(task.Name + "...");
            using (new TrackingProgressBar(task))
                task.RunSync();
        }
    }
}
