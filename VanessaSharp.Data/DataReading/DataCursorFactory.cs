using System.Diagnostics.Contracts;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.DataReading
{
    /// <summary>
    /// Реализация <see cref="IDataCursorFactory"/>.
    /// </summary>
    internal sealed class DataCursorFactory : IDataCursorFactory
    {
        /// <summary>
        /// Экземпляр по умолчанию.
        /// </summary>
        public static DataCursorFactory Default
        {
            get
            {
                Contract.Ensures(Contract.Result<DataCursorFactory>() != null);

                return _default;
            }
        }
        private static readonly DataCursorFactory _default = new DataCursorFactory();

        /// <summary>
        /// Создание курсора.
        /// </summary>
        /// <param name="fieldInfoCollection">Коллекция описания полей читателя.</param>
        /// <param name="queryResultSelection">Курсор 1С.</param>
        public IDataCursor Create(IDataReaderFieldInfoCollection fieldInfoCollection, IQueryResultSelection queryResultSelection)
        {
            return new DataCursor(fieldInfoCollection, queryResultSelection);
        }
    }
}
