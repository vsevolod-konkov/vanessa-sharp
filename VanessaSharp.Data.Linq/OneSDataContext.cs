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
            get { return _queryProvider.Connection; }
        }

        /// <summary>
        /// Получение объекта запроса записей
        /// источника данных 1С <paramref name="sourceName"/>.
        /// </summary>
        /// <param name="sourceName">Источник данных 1С.</param>
        public IQueryable<OneSDataRecord> GetRecords(string sourceName)
        {
            return _queryProvider.CreateGetRecordsQuery(sourceName);
        }

        // TODO Прототип
        public IQueryable<T> Get<T>()
        {
            throw new NotImplementedException();
        }

        // TODO Прототип
        public OneSCatalogDataContext Catalogs
        {
            get { throw new NotImplementedException(); }
        }
    }
}