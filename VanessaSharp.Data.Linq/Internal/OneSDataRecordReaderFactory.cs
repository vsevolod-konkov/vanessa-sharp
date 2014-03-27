using System;
using System.Collections.ObjectModel;

namespace VanessaSharp.Data.Linq.Internal
{
    /// <summary>
    /// Фабрика создания читателя <see cref="OneSDataRecord"/>.
    /// </summary>
    internal sealed class OneSDataRecordReaderFactory : IItemReaderFactory<OneSDataRecord>
    {
        /// <summary>Закрытый конструктор.</summary>
        private OneSDataRecordReaderFactory()
        {}
        
        /// <summary>Экземпляр по умолчанию.</summary>
        public static OneSDataRecordReaderFactory Default
        {
            get { return _default; }
        }
        private static readonly OneSDataRecordReaderFactory _default = new OneSDataRecordReaderFactory();

        /// <summary>
        /// Создание читателя элементов.
        /// </summary>
        public Func<object[], OneSDataRecord> CreateItemReader(ISqlResultReader sqlResultReader)
        {
            var fieldsCount = sqlResultReader.FieldCount;
            var fields = new string[fieldsCount];

            for (var fieldIndex = 0; fieldIndex < fieldsCount; fieldIndex++)
                fields[fieldIndex] = sqlResultReader.GetFieldName(fieldIndex);

            var recordReader = new OneSDataRecordReader(new ReadOnlyCollection<string>(fields));

            return recordReader.ReadRecord;
        }

        /// <summary>
        /// Читатель <see cref="OneSDataRecord"/>.
        /// </summary>
        private sealed class OneSDataRecordReader
        {
            /// <summary>Имена полей.</summary>
            private readonly ReadOnlyCollection<string> _fieldNames;

            /// <summary>Конструктор.</summary>
            /// <param name="fieldNames">Имена полей.</param>
            public OneSDataRecordReader(ReadOnlyCollection<string> fieldNames)
            {
                _fieldNames = fieldNames;
            }

            /// <summary>Вычитывание записи из буфера.</summary>
            public OneSDataRecord ReadRecord(object[] buffer)
            {
                return new OneSDataRecord(_fieldNames);
            }
        }
    }
}
