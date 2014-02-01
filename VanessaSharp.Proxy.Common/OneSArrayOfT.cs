namespace VanessaSharp.Proxy.Common
{
    /// <summary>Типизированная обертка над массивом 1С.</summary>
    /// <typeparam name="T">Тип элементов в массиве.</typeparam>
    public sealed class OneSArray<T> : OneSArray, IOneSArray<T>
    {
        /// <summary>Конструктор.</summary>
        /// <param name="comObject">RCW-обертка над 1С-объектом.</param>
        /// <param name="proxyWrapper">Обертыватель 1С-объектов.</param>
        /// <param name="globalContext">Глобальный контекст.</param>
        public OneSArray(
            object comObject, IOneSProxyWrapper proxyWrapper, OneSGlobalContext globalContext)
            : base(comObject, proxyWrapper, globalContext)
        {
        }

        /// <summary>Получение элемента по индексу.</summary>
        /// <param name="index">Индекс.</param>
        public new T Get(int index)
        {
            return Get<T>(index);
        }
    }
}