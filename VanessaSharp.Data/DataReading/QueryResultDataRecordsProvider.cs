using System;
using System.Diagnostics.Contracts;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.DataReading
{
    /// <summary>
    /// Реализация <see cref="IDataRecordsProvider"/>
    /// на случай, когда данные представлены в
    /// <see cref="IQueryResult"/>.
    /// </summary>
    /// <remarks>
    /// Данные могут быть представлены <see cref="IQueryResult"/> в случае получения
    /// результата запроса, или в случае если это данные табличной части. 
    /// </remarks>
    internal sealed class QueryResultDataRecordsProvider : IDataRecordsProvider
    {
        /// <summary>Результат запроса.</summary>
        private readonly IQueryResult _queryResult;

        /// <summary>Стратегия обхода записей.</summary>
        private readonly QueryResultIteration _queryResultIteration;

        /// <summary>Фабрика создания курсора.</summary>
        private readonly IDataCursorFactory _dataCursorFactory;

        /// <summary>
        /// Конструктор для модульного тестирования.
        /// </summary>
        /// <param name="queryResult">Результат запроса.</param>
        /// <param name="queryResultIteration">Стратегия обхода записей.</param>
        /// <param name="fieldInfoCollection">Коллекция с информацией по полям.</param>
        /// <param name="dataCursorFactory">Фабрика создания курсора.</param>
        internal QueryResultDataRecordsProvider(
            IQueryResult queryResult,
            QueryResultIteration queryResultIteration,
            IDataReaderFieldInfoCollection fieldInfoCollection,
            IDataCursorFactory dataCursorFactory)
        {
            Contract.Requires<ArgumentNullException>(queryResult != null);
            Contract.Requires<ArgumentNullException>(fieldInfoCollection != null);
            Contract.Requires<ArgumentNullException>(dataCursorFactory != null);
            
            _queryResult = queryResult;
            _queryResultIteration = queryResultIteration;
            _fieldInfoCollection = fieldInfoCollection;
            _dataCursorFactory = dataCursorFactory;
        }

        private QueryResultDataRecordsProvider(
            IQueryResult queryResult,
            QueryResultIteration queryResultIteration,
            ITypeDescriptionConverter typeDescriptionConverter,
            IDataCursorFactory dataCursorFactory
            )
            : this(
                queryResult,
                queryResultIteration,
                DataReaderFieldInfoCollectionLoader.Create(queryResult, typeDescriptionConverter),
                dataCursorFactory
            )
        {}

        /// <summary>
        /// Конструктор для использования.
        /// </summary>
        /// <param name="queryResult">Результат запроса.</param>
        /// <param name="queryResultIteration">Стратегия обхода записей.</param>
        public QueryResultDataRecordsProvider(
            IQueryResult queryResult, QueryResultIteration queryResultIteration)
            : this(
                queryResult,
                queryResultIteration,
                TypeDescriptionConverter.Default,
                DataCursorFactory.Default
            )
        {
            Contract.Requires<ArgumentNullException>(queryResult != null);
        }

        /// <summary>Инварианты класса.</summary>
        [ContractInvariantMethod]
        private void Invariant()
        {
            Contract.Invariant(_queryResult != null);
            Contract.Invariant(_fieldInfoCollection != null);
            Contract.Invariant(_dataCursorFactory != null);
        }

        /// <summary>Результат запроса.</summary>
        /// <remarks>
        /// Свойство для модульного тестирования.
        /// </remarks>
        internal IQueryResult QueryResult
        {
            get { return _queryResult; }
        }

        /// <summary>Стратегия обхода записей.</summary>
        /// <remarks>
        /// Свойство для модульного тестирования.
        /// </remarks>
        internal QueryResultIteration QueryResultIteration
        {
            get { return _queryResultIteration; }
        }

        /// <summary>
        /// Описание полей записей.
        /// </summary>
        public IDataReaderFieldInfoCollection Fields
        {
            get { return _fieldInfoCollection; }
        }
        private readonly IDataReaderFieldInfoCollection _fieldInfoCollection;

        /// <summary>
        /// Имеются ли записи.
        /// </summary>
        public bool HasRecords
        {
            get { return !_queryResult.IsEmpty(); }
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
            if (_queryResult.IsEmpty())
            {
                result = null;
                return false;
            }

            result = _dataCursorFactory.Create(
                _fieldInfoCollection,
                _queryResult.Choose(_queryResultIteration));

            return true;
        }

        public void Dispose()
        {
            _queryResult.Dispose();
        }
    }
}
