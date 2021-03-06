﻿namespace VanessaSharp.Proxy.Common
{
    /// <summary>
    /// Типизированная обертка над описанием типа значения 1С.
    /// </summary>
    public sealed class OneSType
        : OneSContextBoundObject, IOneSType
    {
        /// <summary>Конструктор.</summary>
        /// <param name="comObject">RCW-обертка над 1С-объектом.</param>
        /// <param name="proxyWrapper">Обертыватель 1С-объектов.</param>
        /// <param name="globalContext">Глобальный контекст.</param>
        public OneSType
            (object comObject, IOneSProxyWrapper proxyWrapper, OneSGlobalContext globalContext)
            : base(comObject, proxyWrapper, globalContext)
        {}
    }
}
