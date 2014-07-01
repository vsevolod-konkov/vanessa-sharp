using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace VanessaSharp.Data.Linq.UnitTests.Utility
{
    /// <summary>
    /// Расширения для <see cref="IEnumerable{T}"/>.
    /// </summary>
    internal static class EnumerableExtensions
    {
        /// <summary>
        /// Метод расширения для преобразования последовательности в неизменяемую коллекцию.
        /// </summary>
        /// <typeparam name="T">Тип элементов.</typeparam>
        /// <param name="sequence">Последовательность элементов.</param>
        public static ReadOnlyCollection<T> ToReadOnly<T>(this IEnumerable<T> sequence)
        {
            Contract.Requires<ArgumentNullException>(sequence != null);

            return new ReadOnlyCollection<T>(sequence.ToArray());
        }
    }
}
