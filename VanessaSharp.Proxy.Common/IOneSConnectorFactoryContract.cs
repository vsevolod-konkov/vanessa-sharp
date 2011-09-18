using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>Описание контракта для <see cref="IOneSConnectorFactory"/>.</summary>
    [ContractClassFor(typeof(IOneSConnectorFactory))]
    internal abstract class IOneSConnectorFactoryContract : IOneSConnectorFactory
    {
        /// <summary>Создание соединения в зависимости от версии.</summary>
        /// <param name="version">Версия.</param>
        /// <returns>Возвращает объект коннектора к информационной БД определенной версии.</returns>
        /// <exception cref="ArgumentNullException">В случае, если значение <paramref name="version"/> было пустым.</exception>
        /// <exception cref="InvalidOperationException">В случае, если фабрика не может создать экземпляр коннектора заданной версии.</exception>
        IOneSConnector IOneSConnectorFactory.Create(string version)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(version));
            Contract.Ensures(Contract.Result<IOneSConnector>() != null);

            return null;
        }
    }
}
