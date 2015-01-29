using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Proxy.Common.EnumMapping
{
    /// <summary>
    /// Соответствие значений перечисления объектам 1С.
    /// </summary>
    [ContractClass(typeof(OneSEnumMappingContract))]
    internal interface IOneSEnumMapping
    {
        /// <summary>
        /// Попытка получения значения перечисления,
        /// соответствующее COM-объекту, полученному из 1С.
        /// </summary>
        /// <param name="comObject">COM-объект 1С.</param>
        /// <param name="enumValue">
        /// Найденное перечислимое значение, соответствующее <paramref name="comObject"/>.
        /// </param>
        /// <returns>
        /// Возвращает <c>true</c>, если значение найдено.
        /// В ином случа возвращает <c>false</c>.
        /// </returns>
        bool TryGetEnumValue(object comObject, out Enum enumValue);
    }

    [ContractClassFor(typeof(IOneSEnumMapping))]
    internal abstract class OneSEnumMappingContract : IOneSEnumMapping
    {
        bool IOneSEnumMapping.TryGetEnumValue(object comObject, out Enum enumValue)
        {
            Contract.Requires<ArgumentNullException>(comObject != null);
            Contract.Ensures(
                (Contract.Result<bool>() && Contract.ValueAtReturn(out enumValue) != null)
                || !Contract.Result<bool>()
                );

            enumValue = default(Enum);
            return false;
        }
    }
}