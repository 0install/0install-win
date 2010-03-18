using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Common.Collections;

namespace Common
{
    /// <summary>
    /// An immutable class representing command-line arguments passed to an application.
    /// </summary>
    public class Arguments
    {
        #region Properties
        private readonly string _args;
        public override string ToString()
        {
            return _args;
        }

        private readonly ReadOnlyCollection<string> _files;
        /// <summary>
        /// A list of all file names in the arguments.
        /// </summary>
        public IList<string> Files { get { return _files; } }

        private readonly IDictionary<string, string> _commands;
        /// <summary>
        /// A list of all commands without leading slash or hyphen (and their options if any) in the arguments.
        /// </summary>
        public IDictionary<string, string> Commands { get { return _commands; } }
        #endregion

        #region Constructor

        #region Helper
        /// <returns><see langword="true"/> if <paramref name="value"/> starts with a slash or a hyphen.</returns>
        private static bool IsCommand(string value)
        {
            return value.StartsWith("/", StringComparison.Ordinal) || value.StartsWith("-", StringComparison.Ordinal);
        }
        #endregion

        /// <summary>
        /// Creates a new arguments instance based on the argument array from a Main method.
        /// </summary>
        /// <param name="args">The array of arguments.</param>
        public Arguments(string[] args)
        {
            #region Sanity checks
            if (args == null) throw new ArgumentNullException("args");
            #endregion

            _args = string.Concat(args);

            // Temp collections for building the lists
            var filesTemp = new List<string>(args.Length);
            var commandsTemp = new Dictionary<string, string>(args.Length);

            // Separate the arguments element-wise into categories
            for (int i = 0; i < args.Length; i++)
            {
                if (IsCommand(args[i]))
                {
                    // Is the next element of the argument another command or an option?
                    if (i + 1 < args.Length && !IsCommand(args[i + 1]))
                    { // Command with an option (remove leading slash or hypen)
                        commandsTemp.Add(args[i].Remove(0, 1), args[++i]);
                    }
                    else
                    { // Command without an option (remove leading slash or hypen)
                        commandsTemp.Add(args[i].Remove(0, 1), null);
                    }
                }
                else filesTemp.Add(args[i]);
            }

            // Make the collections immutable
            _files = new ReadOnlyCollection<string>(filesTemp);
            _commands = new Dictionary<string, string>(commandsTemp);
        }
        #endregion
    }
}
