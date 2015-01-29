using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Proxy.Common.EnumMapping
{
    /// <summary>
    /// Поставщик информации по соответствию перечислений объектам 1С.
    /// </summary>
    [ContractClass(typeof(OneSEnumMapInfoProviderContract))]
    internal interface IOneSEnumMapInfoProvider
    {
        /// <summary>
        /// Получение карты соответствия
        /// для перечислимого типа.
        /// </summary>
        /// <param name="enumType">
        /// Перечислимый тип, для которого необходимо дать информацию по соответстивию.
        /// </param>
        IOneSEnumMapInfo GetEnumMapInfo(Type enumType);
    }

    [ContractClassFor(typeof(IOneSEnumMapInfoProvider))]
    internal abstract class OneSEnumMapInfoProviderContract : IOneSEnumMapInfoProvider
    {
        IOneSEnumMapInfo IOneSEnumMapInfoProvider.GetEnumMapInfo(Type enumType)
        {
            Contract.Requires<ArgumentNullException>(enumType != null);
            Contract.Ensures(Contract.Result<IOneSEnumMapInfo>() != null);

            return null;
        }
    }
}
