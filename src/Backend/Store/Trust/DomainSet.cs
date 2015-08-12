using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;

namespace ZeroInstall.Store.Trust
{
    /// <summary>
    /// A set of alphabetically sorted <see cref="Domain"/>s.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "A Set is a specific type of Collection.")]
    [Serializable]
    public class DomainSet : SortedSet<Domain>
    {
        public DomainSet() : base(new DomainComparer())
        {}

        public Domain this[int index] => this.Skip(index).First();

        protected DomainSet(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}

        private class DomainComparer : IComparer<Domain>
        {
            public int Compare(Domain x, Domain y)
            {
                return string.Compare(x.Value, y.Value, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}