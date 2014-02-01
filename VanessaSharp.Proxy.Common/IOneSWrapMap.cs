using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>
    /// Интерфейс карты соответствия
    /// запрашиваемого типа и действий связанных с ним.
    /// </summary>
    [ContractClass(typeof(IOneSWrapMapContract))]
    internal interface IOneSWrapMap
    {
        /// <summary>
        /// Получение делегата создания обертки по запрашиваемому типу.
        /// </summary>
        /// <param name="type">Запрашиваемый тип обертки.</param>
        /// <returns>Возвращает <c>null</c> если для запрашиваемого типа нет создателя.</returns>
        Func<object, IOneSProxyWrapper, OneSGlobalContext, OneSObject> GetObjectCreator(Type type);

        /// <summary>
        /// Получение имени типа в 1С соответствующего типу CLR.
        /// </summary>
        /// <param name="type">Запрашиваемый тип CLR.</param>
        /// <returns>Возвращает <c>null</c> если для запрашиваемого типа нет соответствующего имени.</returns>
        string GetOneSObjectTypeName(Type type);
    }

    /// <summary>
    /// Описание контракта для <see cref="IOneSWrapMap"/>.
    /// </summary>
    [ContractClassFor(typeof(IOneSWrapMap))]
    internal abstract class IOneSWrapMapContract : IOneSWrapMap
    {
        /// <summary>
        /// Получение делегата создания обертки по запрашиваемому типу.
        /// </summary>
        /// <param name="type">Запрашиваемый тип обертки.</param>
        /// <returns>Возвращает <c>null</c> если для запрашиваемого типа нет создателя.</returns>
        Func<object, IOneSProxyWrapper, OneSGlobalContext, OneSObject> IOneSWrapMap.GetObjectCreator(Type type)
        {
            Contract.Requires<ArgumentNullException>(type != null);

            return default(Func<object, IOneSProxyWrapper, OneSGlobalContext, OneSObject>);
        }

        /// <summary>
        /// Получение имени типа в 1С соответствующего типу CLR.
        /// </summary>
        /// <param name="type">Запрашиваемый тип CLR.</param>
        /// <returns>Возвращает <c>null</c> если для запрашиваемого типа нет соответствующего имени.</returns>
        string IOneSWrapMap.GetOneSObjectTypeName(Type type)
        {
            Contract.Requires<ArgumentNullException>(type != null);

            return default(string);
        }
    }
}
