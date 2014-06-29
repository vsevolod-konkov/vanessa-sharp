using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace VanessaSharp.Data.Linq
{
    /// <summary>
    /// Контекст данных 1С - корневой объект Linq-провайдера.
    /// </summary>
    public class OneSDataContext : IDisposable
    {
        /// <summary>LINQ-провайдер к 1С.</summary>
        private readonly IOneSQueryProvider _queryProvider;

        /// <summary>Конструктор.</summary>
        /// <param name="queryProvider">LINQ-провайдер к 1С.</param>
        internal OneSDataContext(IOneSQueryProvider queryProvider)
        {
            Contract.Requires<ArgumentNullException>(queryProvider != null);

            _queryProvider = queryProvider;
        }
        
        /// <summary>Конструктор.</summary>
        /// <param name="connection">Соединение к 1С.</param>
        public OneSDataContext(OneSConnection connection)
            : this(new OneSQueryProvider(connection))
        {
            Contract.Requires<ArgumentNullException>(connection != null);
        }

        [ContractInvariantMethod]
        private void Invariant()
        {
            Contract.Invariant(_queryProvider != null);
        }

        /// <summary>
        /// Выполняет определяемые приложением задачи, связанные с удалением, высвобождением или сбросом неуправляемых ресурсов.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            _queryProvider.Dispose();
        }

        /// <summary>
        /// Соединение к 1С.
        /// </summary>
        public OneSConnection Connection
        {
            get
            {
                Contract.Ensures(Contract.Result<OneSConnection>() != null);

                return _queryProvider.Connection;
            }
        }

        /// <summary>
        /// Получение объекта запроса записей
        /// источника данных 1С <paramref name="sourceName"/>.
        /// </summary>
        /// <param name="sourceName">Источник данных 1С.</param>
        public IQueryable<OneSDataRecord> GetRecords(string sourceName)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(sourceName));
            Contract.Ensures(Contract.Result<IQueryable<OneSDataRecord>>() != null);
            
            return _queryProvider.CreateGetRecordsQuery(sourceName);
        }

        /// <summary>
        /// Получение объекта запроса записей
        /// источника данных 1С
        /// описанных соответствующих типу <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Тип запрашиваемых записей.</typeparam>
        public IQueryable<T> Get<T>()
        {
            Contract.Ensures(Contract.Result<IQueryable<T>>() != null);

            return _queryProvider.CreateQueryOf<T>();
        }

        // TODO Прототип
        public OneSCatalogDataContext Catalogs
        {
            get { throw new NotImplementedException(); }
        }
    }
}