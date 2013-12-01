using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>Обертка с глобальным контекстом.</summary>
    internal sealed class OneSProxyWrapperWithGlobalContext : OneSProxyWrapper
    {
        /// <summary>Глобальный контекст.</summary>
        private readonly OneSGlobalContext _globalContext;

        /// <summary>Конструктор.</summary>
        /// <param name="globalContext">Ссылка на глобальный контекст 1С.</param>
        public OneSProxyWrapperWithGlobalContext(OneSGlobalContext globalContext)
        {
            Contract.Requires<ArgumentNullException>(globalContext != null);

            _globalContext = globalContext;
        }

        /// <summary>Обертывание 1С-объекта.</summary>
        /// <param name="obj">Обертываемый объект.</param>
        /// <param name="type">Тип к которому можно привести возвращаемую обертку.</param>
        protected override OneSObject WrapOneSObject(object obj, Type type)
        {
            return new OneSContextBoundObject(obj, this, _globalContext);
        }
    }
}
