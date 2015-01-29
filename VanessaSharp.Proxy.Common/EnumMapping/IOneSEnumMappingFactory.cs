using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Proxy.Common.EnumMapping
{
    /// <summary>
    /// Фабрика для создания карты соответствия
    /// значениям перечислений и объектов 1С.
    /// </summary>
    [ContractClass(typeof(OneSEnumMappingFactoryContract))]
    internal interface IOneSEnumMappingFactory
    {
        /// <summary>
        /// Создание карты соответствия.
        /// </summary>
        /// <param name="enumType">Перечислимый тип.</param>
        /// <param name="globalContext">Глобалльный контекст 1С.</param>
        /// <returns></returns>
        IOneSEnumMapping CreateMapping(Type enumType, OneSObject globalContext);
    }

    [ContractClassFor(typeof(IOneSEnumMappingFactory))]
    internal abstract class OneSEnumMappingFactoryContract : IOneSEnumMappingFactory
    {
        IOneSEnumMapping IOneSEnumMappingFactory.CreateMapping(Type enumType, OneSObject globalContext)
        {
            Contract.Requires<ArgumentNullException>(enumType != null);
            Contract.Requires<ArgumentException>(enumType.IsEnum);
            Contract.Requires<ArgumentNullException>(globalContext != null);

            Contract.Ensures(Contract.Result<IOneSEnumMapping>() != null);

            return null;
        }
    }
}