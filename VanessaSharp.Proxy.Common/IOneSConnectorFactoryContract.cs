using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>Описание контракта для <see cref="IOneSConnectorFactory"/>.</summary>
    [ContractClassFor(typeof(IOneSConnectorFactory))]
    internal abstract class IOneSConnectorFactoryContract : IOneSConnectorFactory
    {
        /// <summary>Создание коннектора.</summary>
        /// <param name="creationParams">Параметры-рекомендации создания коннектора.</param>
        /// <returns>Возвращает объект коннектора к информационной БД определенной версии.</returns>
        /// <exception cref="InvalidOperationException">В случае, если фабрика не смогла создать экземпляр коннектора.</exception>
        IOneSConnector IOneSConnectorFactory.Create(ConnectorCreationParams creationParams)
        {
            Contract.Ensures(Contract.Result<IOneSConnector>() != null);

            return null;
        }
    }
}
