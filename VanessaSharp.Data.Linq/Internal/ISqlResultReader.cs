using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Data.Linq.Internal
{
    /// <summary>Читатель табличных данных результата запроса.</summary>
    [ContractClass(typeof(ISqlResultReaderContract))]
    internal interface ISqlResultReader : IDisposable
    {
        /// <summary>Передвигает курсор на следующую позицию.</summary>
        bool Read();

        /// <summary>Количество колонок.</summary>
        int FieldCount { get; }

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
    internal abstract class ISqlResultReaderContract : ISqlResultReader
    {
        public abstract void Dispose();
        public abstract bool Read();

        int ISqlResultReader.FieldCount
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 0);

                return 0;
            }
        }

        string ISqlResultReader.GetFieldName(int fieldIndex)
        {
            Contract.Requires<ArgumentOutOfRangeException>(fieldIndex >= 0 && fieldIndex < ((ISqlResultReader)this).FieldCount);
            Contract.Ensures(Contract.Result<string>() != null);

            return null;
        }

        void ISqlResultReader.GetValues(object[] buffer)
        {
            Contract.Requires<ArgumentNullException>(buffer != null);
        }
    }
}
