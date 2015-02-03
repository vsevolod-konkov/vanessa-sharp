using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>Обертыватель объектов 1С.</summary>
    [ContractClass(typeof(IOneSProxyWrapperContract))]
    public interface IOneSProxyWrapper
    {
        /// <summary>Создание обертки над объектом.</summary>
        /// <param name="obj">Обертываемый объект.</param>
        /// <param name="type">Тип интерфейса, который должен поддерживаться оберткой.</param>
        object Wrap(object obj, Type type);

        /// <summary>
        /// Конвертация аргумента для 1С.
        /// </summary>
        /// <param name="value">Конвертируемое значение.</param>
        object ConvertToOneS(object value);
    }
}
