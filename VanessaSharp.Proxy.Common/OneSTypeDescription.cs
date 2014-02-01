namespace VanessaSharp.Proxy.Common
{
    /// <summary>
    /// Типизированная обертка над объектом 1С 
    /// описания типа
    /// для реализации <see cref="ITypeDescription"/>.
    /// </summary>
    public sealed class OneSTypeDescription : OneSContextBoundObject, ITypeDescription
    {
        /// <summary>Конструктор.</summary>
        /// <param name="comObject">RCW-обертка над 1С-объектом.</param>
        /// <param name="proxyWrapper">Обертыватель 1С-объектов.</param>
        /// <param name="globalContext">Глобальный контекст.</param>
        public OneSTypeDescription(
            object comObject, IOneSProxyWrapper proxyWrapper, OneSGlobalContext globalContext) 
            : base(comObject, proxyWrapper, globalContext)
        {}

        /// <summary>Типы в описании.</summary>
        public IOneSArray<IOneSType> Types
        {
            get { return DynamicProxy.Types; }
        }
    }
}