using System;
using System.Collections.Generic;
using System.Globalization;

namespace ZeroInstall.Model
{
    /// <summary>
    /// This compares two <see cref="CultureInfo"/>s by alphabetically comparing their string representations.
    /// </summary>
    internal class CultureComparer : IComparer<CultureInfo>
    {
        public int Compare(CultureInfo x, CultureInfo y)
        {
            #region Sanity checks
            if (x == null) throw new ArgumentNullException("x");
            if (y == null) throw new ArgumentNullException("y");
            #endregion

            return x.ToString().CompareTo(y.ToString());
        }
    }
}
