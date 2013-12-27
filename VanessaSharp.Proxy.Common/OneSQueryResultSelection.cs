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
    }
}
