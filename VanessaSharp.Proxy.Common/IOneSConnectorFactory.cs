using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>Фабрика коннекторов к информационной БД 1C.</summary>
    [ContractClass(typeof(IOneSConnectorFactoryContract))]
    public interface IOneSConnectorFactory
    {
        /// <summary>Создание коннектора.</summary>
        /// <param name="creationParams">Параметры-рекомендации создания коннектора.</param>
        /// <returns>Возвращает объект коннектора к информационной БД определенной версии.</returns>
        /// <exception cref="InvalidOperationException">В случае, если фабрика не смогла создать экземпляр коннектора.</exception>
        IOneSConnector Create(ConnectorCreationParams creationParams);
    }
}
