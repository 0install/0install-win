using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ZeroInstall.Store.Trust
{
    /// <summary>
    /// A set of alphabetically sorted <see cref="Domain"/>s.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "A Set is a specific type of Collection.")]
    public class DomainSet : SortedSet<Domain>
    {
        public DomainSet() : base(new DomainComparer())
        {}

        public Domain this[int index]
        {
            get { return this.Skip(index).First(); }
        }

        private class DomainComparer : IComparer<Domain>
        {
            public int Compare(Domain x, Domain y)
            {
                return string.Compare(x.Value, y.Value, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}