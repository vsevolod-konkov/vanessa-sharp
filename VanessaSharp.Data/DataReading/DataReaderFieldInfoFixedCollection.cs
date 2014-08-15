using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace VanessaSharp.Data.DataReading
{
    /// <summary>
    /// Реализация <see cref="IDataReaderFieldInfoCollection"/>
    /// с фиксированной коллекцией информации о полях читателя данных.
    /// </summary>
    internal sealed class DataReaderFieldInfoFixedCollection : IDataReaderFieldInfoCollection
    {
        /// <summary>Коллекция полей читателя данных.</summary>
        private readonly ReadOnlyCollection<DataReaderFieldInfo> _fields;

        /// <summary>Индекс по имени колонки.</summary>
        private readonly Dictionary<string, int> _indexes;

        /// <summary>
        /// Конструктор с передачей фиксированной коллекции полей читателя данных.
        /// </summary>
        /// <param name="fields">Коллекция полей читателя данных.</param>
        public DataReaderFieldInfoFixedCollection(ReadOnlyCollection<DataReaderFieldInfo> fields)
        {
            Contract.Requires<ArgumentNullException>(fields != null);

            _fields = fields;
            _indexes = _fields
                .Select((f, index) => new {FieldName = f.Name, Index = index})
                .ToDictionary(t => t.FieldName, t => t.Index);

        }

        /// <summary>Индексатор поля по индексу в коллекции.</summary>
        /// <param name="ordinal">Индекс поля.</param>
        public DataReaderFieldInfo this[int ordinal]
        {
            get { return _fields[ordinal]; }
        }

        /// <summary>
        /// Индекс поля с заданным именем <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Имя поля.</param>
        public int IndexOf(string name)
        {
            int result;
            return _indexes.TryGetValue(name, out result)
                       ? result
                       : -1;
        }

        /// <summary>Количество полей в коллекции.</summary>
        public int Count 
        {
            get { return _fields.Count; } 
        }
    }
}