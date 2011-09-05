using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>Объект 1C, привязанный к глобальному контексту.</summary>
    public sealed class OneSContextBoundObject : OneSObject, IGlobalContextBound
    {
        /// <summary>Конструктор.</summary>
        /// <param name="comObject">RCW-обертка над 1С-объектом.</param>
        /// <param name="proxyWrapper">Обертыватель 1С-объектов.</param>
        /// <param name="globalContext">Глобальный контекст.</param>
        public OneSContextBoundObject(object comObject, IOneSProxyWrapper proxyWrapper, OneSGlobalContext globalContext)
            : base(comObject, proxyWrapper)
        {
            Contract.Requires<ArgumentNullException>(globalContext != null);

            _globalContext = globalContext;
        }

        /// <summary>Глобальный контекст.</summary>
        public OneSGlobalContext GlobalContext
        {
            get { return _globalContext; }
        }
        private readonly OneSGlobalContext _globalContext;

        /// <summary>Глобальный контекст.</summary>
        IGlobalContext IGlobalContextBound.GlobalContext
        {
            get { return GlobalContext; }
        }
    }
}
