using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Common.Properties;

namespace Common
{
    /// <summary>
    /// Provides access to operations provided by NanoGrid.
    /// </summary>
    /// <remarks>
    /// For details about NanoGrid see: http://www.nano-byte.de/info/nanogrid/
    /// </remarks>
    public static class NanoGrid
    {
        /// <summary>
        /// Launches an application via NanoGrid. Exceptions are handled and reported via message boxes.
        /// </summary>
        /// <param name="appName">The name of the application to launch via NanoGrid.</param>
        /// <param name="args">Arguments to be passed on to the launched application.</param>
        public static void Launch(string appName, string args)
        {
            try
            {
                string nanoGridCommand = string.Format(CultureInfo.InvariantCulture, "nanogrid:/launch/\"{0}:{1}\" /autoClose", appName, args);
                Process.Start(nanoGridCommand);
            }
            catch
            {
                Msg.Inform(null, Resources.MissingNanoGrid, MsgSeverity.Error);
            }
        }

        /// <summary>
        /// Uploads a file using NanoGrid.
        /// </summary>
        /// <param name="path">The path to the file to be uploaded.</param>
        public static void Upload(string path)
        {
            try
            {
                string nanoGridCommand = string.Format(CultureInfo.InvariantCulture, "nanogrid:/upload/\"{0}\" /autoClose", path);
                Process.Start(nanoGridCommand);
            }
            catch (Win32Exception)
            {
                Msg.Inform(null, Resources.MissingNanoGrid, MsgSeverity.Error);
            }
            catch (FileNotFoundException)
            {
                Msg.Inform(null, Resources.MissingNanoGrid, MsgSeverity.Error);
            }
        }
    }
}
