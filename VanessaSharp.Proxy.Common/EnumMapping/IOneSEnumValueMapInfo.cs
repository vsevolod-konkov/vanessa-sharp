using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Proxy.Common.EnumMapping
{
    /// <summary>
    /// Информация о соответствии объекта 1С
    /// перечислимому значению.
    /// </summary>
    [ContractClass(typeof(OneSEnumValueMapInfoContract))]
    internal interface IOneSEnumValueMapInfo
    {
        /// <summary>
        /// Перечислимое значение, для которого есть соответствие в 1С.
        /// </summary>
        Enum EnumValue { get; }

        /// <summary>
        /// Получение объекта 1С, соответствующего
        /// перечислимому значению из объекта 1С,
        /// соответствующего типу перечисления.
        /// </summary>
        /// <param name="enumObject">Объект 1С всего перечисления.</param>
        object GetOneSEnumValue(OneSObject enumObject);
    }

    [ContractClassFor(typeof(IOneSEnumValueMapInfo))]
    internal abstract class OneSEnumValueMapInfoContract : IOneSEnumValueMapInfo
    {
        Enum IOneSEnumValueMapInfo.EnumValue
        {
            get
            {
                Contract.Ensures(Contract.Result<Enum>() != null);

                return null;
            }
        }

        object IOneSEnumValueMapInfo.GetOneSEnumValue(OneSObject enumObject)
        {
            Contract.Requires<ArgumentNullException>((object)enumObject != null);
            Contract.Ensures(Contract.Result<object>() != null);

            return null;
        }
    }
}