using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Data.DataReading
{
    /// <summary>
    /// Интерфейс поставщика записей для читателя.
    /// </summary>
    [ContractClass(typeof(DataRecordsProviderContract))]
    internal interface IDataRecordsProvider : IDisposable
    {
        /// <summary>
        /// Описание полей записей.
        /// </summary>
        IDataReaderFieldInfoCollection Fields { get; }

        /// <summary>
        /// Имеются ли записи.
        /// </summary>
        bool HasRecords { get; }

        /// <summary>
        /// Попытка создания курсора.
        /// </summary>
        /// <param name="result">Созданный курсор.</param>
        /// <returns>
        /// Возвращает <c>true</c>, если удалось создать курсор.
        /// В ином случае возвращает <c>false</c>.
        /// </returns>
        bool TryCreateCursor(out IDataCursor result);
    }

    [ContractClassFor(typeof(IDataRecordsProvider))]
    internal abstract class DataRecordsProviderContract : IDataRecordsProvider
    {

        IDataReaderFieldInfoCollection IDataRecordsProvider.Fields
        {
            get
            {
                Contract.Ensures(Contract.Result<IDataReaderFieldInfoCollection>() != null);

                return null;
            }
        }

        bool IDataRecordsProvider.HasRecords { get { return false; } }

        bool IDataRecordsProvider.TryCreateCursor(out IDataCursor result)
        {
            Contract.Ensures(
                (Contract.Result<bool>() && Contract.ValueAtReturn(out result) != null)
                ^
                (!Contract.Result<bool>() && Contract.ValueAtReturn(out result) == null)
                );

            result = null;
            return false;
        }

        public abstract void Dispose();
    }
}
