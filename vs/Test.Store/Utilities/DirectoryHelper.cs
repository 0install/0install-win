using System;
using System.Collections.Generic;
using System.Text;

namespace ZeroInstall.Store.Utilities
{
    public static class DirectoryHelper
    {
        /// <summary>
        /// Finds a free folder or file name by optionally appending a number
        /// to a path string.
        /// </summary>
        /// <returns>the modified preferred path</returns>
        public static string FindInexistantPath(string preferredPath)
        {
            if (!System.IO.Directory.Exists(preferredPath))
                return preferredPath;

            int suffix = 1;
            while (System.IO.Directory.Exists(preferredPath + suffix))
            {
                preferredPath = preferredPath + "_";
                ++suffix;
            }
            return preferredPath + suffix;
        }
    }
}
