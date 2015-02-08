using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>
    /// Временный интерфейс,
    /// до рефакторинга <see cref="OneSGlobalContext"/>.
    /// Выполняющий роль сервисных методов глобального контекста.
    /// </summary>
    [ContractClass(typeof(GlobalContextServiceContract))]
    internal interface IGlobalContextService
    {
        /// <summary>
        /// Создание объекта 1С типа UUID по строке.
        /// </summary>
        /// <param name="guidString">Строквое представление идентификатора GUID.</param>
        object NewUuid(string guidString);

        /// <summary>Строковое представление объекта.</summary>
        /// <param name="obj">Объект.</param>
        string String(object obj);
    }

    [ContractClassFor(typeof(IGlobalContextService))]
    internal abstract class GlobalContextServiceContract : IGlobalContextService
    {
        object IGlobalContextService.NewUuid(string guidString)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(guidString));
            Contract.Ensures(Contract.Result<object>() != null);

            return null;
        }

        string IGlobalContextService.String(object obj)
        {
            return null;
        }
    }
}
