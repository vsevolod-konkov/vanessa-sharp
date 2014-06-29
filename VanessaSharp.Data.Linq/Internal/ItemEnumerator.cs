using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;

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

        /// <summary>Буфер для чтения значений.</summary>
        private readonly object[] _buffer;

        /// <summary>Читатель элемента из записи таблицы.</summary>
        private readonly Func<object[], T> _itemReader;

        /// <summary>Состояния перечислителя.</summary>
        private enum EnumeratorState
        {
            Bof,
            Eof,
            Item,
        }

        /// <summary>Текущее состояние.</summary>
        private EnumeratorState _currentState = EnumeratorState.Bof;

        /// <summary>Конструктор.</summary>
        /// <param name="sqlReader">Читатель табличных данных из результата SQL-запроса.</param>
        /// <param name="itemReader">Читатель элемента из записи таблицы.</param>
        public ItemEnumerator(ISqlResultReader sqlReader, Func<object[], T> itemReader)
        {
            Contract.Requires<ArgumentNullException>(sqlReader != null);
            Contract.Requires<ArgumentNullException>(itemReader != null);

            _sqlReader = sqlReader;
            _buffer = new object[sqlReader.FieldCount];
            _itemReader = itemReader;
        }

        [ContractInvariantMethod]
        private void Invariant()
        {
            Contract.Invariant(_sqlReader != null);
            Contract.Invariant(_itemReader != null);
        }

        /// <summary>Читатель табличных данных из результата SQL-запроса.</summary>
        /// <remarks>Для проверки в тестах.</remarks>
        internal bool IsSameSqlResultReader(ISqlResultReader otherSqlReader)
        {
            return _sqlReader == otherSqlReader;
        }

        /// <summary>Читатель элемента из записи таблицы.</summary>
        /// <remarks>Для проверки в тестах.</remarks>
        internal Func<object[], T> ItemReader
        {
            get { return _itemReader; }
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
            if (_currentState == EnumeratorState.Eof)
                return false;

            var result = _sqlReader.Read();
            if (result)
            {
                _currentState = EnumeratorState.Item;
                _current = ReadItem();
            }
            else
            {
                _currentState = EnumeratorState.Eof;
            }

            return result;
        }

        /// <summary>Чтение элемента.</summary>
        private T ReadItem()
        {
            _sqlReader.GetValues(_buffer);

            return _itemReader(_buffer);
        }

        /// <summary>
        /// Устанавливает перечислитель в его начальное положение, перед первым элементом коллекции.
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException">Коллекция была изменена после создания перечислителя. </exception>
        /// <filterpriority>2</filterpriority>
        public void Reset()
        {
            throw new NotSupportedException(
                string.Format("Метод \"{0}\" не поддерживается.", MethodBase.GetCurrentMethod()));
        }

        /// <summary>
        /// Получает элемент коллекции, соответствующий текущей позиции перечислителя.
        /// </summary>
        /// <returns>
        /// Элемент коллекции, соответствующий текущей позиции перечислителя.
        /// </returns>
        public T Current
        {
            get
            {
                if (_currentState != EnumeratorState.Item)
                {
                    throw new InvalidOperationException(
                        "Недопустимое состояние перечислителя для получения значения свойства Current.");
                }

                return _current;
            }
        }
        private T _current;

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
