using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>Фабрика коннекторов к информационной БД 1C.</summary>
    [ContractClass(typeof(IOneSConnectorFactoryContract))]
    public interface IOneSConnectorFactory
    {
        /// <summary>Создание соединения в зависимости от версии.</summary>
        /// <param name="version">Версия.</param>
        /// <returns>Возвращает объект коннектора к информационной БД определенной версии.</returns>
        /// <exception cref="ArgumentNullException">В случае, если значение <paramref name="version"/> было пустым.</exception>
        /// <exception cref="InvalidOperationException">В случае, если фабрика не может создать экземпляр коннектора заданной версии.</exception>
        IOneSConnector Create(string version);
    }
}
