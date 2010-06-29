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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Common.Helpers;
using ZeroInstall.Injector.Properties;
using ZeroInstall.Model;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Injector
{
    /// <summary>
    /// Executes a set of <see cref="IDImplementation"/>s as a program.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    public class Launcher
    {
        #region Variables
        /// <summary>The <see cref="IDImplementation"/> to be launched, followed by all its dependencies.</summary>
        private readonly Selections _selections;

        /// <summary>Used to locate the selected <see cref="IDImplementation"/>s.</summary>
        private readonly IStore _store;
        #endregion

        #region Properties
        /// <summary>
        /// An alternative executable to to run from the main <see cref="IDImplementation"/> instead of <see cref="ImplementationBase.Main"/>.
        /// </summary>
        public string Main { get; set; }

        /// <summary>
        /// Instead of executing the selected program directly, pass it as an argument to this program. Useful for debuggers.
        /// </summary>
        public string Wrapper { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new launcher from <see cref="Selections"/>.
        /// </summary>
        /// <param name="selections">The <see cref="IDImplementation"/> to be launched, followed by all its dependencies.</param>
        /// <param name="store">Used to locate the selected <see cref="IDImplementation"/>s.</param>
        public Launcher(Selections selections, IStore store)
        {
            #region Sanity checks
            if (selections == null) throw new ArgumentNullException("selections");
            if (store == null) throw new ArgumentNullException("store");
            if (selections.Implementations.IsEmpty) throw new ArgumentException(Resources.NoImplementationsPassed, "selections");
            #endregion

            _selections = selections.CloneSelections();
            _store = store;
        }
        #endregion

        //--------------------//

        #region Helpers
        /// <summary>
        /// Locates an <see cref="IDImplementation"/> on the disk (usually in a <see cref="Store"/>).
        /// </summary>
        /// <param name="implementation">The <see cref="IDImplementation"/> to be located.</param>
        /// <returns>A fully qualified path pointing to the implementation's location on the local disk.</returns>
        /// <exception cref="ImplementationNotFoundException">Thrown if the <paramref name="implementation"/> is not cached yet.</exception>
        private string GetImplementationPath(IDImplementation implementation)
        {
            return (string.IsNullOrEmpty(implementation.LocalPath) ? _store.GetPath(implementation.ManifestDigest) : implementation.LocalPath);
        }
        #endregion

        #region Bindings
        /// <summary>
        /// Applies <see cref="Binding"/>s allowing the launched application to locate selected <see cref="IDImplementation"/>s.
        /// </summary>
        /// <param name="startInfo">The applications environment to apply the  <see cref="Binding"/>s to.</param>
        /// <param name="bindingContainer">The list of <see cref="Binding"/>s to be performed.</param>
        /// <param name="implementation">The <see cref="IDImplementation"/> to be made locatable via the <see cref="Binding"/>s.</param>
        private void ApplyBindings(ProcessStartInfo startInfo, IBindingContainer bindingContainer, IDImplementation implementation)
        {
            string implementationDirectory = GetImplementationPath(implementation);

            #region EnvironmentBinding
            foreach (var binding in bindingContainer.EnvironmentBindings)
            {
                var environmentVariables = startInfo.EnvironmentVariables;
                string environmentValue = Path.Combine(implementationDirectory, StringHelper.UnifySlashes(binding.Value));

                if (!environmentVariables.ContainsKey(binding.Name)) environmentVariables.Add(binding.Name, binding.Default);

                switch (binding.Mode)
                {
                    case EnvironmentMode.Prepend:
                        environmentVariables[binding.Name] = environmentValue + Path.PathSeparator + environmentVariables[binding.Name];
                        break;
                    case EnvironmentMode.Append:
                        environmentVariables[binding.Name] += Path.PathSeparator + environmentValue;
                        break;
                    case EnvironmentMode.Replace:
                        environmentVariables[binding.Name] = environmentValue;
                        break;
                }
            }
            #endregion

            // ToDo: Implement OverlayBindings
        }
        #endregion

        #region Prepare Process
        /// <summary>
        /// Prepares a <see cref="ProcessStartInfo"/> for executing the application as specified by the <see cref="Selections"/>.
        /// </summary>
        /// <param name="arguments">Arguments to be passed to the launched applications.</param>
        /// <returns>The <see cref="ProcessStartInfo"/> that can be used to create a new <see cref="Process"/>.</returns>
        /// <exception cref="ImplementationNotFoundException">Thrown if one of the <see cref="IDImplementation"/>s is not cached yet.</exception>
        public ProcessStartInfo Prepare(string arguments)
        {
            // Get the implementation to be launched
            IDImplementation startupImplementation = _selections.Implementations.First;

            // Apply the user-override for the Main exectuable if set
            string startupMain = (string.IsNullOrEmpty(Main) ? startupImplementation.Main : Main);

            // Prepare the new process to launch the implementation
            var startInfo = new ProcessStartInfo(Path.Combine(GetImplementationPath(startupImplementation), StringHelper.UnifySlashes(startupMain)), arguments)
            { ErrorDialog = true, UseShellExecute = false };

            // Apply user-given wrapper application if set
            if (!string.IsNullOrEmpty(Wrapper))
            {
                startInfo.Arguments = startInfo.FileName + " " + startInfo.Arguments;
                startInfo.FileName = Wrapper;
            }

            #region Bindings
            foreach (var implementation in _selections.Implementations)
            {
                // Apply bindings implementations use to find themselves
                ApplyBindings(startInfo, implementation, implementation);

                // Apply bindings implementations use to find their dependencies
                foreach (var dependency in implementation.Dependencies)
                    ApplyBindings(startInfo, dependency, _selections.GetSelection(dependency.Interface));
            }
            #endregion

            return startInfo;
        }
        #endregion

        #region Run
        /// <summary>
        /// Launches the application as specified by the <see cref="Selections"/> and returns when it has finished executing.
        /// </summary>
        /// <param name="arguments">Arguments to be passed to the launched applications.</param>
        /// <exception cref="ImplementationNotFoundException">Thrown if one of the <see cref="IDImplementation"/>s is not cached yet.</exception>
        public void RunSync(string arguments)
        {
            var process = new Process {StartInfo = Prepare(arguments)};
            process.Start();
            process.WaitForExit();
        }
        #endregion
    }
}
