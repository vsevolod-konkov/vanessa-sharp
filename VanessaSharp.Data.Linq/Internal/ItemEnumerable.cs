using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Data.Linq.Internal
{
    /// <summary>
    /// Реализация перечислителя <see cref="OneSDataRecord"/>.
    /// </summary>
    internal sealed class ItemEnumerable<T> : IEnumerable<T>
    {
        /// <summary>Конструктор.</summary>
        /// <param name="sqlResultReader">Читатель результата SQL-запроса.</param>
        /// <param name="itemReaderFactory">Фабрика создания читателя элемента.</param>
        public ItemEnumerable(ISqlResultReader sqlResultReader, IItemReaderFactory<T> itemReaderFactory)
        {
            Contract.Requires<ArgumentNullException>(sqlResultReader != null);
            Contract.Requires<ArgumentNullException>(itemReaderFactory != null);

            _sqlResultReader = sqlResultReader;
            _itemReaderFactory = itemReaderFactory;
        }

        /// <summary>Читатель результата SQL-запроса.</summary>
        internal ISqlResultReader SqlResultReader
        {
            get { return _sqlResultReader; }
        }
        private readonly ISqlResultReader _sqlResultReader;

        /// <summary>Фабрика создания читателя элемента.</summary>
        internal IItemReaderFactory<T> ItemReaderFactory
        {
            get { return _itemReaderFactory; }
        }
        private readonly IItemReaderFactory<T> _itemReaderFactory;

        /// <summary>
        /// Возвращает перечислитель, выполняющий итерацию в коллекции.
        /// </summary>
        /// <returns>
        /// Интерфейс <see cref="T:System.Collections.Generic.IEnumerator`1"/>, который может использоваться для перебора элементов коллекции.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<T> GetEnumerator()
        {
            return new ItemEnumerator<T>(
                _sqlResultReader,
                _itemReaderFactory.CreateItemReader(_sqlResultReader));
        }

        /// <summary>
        /// Возвращает перечислитель, который осуществляет перебор элементов коллекции.
        /// </summary>
        /// <returns>
        /// Объект <see cref="T:System.Collections.IEnumerator"/>, который может использоваться для перебора элементов коллекции.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
