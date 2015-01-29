using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace VanessaSharp.Proxy.Common.EnumMapping
{
    /// <summary>
    /// Реализация <see cref="IOneSEnumMapping"/>.
    /// </summary>
    internal sealed class OneSEnumMapping : IOneSEnumMapping
    {
        /// <summary>
        /// Создание объекта по глобальному контексту и инфомации по соответствию.
        /// </summary>
        public static OneSEnumMapping Create(OneSObject globalContext, IOneSEnumMapInfo mapInfo)
        {
            var enumObject = mapInfo.GetOneSEnumObject(globalContext);

            return Create(enumObject, mapInfo.ValueMaps);
        }

        private static OneSEnumMapping Create(OneSObject enumObject, IEnumerable<IOneSEnumValueMapInfo> valueMappings)
        {
            Contract.Requires<ArgumentNullException>(enumObject != null);
            Contract.Requires<ArgumentNullException>(valueMappings != null);

            var dictionary =
                (
                    from valueMapping in valueMappings
                    let value = valueMapping.GetOneSEnumValue(enumObject)
                    select new { valueMapping.EnumValue, ComObject = ((IOneSProxy)value).Unwrap() }
                )
                    .ToDictionary(t => t.ComObject, t => t.EnumValue);

            return new OneSEnumMapping(dictionary);
        }

        private readonly Dictionary<object, Enum> _comObjectToEnumValueIndex;

        private OneSEnumMapping(Dictionary<object, Enum> comObjectToEnumValueIndex)
        {
            Contract.Requires<ArgumentNullException>(comObjectToEnumValueIndex != null);

            _comObjectToEnumValueIndex = comObjectToEnumValueIndex;
        }

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
        public bool TryGetEnumValue(object comObject, out Enum enumValue)
        {
            return _comObjectToEnumValueIndex.TryGetValue(comObject, out enumValue);
        }
    }
}