using System.Diagnostics.Contracts;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>
    /// Типизированная обертка для реализации
    /// <see cref="IQueryResultSelection"/>.
    /// </summary>
    public sealed class OneSQueryResultSelection 
        : OneSContextBoundObject, IQueryResultSelection
    {
        /// <summary>Конструктор.</summary>
        /// <param name="comObject">RCW-обертка над 1С-объектом.</param>
        /// <param name="proxyWrapper">Обертыватель 1С-объектов.</param>
        /// <param name="globalContext">Глобальный контекст.</param>
        public OneSQueryResultSelection(object comObject, IOneSProxyWrapper proxyWrapper, OneSGlobalContext globalContext)
            : base(comObject, proxyWrapper, globalContext)
        {}

        /// <summary>Чтение следующей записи.</summary>
        public bool Next()
        {
            return DynamicProxy.Next();
        }

        /// <summary>
        /// Получение значения поля.
        /// </summary>
        /// <param name="index">Индекс поля.</param>
        public object Get(int index)
        {
            return DynamicProxy.Get(index);
        }

        /// <summary>
        /// Получение значения поля по имени поля.
        /// </summary>
        /// <param name="fieldName">Имя поля.</param>
        public object Get(string fieldName)
        {
            return this[fieldName];
        }

        /// <summary>
        /// Уровень текущей записи.
        /// </summary>
        public int Level
        {
            get { return DynamicProxy.Level(); }
        }

        /// <summary>
        /// Имя группы текущей записи.
        /// </summary>
        public string Group
        {
            get { return DynamicProxy.Group(); }
        }
        
        /// <summary>
        /// Тип текущей записи.
        /// </summary>
        public SelectRecordType RecordType
        {
            get
            {
                //var recordType = DynamicProxy.RecordType();
                //return SelectRecordType.DetailRecord;

                return DynamicProxy.RecordType();
            }
        }

        /// <summary>
        /// Выборка вложенных записей для текущей записи результата.
        /// </summary>
        /// <param name="queryResultIteration">
        /// Стратегия обхода записей.
        /// </param>
        /// <param name="groupNames">
        /// Имена группировок, через запятую по которым будет производиться обход.
        /// </param>
        /// <param name="groupValues">
        /// Значения группировок, через запятую по которым будет производиться обход.
        /// </param>
        public IQueryResultSelection Choose(
            QueryResultIteration queryResultIteration,
            string groupNames,
            string groupValues)
        {
            if (groupValues == null)
            {
                if (groupNames == null)
                {
                    if (queryResultIteration == QueryResultIteration.Default)
                        return DynamicProxy.Choose();

                    return DynamicProxy.Choose(queryResultIteration);
                }

                return DynamicProxy.Choose(
                    queryResultIteration == QueryResultIteration.Default
                        ? QueryResultIteration.Linear
                        : queryResultIteration,
                    groupNames);
            }

            return DynamicProxy.Choose(
                    queryResultIteration == QueryResultIteration.Default
                        ? QueryResultIteration.Linear
                        : queryResultIteration,
                    groupNames,
                    groupValues);
        }

        /// <summary>
        /// Сброс курсора на начальную позицию.
        /// </summary>
        public void Reset()
        {
            DynamicProxy.Reset();
        }
    }
}
