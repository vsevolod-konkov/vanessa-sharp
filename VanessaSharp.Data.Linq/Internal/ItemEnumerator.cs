using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Data.Linq.Internal
{
    /// <summary>
    /// Перечислитель элементов используя чтение
    /// из <see cref="ISqlResultReader"/>.
    /// </summary>
    /// <typeparam name="T">Тип элементов.</typeparam>
    internal sealed class ItemEnumerator<T> : IEnumerator<T>
    {
        /// <summary>Читатель табличных данных из результата SQL-запроса.</summary>
        private readonly ISqlResultReader _sqlReader;

        /// <summary>Читатель элемента из записи таблицы.</summary>
        private readonly Func<ISqlResultReader, T> _itemReader;

        /// <summary>Конструктор.</summary>
        /// <param name="sqlReader">Читатель табличных данных из результата SQL-запроса.</param>
        /// <param name="itemReader">Читатель элемента из записи таблицы.</param>
        public ItemEnumerator(ISqlResultReader sqlReader, Func<ISqlResultReader, T> itemReader)
        {
            _sqlReader = sqlReader;
            _itemReader = itemReader;
        }

        [ContractInvariantMethod]
        private void Invariant()
        {
            Contract.Invariant(_sqlReader != null);
            Contract.Invariant(_itemReader != null);
        }

        /// <summary>
        /// Выполняет определяемые приложением задачи, связанные с удалением, высвобождением или сбросом неуправляемых ресурсов.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            _sqlReader.Dispose();
        }

        /// <summary>
        /// Перемещает перечислитель к следующему элементу коллекции.
        /// </summary>
        /// <returns>
        /// Значение true, если перечислитель был успешно перемещен к следующему элементу; значение false, если перечислитель достиг конца коллекции.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">Коллекция была изменена после создания перечислителя. </exception>
        /// <filterpriority>2</filterpriority>
        public bool MoveNext()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Устанавливает перечислитель в его начальное положение, перед первым элементом коллекции.
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException">Коллекция была изменена после создания перечислителя. </exception>
        /// <filterpriority>2</filterpriority>
        public void Reset()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Получает элемент коллекции, соответствующий текущей позиции перечислителя.
        /// </summary>
        /// <returns>
        /// Элемент коллекции, соответствующий текущей позиции перечислителя.
        /// </returns>
        public T Current { get; private set; }

        /// <summary>
        /// Получает текущий элемент в коллекции.
        /// </summary>
        /// <returns>
        /// Текущий элемент в коллекции.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">Перечислитель помещается перед первым элементом коллекции или после последнего элемента.</exception>
        /// <filterpriority>2</filterpriority>
        object IEnumerator.Current
        {
            get { return Current; }
        }
    }
}
