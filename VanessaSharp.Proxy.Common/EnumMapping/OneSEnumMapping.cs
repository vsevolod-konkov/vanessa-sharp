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
        /// Список соответствий.
        /// </summary>
        private readonly IList<EnumValueMap> _mapValues;

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

            var pairs =
                from valueMapping in valueMappings
                let valueObj = valueMapping.GetOneSEnumValue(enumObject)
                select new EnumValueMap(valueMapping.EnumValue, (OneSObject)valueObj);

            return new OneSEnumMapping(pairs.ToArray());
        }

        private OneSEnumMapping(IList<EnumValueMap> mapValues)
        {
            Contract.Requires<ArgumentNullException>(mapValues != null);

            _mapValues = mapValues;
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
            foreach (var mapValue in _mapValues)
            {
                if (mapValue.OneSProxy.Unwrap().Equals(comObject))
                {
                    enumValue = mapValue.EnumValue;
                    return true;
                }
            }

            enumValue = null;
            return false;
        }

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
        public bool TryGetOneSValue(Enum value, out OneSObject result)
        {
            foreach (var mapValue in _mapValues)
            {
                if (mapValue.EnumValue.Equals(value))
                {
                    result = mapValue.OneSObject;
                    return true;
                }
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Пара соответствия перечислимого значения и объекта 1С.
        /// </summary>
        private struct EnumValueMap
        {
            public EnumValueMap(Enum enumValue, OneSObject oneSObject)
            {
                Contract.Requires<ArgumentNullException>(enumValue != null);
                Contract.Requires<ArgumentNullException>(oneSObject != null);

                _enumValue = enumValue;
                _oneSObject = oneSObject;
            }

            public Enum EnumValue
            {
                get { return _enumValue; }
            }
            private readonly Enum _enumValue;

            public OneSObject OneSObject
            {
                get { return _oneSObject; }
            }
            private readonly OneSObject _oneSObject;

            public IOneSProxy OneSProxy
            {
                get { return OneSObject; }
            }
        }
    }
}