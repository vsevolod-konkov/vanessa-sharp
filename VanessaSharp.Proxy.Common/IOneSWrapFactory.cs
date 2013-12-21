using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>
    /// Фабрика создания оберток над
    /// объектами 1С.
    /// </summary>
    [ContractClass(typeof(IOneSWrapFactoryContract))]
    public interface IOneSWrapFactory
    {
        /// <summary>Создание обертки.</summary>
        /// <param name="comObject">RCW-обертка над объектом 1С.</param>
        /// <param name="parameters">Параметры для создания обертки.</param>
        OneSObject CreateWrap(object comObject, CreateWrapParameters parameters);
    }

    [ContractClassFor(typeof(IOneSWrapFactory))]
    internal abstract class IOneSWrapFactoryContract
        : IOneSWrapFactory
    {
        /// <summary>Создание обертки.</summary>
        /// <param name="comObject">RCW-обертка над объектом 1С.</param>
        /// <param name="parameters">Параметры для создания обертки.</param>
        OneSObject IOneSWrapFactory.CreateWrap(object comObject, CreateWrapParameters parameters)
        {
            Contract.Requires<ArgumentNullException>(comObject != null);
            Contract.Requires<ArgumentNullException>(parameters != null);
            Contract.Ensures(Contract.Result<OneSObject>() != null);

            return default(OneSObject);
        }
    }
}
