using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Data.DataReading
{
    /// <summary>
    /// Реализация <see cref="IDataRecordsProvider"/>
    /// для записей-потомков.
    /// </summary>
    internal sealed class DescendantsDataRecordsProvider : IDataRecordsProvider
    {
        /// <summary>Курсор данных.</summary>
        private readonly IDataCursor _dataCursor;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="fields">
        /// Описание полей записей.
        /// </param>
        /// <param name="dataCursor">
        /// Курсор данных.
        /// </param>
        public DescendantsDataRecordsProvider(
            IDataReaderFieldInfoCollection fields, IDataCursor dataCursor)
        {
            Contract.Requires<ArgumentNullException>(fields != null);
            Contract.Requires<ArgumentNullException>(dataCursor != null);
            
            _fields = fields;
            _dataCursor = dataCursor;
        }

        /// <summary>
        /// Описание полей записей.
        /// </summary>
        public IDataReaderFieldInfoCollection Fields
        {
            get { return _fields; }
        }
        private readonly IDataReaderFieldInfoCollection _fields;

        /// <summary>
        /// Имеются ли записи.
        /// </summary>
        public bool HasRecords
        {
            get { return true; }
        }

        /// <summary>
        /// Попытка создания курсора.
        /// </summary>
        /// <param name="result">Созданный курсор.</param>
        /// <returns>
        /// Возвращает <c>true</c>, если удалось создать курсор.
        /// В ином случае возвращает <c>false</c>.
        /// </returns>
        public bool TryCreateCursor(out IDataCursor result)
        {
            result = _dataCursor;

            return true;
        }

        public void Dispose()
        {
            _dataCursor.Dispose();
        }
    }
}
