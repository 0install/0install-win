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
using System.Diagnostics.CodeAnalysis;
using NanoByte.Common;
using NanoByte.Common.Cli;
using NanoByte.Common.Tasks;

namespace ZeroInstall.Store
{
    /// <summary>
    /// Uses the stdin/stderr streams to inform the user about tasks being run, ask the user questions and output results.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "Diamond inheritance structure leads to false positive.")]
    public class CliServiceHandler : CliTaskHandler, IServiceHandler
    {
        /// <inheritdoc/>
        public bool Batch { get; set; }

        /// <inheritdoc/>
        public bool AskQuestion(string question, string batchInformation = null)
        {
            if (Batch)
            {
                if (!string.IsNullOrEmpty(batchInformation)) Log.Warn(batchInformation);
                return false;
            }

            Log.Info(question);

            // Loop until the user has made a valid choice
            while (true)
            {
                string input = CliUtils.ReadString("[Y/N]");
                if (input == null) throw new OperationCanceledException();
                switch (input.ToLower())
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

        /// <inheritdoc/>
        public void Output(string title, string information)
        {
            Console.WriteLine(information);
        }
    }
}
