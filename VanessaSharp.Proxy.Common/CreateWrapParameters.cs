using System;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>
    /// Параметры для создания типизированной обертки
    /// над объектом 1С.
    /// </summary>
    public sealed class CreateWrapParameters
    {
        /// <summary>Конструктор.</summary>
        /// <param name="requiredType">Тип который требуется, чтобы был поддерживаем оберткой.</param>
        /// <param name="proxyWrapper">Обертыватель.</param>
        /// <param name="globalContext">Глобальный контекст 1С.</param>
        public CreateWrapParameters(
            Type requiredType, IOneSProxyWrapper proxyWrapper, OneSGlobalContext globalContext)
        {
            _requiredType = requiredType;
            _proxyWrapper = proxyWrapper;
            _globalContext = globalContext;
        }
        
        /// <summary>Тип который требуется, чтобы был поддерживаем оберткой.</summary>
        public Type RequiredType { get { return _requiredType; } }
        private readonly Type _requiredType;

        /// <summary>Обертыватель.</summary>
        public IOneSProxyWrapper ProxyWrapper { get { return _proxyWrapper; } }
        private readonly IOneSProxyWrapper _proxyWrapper;

        /// <summary>Глобальный контекст 1С.</summary>
        public OneSGlobalContext GlobalContext { get { return _globalContext; } }
        private readonly OneSGlobalContext _globalContext;
    }
}
