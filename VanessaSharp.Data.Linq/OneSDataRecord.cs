using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace VanessaSharp.Data.Linq
{
    /// <summary>
    /// Запись табличных данных 1С.
    /// </summary>
    public sealed class OneSDataRecord
    {
        /// <summary>Поля.</summary>
        private readonly ReadOnlyCollection<string> _fields;

        /// <summary>Значения.</summary>
        private readonly ReadOnlyCollection<OneSValue> _values;

        /// <summary>Конструктор.</summary>
        /// <param name="fields">Коллекция полей.</param>
        /// <param name="values">Коллекция значений.</param>
        internal OneSDataRecord(ReadOnlyCollection<string> fields, ReadOnlyCollection<OneSValue> values)
        {
            Contract.Requires<ArgumentNullException>(fields != null);
            Contract.Requires<ArgumentNullException>(values != null);
            Contract.Requires<ArgumentException>(values.Count == fields.Count, "Количество значений должно совпадать с количеством полей.");

            _fields = fields;
            _values = values;
        }
        
        /// <summary>
        /// Получение строкового значения колонки по имени колонки.
        /// </summary>
        /// <param name="columnName">Имя колонки.</param>
        public string GetString(string columnName)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(columnName));

            return (string)GetValue(columnName);
        }

        /// <summary>Получение строкового значения колонки по индексу.</summary>
        /// <param name="index">Индекс колонки.</param>
        public string GetString(int index)
        {
            Contract.Requires<ArgumentOutOfRangeException>(index >= 0 && index < Fields.Count);

            return (string)GetValue(index);
        }

        /// <summary>
        /// Получение значения <see cref="int"/> по имени колонки.
        /// </summary>
        /// <param name="columnName">Имя колонка.</param>
        public int GetInt32(string columnName)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(columnName));

            return (int)GetValue(columnName);
        }

        /// <summary>
        /// Получение значения <see cref="int"/> по индексу.
        /// </summary>
        /// <param name="index">Индекс колонки.</param>
        public int GetInt32(int index)
        {
            Contract.Requires<ArgumentOutOfRangeException>(index >= 0 && index < Fields.Count);

            return (int)GetValue(index);
        }

        /// <summary>
        /// Получение значения <see cref="double"/> по имени колонки.
        /// </summary>
        /// <param name="columnName">Имя колонка.</param>
        public double GetDouble(string columnName)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(columnName));

            return (double)GetValue(columnName);
        }

        /// <summary>
        /// Получение значения <see cref="double"/> по индексу.
        /// </summary>
        /// <param name="index">Индекс колонки.</param>
        public double GetDouble(int index)
        {
            Contract.Requires<ArgumentOutOfRangeException>(index >= 0 && index < Fields.Count);

            return (double)GetValue(index);
        }

        /// <summary>
        /// Получение значения <see cref="bool"/> по имени колонки.
        /// </summary>
        /// <param name="columnName">Имя колонка.</param>
        public bool GetBoolean(string columnName)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(columnName));

            return (bool)GetValue(columnName);
        }

        /// <summary>
        /// Получение значения <see cref="bool"/> по индексу.
        /// </summary>
        /// <param name="index">Индекс колонки.</param>
        public bool GetBoolean(int index)
        {
            Contract.Requires<ArgumentOutOfRangeException>(index >= 0 && index < Fields.Count);

            return (bool)GetValue(index);
        }

        /// <summary>
        /// Получение значения <see cref="DateTime"/> по имени колонки.
        /// </summary>
        /// <param name="columnName">Имя колонка.</param>
        public DateTime GetDateTime(string columnName)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(columnName));

            return (DateTime)GetValue(columnName);
        }

        /// <summary>
        /// Получение значения <see cref="DateTime"/> по индексу.
        /// </summary>
        /// <param name="index">Индекс колонки.</param>
        public DateTime GetDateTime(int index)
        {
            Contract.Requires<ArgumentOutOfRangeException>(index >= 0 && index < Fields.Count);

            return (DateTime)GetValue(index);
        }

        /// <summary>
        /// Получение значения <see cref="char"/> по имени колонки.
        /// </summary>
        /// <param name="columnName">Имя колонка.</param>
        public char GetChar(string columnName)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(columnName));

            return (char)GetValue(columnName);
        }

        /// <summary>
        /// Получение значения <see cref="char"/> по индексу.
        /// </summary>
        /// <param name="index">Индекс колонки.</param>
        public char GetChar(int index)
        {
            Contract.Requires<ArgumentOutOfRangeException>(index >= 0 && index < Fields.Count);

            return (char)GetValue(index);
        }

        /// <summary>
        /// Получение значения по имени колонки.
        /// </summary>
        /// <param name="columnName">Имя колонки.</param>
        public OneSValue this[string columnName]
        {
            get
            {
                Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(columnName));
                Contract.Ensures(Contract.Result<OneSValue>() != null);

                return GetValue(columnName);
            }
        }

        /// <summary>
        /// Получение значения по индексу поля.
        /// </summary>
        /// <param name="index">Индекс поля.</param>
        public OneSValue this[int index]
        {
            get
            {
                Contract.Requires<ArgumentOutOfRangeException>(index >= 0 && index < Fields.Count);
                Contract.Ensures(Contract.Result<OneSValue>() != null);

                return GetValue(index);
            }
        }

        // TODO: Прототип
        public dynamic AsDynamic()
        {
            throw new NotImplementedException();
        }

        /// <summary>Коллекция полей.</summary>
        public IList<string> Fields
        {
            get { return _fields; }
        }
        
        /// <summary>Копирование значений в буфер.</summary>
        /// <param name="values">Буфер значений.</param>
        /// <returns>Количество скопированных значений.</returns>
        public int GetValues(object[] values)
        {
            Contract.Requires<ArgumentNullException>(values != null);
            Contract.Ensures(Contract.Result<int>() >= 0);

            var bufferSize = Math.Min(values.Length, _values.Count);
            var buffer = _values.Take(bufferSize).Select(v => v.ToObject()).ToArray();
            
            Array.Copy(buffer, values, bufferSize);

            return bufferSize;
        }

        /// <summary>Копирование значений в буфер.</summary>
        /// <param name="values">Буфер значений.</param>
        /// <returns>Количество скопированных значений.</returns>
        public int GetValues(OneSValue[] values)
        {
            Contract.Requires<ArgumentNullException>(values != null);
            Contract.Ensures(Contract.Result<int>() >= 0);

            if (values.Length < _values.Count)
            {
                var buffer = new OneSValue[_values.Count];
                GetValues(buffer);

                var copiesCount = values.Length;
                Array.Copy(buffer, values, copiesCount);

                return copiesCount;
            }

            _values.CopyTo(values, 0);

            return _values.Count;
        }

        /// <summary>Получение значения по индексу поля.</summary>
        /// <param name="index">Индекс поля.</param>
        public OneSValue GetValue(int index)
        {
            Contract.Requires<ArgumentOutOfRangeException>(index >= 0 && index < Fields.Count);
            Contract.Ensures(Contract.Result<OneSValue>() != null);

            return _values[index];
        }

        /// <summary>Получение значения по имени колонки.</summary>
        /// <param name="columnName">Имя колонки.</param>
        public OneSValue GetValue(string columnName)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(columnName));
            Contract.Ensures(Contract.Result<OneSValue>() != null);

            var index = _fields.IndexOf(columnName);
            if (index >= 0)
                return _values[index];

            throw new KeyNotFoundException(string.Format(
                "Колонки с именем \"{0}\" нет в записи.", columnName));
        }
    }
}