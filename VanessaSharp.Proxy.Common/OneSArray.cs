namespace VanessaSharp.Proxy.Common
{
    /// <summary>
    /// Типизированная обертка над массивом 1С.
    /// Реализует <see cref="IOneSArray"/>.
    /// </summary>
    public class OneSArray : OneSContextBoundObject, IOneSArray
    {
        /// <summary>Конструктор.</summary>
        /// <param name="comObject">RCW-обертка над 1С-объектом.</param>
        /// <param name="proxyWrapper">Обертыватель 1С-объектов.</param>
        /// <param name="globalContext">Глобальный контекст.</param>
        public OneSArray(
            object comObject, IOneSProxyWrapper proxyWrapper, OneSGlobalContext globalContext)
            : base(comObject, proxyWrapper, globalContext)
        {}

        /// <summary>Количество элементов в массиве.</summary>
        public int Count()
        {
            return DynamicProxy.Count();
        }

        /// <summary>Типизированное получение элемента по индексу.</summary>
        /// <typeparam name="T">Возвращаемый тип.</typeparam>
        /// <param name="index">Индекс элемента.</param>
        protected T Get<T>(int index)
        {
            return DynamicProxy.Get(index);
        }

        /// <summary>Получение элемента по индексу.</summary>
        /// <param name="index">Индекс элемента.</param>
        public object Get(int index)
        {
            return Get<object>(index);
        }
    }
}