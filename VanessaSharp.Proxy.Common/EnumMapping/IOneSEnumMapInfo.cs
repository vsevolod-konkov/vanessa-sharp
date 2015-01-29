using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Proxy.Common.EnumMapping
{
    /// <summary>
    /// Информация о соответствии перечислимого типа.
    /// </summary>
    [ContractClass(typeof(OneSEnumMapInfoContract))]
    internal interface IOneSEnumMapInfo
    {
        /// <summary>
        /// Получение объекта 1С из глобального контекста,
        /// соответствующего типу перечисления.
        /// </summary>
        /// <param name="globalContext">
        /// Глобальный контекст 1С.
        /// </param>
        OneSObject GetOneSEnumObject(OneSObject globalContext);

        /// <summary>
        /// Список информаций о соответствии
        /// объектов 1С перечислимым значениям.
        /// </summary>
        IEnumerable<IOneSEnumValueMapInfo> ValueMaps { get; }
    }

    [ContractClassFor(typeof(IOneSEnumMapInfo))]
    internal abstract class OneSEnumMapInfoContract : IOneSEnumMapInfo
    {
        OneSObject IOneSEnumMapInfo.GetOneSEnumObject(OneSObject globalContext)
        {
            Contract.Requires<ArgumentNullException>((object)globalContext != null);
            Contract.Ensures(Contract.Result<object>() != null);

            return null;
        }

        IEnumerable<IOneSEnumValueMapInfo> IOneSEnumMapInfo.ValueMaps
        {
            get
            {
                Contract.Ensures(
                    Contract.Result<IEnumerable<IOneSEnumValueMapInfo>>() != null
                    &&
                    Contract.ForAll(Contract.Result<IEnumerable<IOneSEnumValueMapInfo>>(), i => i != null)
                    );

                return null;
            }
        }
    }
}