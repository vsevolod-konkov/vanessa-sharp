using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>Маппер используемый для конвертации перечислений 1С.</summary>
    [ContractClass(typeof(OneSEnumMapperContract))]
    internal interface IOneSEnumMapper
    {
        /// <summary>
        /// Конвертация RCW-обертки 1С в перечисление.
        /// </summary>
        /// <param name="comObj">Конвертируемая RCW-обертка 1С.</param>
        /// <param name="enumType">Тип перечисления.</param>
        object ConvertComObjectToEnum(object comObj, Type enumType);
    }

    [ContractClassFor(typeof(IOneSEnumMapper))]
    internal abstract class OneSEnumMapperContract : IOneSEnumMapper
    {
        /// <summary>
        /// Конвертация RCW-обертки 1С в перечисление.
        /// </summary>
        /// <param name="comObj">Конвертируемая RCW-обертка 1С.</param>
        /// <param name="enumType">Тип перечисления.</param>
        object IOneSEnumMapper.ConvertComObjectToEnum(object comObj, Type enumType)
        {
            Contract.Requires<ArgumentNullException>(comObj != null);
            Contract.Requires<ArgumentNullException>(enumType != null);
            Contract.Requires<ArgumentException>(enumType.IsEnum);

            Contract.Ensures(Contract.Result<object>() != null);

            return null;
        }
    }
}
