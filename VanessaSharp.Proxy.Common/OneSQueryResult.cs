namespace VanessaSharp.Proxy.Common
{
    /// <summary>
    /// Типизированная обертка для реализации <see cref="IQueryResult"/>.
    /// </summary>
    public sealed class OneSQueryResult : OneSContextBoundObject, IQueryResult
    {
        /// <summary>Конструктор.</summary>
        /// <param name="comObject">RCW-обертка над 1С-объектом.</param>
        /// <param name="proxyWrapper">Обертыватель 1С-объектов.</param>
        /// <param name="globalContext">Глобальный контекст.</param>
        public OneSQueryResult(object comObject, IOneSProxyWrapper proxyWrapper, OneSGlobalContext globalContext)
            : base(comObject, proxyWrapper, globalContext)
        {}

        /// <summary>Коллекция колонок результата запроса.</summary>
        public IQueryResultColumnsCollection Columns
        {
            get { return DynamicProxy.Columns; }
        }

        /// <summary>Результат запроса пуст.</summary>
        public bool IsEmpty()
        {
            return DynamicProxy.IsEmpty();
        }

        /// <summary>Выбрать результат запроса в курсор.</summary>
        /// <param name="queryResultIteration">Стратегия перебора записей.</param>
        public IQueryResultSelection Choose(QueryResultIteration queryResultIteration)
        {
            return (queryResultIteration == QueryResultIteration.Default)
                ? DynamicProxy.Choose()
                : DynamicProxy.Choose(queryResultIteration);
        }
    }
}
