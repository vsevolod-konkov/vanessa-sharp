using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace VanessaSharp.Data.Utility
{
    /// <summary>Методы расширения для <see cref="IEnumerable{T}"/>.</summary>
    internal static class EnumerableExtensions
    {
        public static ReadOnlyCollection<T> ToReadOnly<T>(this IEnumerable<T> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);

            return new ReadOnlyCollection<T>(
                (source as IList<T>) ?? source.ToArray());
        }
    }
}
