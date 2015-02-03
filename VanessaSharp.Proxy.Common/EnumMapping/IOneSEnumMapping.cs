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

        /// <summary>
        /// Попытка получения объекта 1С соответствующего
        /// значению перечисления.
        /// </summary>
        /// <param name="value">Перечислимое значение.</param>
        /// <param name="result">Соответствующий 1С-объект.</param>
        /// <returns>
        /// Возвращает <c>true</c>, если соответствие найдено.
        /// В ином случае возвращается <c>false</c>.
        /// </returns>
        bool TryGetOneSValue(Enum value, out OneSObject result);
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

        bool IOneSEnumMapping.TryGetOneSValue(Enum value, out OneSObject result)
        {
            Contract.Requires<ArgumentNullException>(value != null);
            Contract.Ensures(
                (Contract.Result<bool>() && Contract.ValueAtReturn(out result) != null)
                || !Contract.Result<bool>()
                );

            result = null;
            return false;
        }
    }
}