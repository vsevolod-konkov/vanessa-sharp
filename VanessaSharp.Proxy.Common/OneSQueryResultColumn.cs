namespace VanessaSharp.Proxy.Common
{
    /// <summary>
    /// Типизированная обертка над объектов 1С
    /// являющегося колонкой результата запроса.
    /// </summary>
    public sealed class OneSQueryResultColumn
        : OneSContextBoundObject, IQueryResultColumn
    {
        /// <summary>Конструктор.</summary>
        /// <param name="comObject">RCW-обертка над 1С-объектом.</param>
        /// <param name="proxyWrapper">Обертыватель 1С-объектов.</param>
        /// <param name="globalContext">Глобальный контекст.</param>
        public OneSQueryResultColumn(object comObject, IOneSProxyWrapper proxyWrapper, OneSGlobalContext globalContext) 
            : base(comObject, proxyWrapper, globalContext)
        {}

        /// <summary>Наименование колонки.</summary>
        public string Name
        {
            get { return DynamicProxy.Name; }
        }

        /// <summary>Тип колонки.</summary>
        public ITypeDescription ValueType
        {
            get { return DynamicProxy.ValueType; }
        }
    }
}
