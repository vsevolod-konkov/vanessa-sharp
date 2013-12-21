namespace VanessaSharp.Proxy.Common
{
    /// <summary>
    /// Типизированная обертка на объектом 1С
    /// коллекции колонок результата запроса.
    /// </summary>
    public sealed class OneSQueryResultColumnsCollection 
        : OneSContextBoundObject, IQueryResultColumnsCollection
    {
        /// <summary>Конструктор.</summary>
        /// <param name="comObject">RCW-обертка над 1С-объектом.</param>
        /// <param name="proxyWrapper">Обертыватель 1С-объектов.</param>
        /// <param name="globalContext">Глобальный контекст.</param>
        public OneSQueryResultColumnsCollection
            (object comObject, IOneSProxyWrapper proxyWrapper, OneSGlobalContext globalContext) 
            : base(comObject, proxyWrapper, globalContext)
        {}

        /// <summary>Количество колонок в коллекции.</summary>
        public int Count
        {
            get { return DynamicProxy.Count; }
        }

        /// <summary>Колонка.</summary>
        /// <param name="index">Индекс колонки.</param>
        public IQueryResultColumn Get(int index)
        {
            return DynamicProxy.Get(index);
        }
    }
}
