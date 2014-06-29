using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Data.Linq.Internal
{
    /// <summary>Читатель табличных данных результата запроса.</summary>
    [ContractClass(typeof(SqlResultReaderContract))]
    internal interface ISqlResultReader : IDisposable
    {
        /// <summary>Передвигает курсор на следующую позицию.</summary>
        bool Read();

        /// <summary>Количество колонок.</summary>
        int FieldCount { get; }

        /// <summary>Конвертер значений.</summary>
        IValueConverter ValueConverter { get; }

        /// <summary>
        /// Получение имени поля.
        /// </summary>
        /// <param name="fieldIndex">Индекс поля.</param>
        string GetFieldName(int fieldIndex);

        /// <summary>Получение значений из записи.</summary>
        /// <param name="buffer">Буфер для записи.</param>
        void GetValues(object[] buffer);
    }

    [ContractClassFor(typeof(ISqlResultReader))]
    internal abstract class SqlResultReaderContract : ISqlResultReader
    {
        public abstract void Dispose();
        public abstract bool Read();

        /// <summary>Количество колонок.</summary>
        int ISqlResultReader.FieldCount
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 0);

                return 0;
            }
        }

        /// <summary>Конвертер значений.</summary>
        IValueConverter ISqlResultReader.ValueConverter 
        { 
            get
            {
                Contract.Ensures(Contract.Result<IValueConverter>() != null);

                return null;
            } 
        }

        /// <summary>
        /// Получение имени поля.
        /// </summary>
        /// <param name="fieldIndex">Индекс поля.</param>
        string ISqlResultReader.GetFieldName(int fieldIndex)
        {
            Contract.Requires<ArgumentOutOfRangeException>(fieldIndex >= 0 && fieldIndex < ((ISqlResultReader)this).FieldCount);
            Contract.Ensures(Contract.Result<string>() != null);

            return null;
        }

        /// <summary>Получение значений из записи.</summary>
        /// <param name="buffer">Буфер для записи.</param>
        void ISqlResultReader.GetValues(object[] buffer)
        {
            Contract.Requires<ArgumentNullException>(buffer != null);
        }
    }
}
