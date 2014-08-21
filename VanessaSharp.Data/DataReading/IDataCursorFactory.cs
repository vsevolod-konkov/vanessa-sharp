using System;
using System.Diagnostics.Contracts;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.DataReading
{
    /// <summary>
    /// Фабрика создания курсора <see cref="IDataCursor"/>.
    /// </summary>
    [ContractClass(typeof(DataCursorFactoryContract))]
    internal interface IDataCursorFactory
    {
        /// <summary>
        /// Создание курсора.
        /// </summary>
        /// <param name="fieldInfoCollection">Коллекция описания полей читателя.</param>
        /// <param name="queryResultSelection">Курсор 1С.</param>
        IDataCursor Create(IDataReaderFieldInfoCollection fieldInfoCollection, IQueryResultSelection queryResultSelection);
    }

    [ContractClassFor(typeof(IDataCursorFactory))]
    internal abstract class DataCursorFactoryContract : IDataCursorFactory
    {
        /// <summary>
        /// Создание курсора.
        /// </summary>
        /// <param name="fieldInfoCollection">Коллекция описания полей читателя.</param>
        /// <param name="queryResultSelection">Курсор 1С.</param>
        IDataCursor IDataCursorFactory.Create(IDataReaderFieldInfoCollection fieldInfoCollection, IQueryResultSelection queryResultSelection)
        {
            Contract.Requires<ArgumentNullException>(fieldInfoCollection != null);
            Contract.Requires<ArgumentNullException>(queryResultSelection != null);
            Contract.Ensures(Contract.Result<IDataCursor>() != null);

            return null;
        }
    }
}
