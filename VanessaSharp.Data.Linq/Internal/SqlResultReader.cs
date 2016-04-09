namespace VanessaSharp.Data.Linq.Internal
{
    /// <summary>
    /// Стандартная реализация <see cref="ISqlResultReader"/>.
    /// </summary>
    internal sealed class SqlResultReader : ISqlResultReader
    {
        /// <summary>Читатель табличных данных 1С.</summary>
        private readonly OneSDataReader _dataReader;

        /// <summary>Конструктор.</summary>
        /// <param name="dataReader">Читатель табличных данных 1С.</param>
        public SqlResultReader(OneSDataReader dataReader)
        {
            _dataReader = dataReader;
        }

        /// <summary>
        /// Выполняет определяемые приложением задачи, связанные с удалением, высвобождением или сбросом неуправляемых ресурсов.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            _dataReader.Close();
        }

        /// <summary>Передвигает курсор на следующую позицию.</summary>
        public bool Read()
        {
            return _dataReader.Read();
        }

        /// <summary>Количество колонок.</summary>
        public int FieldCount
        {
            get { return _dataReader.FieldCount; }
        }

        /// <summary>Конвертер значений.</summary>
        public IValueConverter ValueConverter
        {
            get { return _dataReader.ValueConverter; }
        }

        /// <summary>
        /// Получение имени поля.
        /// </summary>
        /// <param name="fieldIndex">Индекс поля.</param>
        public string GetFieldName(int fieldIndex)
        {
            return _dataReader.GetName(fieldIndex);
        }

        /// <summary>Получение значений из записи.</summary>
        /// <param name="buffer">Буфер для записи.</param>
        public void GetValues(object[] buffer)
        {
            _dataReader.GetValues(buffer);

            for (var index = 0; index < buffer.Length; index++)
            {
                var dataReader = buffer[index] as OneSDataReader;

                if (dataReader != null)
                    buffer[index] = new SqlResultReader(dataReader);
            }
        }
    }
}
