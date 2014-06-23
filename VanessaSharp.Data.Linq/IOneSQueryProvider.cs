using System;
using System.Linq;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace VanessaSharp.Data.Linq
{
    /// <summary>
    /// Интерфейс LINQ-провайдера 1С.
    /// </summary>
    [ContractClass(typeof(IOneSQueryProviderContract))]
    internal interface IOneSQueryProvider : IQueryProvider, IDisposable
    {
        /// <summary>
        /// Соединение к 1С.
        /// </summary>
        OneSConnection Connection { get; }
        
        /// <summary>
        /// Создание объекта запроса возвращающего записи из <paramref name="sourceName"/>.
        /// </summary>
        /// <param name="sourceName">Имя источника данных.</param>
        IQueryable<OneSDataRecord> CreateGetRecordsQuery(string sourceName);

        /// <summary>
        /// Создание объекта запроса возвращающего записи описываемые типом <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Тип запрашиваемых записей.</typeparam>
        IQueryable<T> CreateQueryOf<T>();
    }

    [ContractClassFor(typeof(IOneSQueryProvider))]
    internal abstract class IOneSQueryProviderContract : IOneSQueryProvider
    {
        OneSConnection IOneSQueryProvider.Connection
        {
            get
            {
                Contract.Ensures(Contract.Result<OneSConnection>() != null);
                
                return null;
            }
        }

        IQueryable<OneSDataRecord> IOneSQueryProvider.CreateGetRecordsQuery(string sourceName)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(sourceName));
            Contract.Ensures(Contract.Result<IQueryable<OneSDataRecord>>() != null);

            return null;
        }

        IQueryable<T> IOneSQueryProvider.CreateQueryOf<T>()
        {
            Contract.Ensures(Contract.Result<IQueryable<T>>() != null);

            return null;
        }

        public abstract IQueryable CreateQuery(Expression expression);
        public abstract IQueryable<TElement> CreateQuery<TElement>(Expression expression);
        public abstract object Execute(Expression expression);
        public abstract TResult Execute<TResult>(Expression expression);
        public abstract void Dispose();
    }
}
