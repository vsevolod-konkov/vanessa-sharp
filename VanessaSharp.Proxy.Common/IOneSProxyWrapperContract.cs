using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>Описание контракта к интерфейсу <see cref="IOneSProxyWrapper"/>.</summary>
    [ContractClassFor(typeof(IOneSProxyWrapper))]
    internal abstract class IOneSProxyWrapperContract : IOneSProxyWrapper
    {
        /// <summary>Создание обертки над объектом.</summary>
        /// <param name="obj">Обертываемый объект.</param>
        /// <param name="type">Тип интерфейса, который должен поддерживаться оберткой.</param>
        object IOneSProxyWrapper.Wrap(object obj, Type type)
        {
            Contract.Requires<ArgumentNullException>(type != null);

            return null;
        }
    }
}
