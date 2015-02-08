using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>Маппер используемый для конвертации перечислений 1С.</summary>
    [ContractClass(typeof(OneSEnumMapperContract))]
    public interface IOneSEnumMapper
    {
        /// <summary>
        /// Конвертация RCW-обертки 1С в перечисление.
        /// </summary>
        /// <param name="comObj">Конвертируемая RCW-обертка 1С.</param>
        /// <param name="enumType">Тип перечисления.</param>
        Enum ConvertComObjectToEnum(object comObj, Type enumType);

        /// <summary>
        /// Попытка конвертации перечислимого значения
        /// в объект 1С.
        /// </summary>
        /// <param name="value">Конвертируемое значение.</param>
        /// <param name="result">Результат конвертации.</param>
        /// <returns>
        /// Возвращает <c>true</c>, если значение удалось конвертировать.
        /// </returns>
        bool TryConvertEnumToOneSObject(Enum value, out OneSObject result);
    }

    [ContractClassFor(typeof(IOneSEnumMapper))]
    internal abstract class OneSEnumMapperContract : IOneSEnumMapper
    {
        /// <summary>
        /// Конвертация RCW-обертки 1С в перечисление.
        /// </summary>
        /// <param name="comObj">Конвертируемая RCW-обертка 1С.</param>
        /// <param name="enumType">Тип перечисления.</param>
        Enum IOneSEnumMapper.ConvertComObjectToEnum(object comObj, Type enumType)
        {
            Contract.Requires<ArgumentNullException>(comObj != null);
            Contract.Requires<ArgumentNullException>(enumType != null);
            Contract.Requires<ArgumentException>(enumType.IsEnum);

            Contract.Ensures(Contract.Result<object>() != null);

            return null;
        }


        /// <summary>
        /// Попытка конвертации перечислимого значения
        /// в объект 1С.
        /// </summary>
        /// <param name="value">Конвертируемое значение.</param>
        /// <param name="result">Результат конвертации.</param>
        /// <returns>
        /// Возвращает <c>true</c>, если значение удалось конвертировать.
        /// </returns>
        bool IOneSEnumMapper.TryConvertEnumToOneSObject(Enum value, out OneSObject result)
        {
            Contract.Requires<ArgumentNullException>(value != null);
            Contract.Ensures(Contract.Result<bool>() ^ Contract.ValueAtReturn(out result) == null);

            result = null;
            return true;
        }
    }
}
