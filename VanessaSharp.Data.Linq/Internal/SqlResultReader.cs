﻿namespace VanessaSharp.Data.Linq.Internal
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
    }
}