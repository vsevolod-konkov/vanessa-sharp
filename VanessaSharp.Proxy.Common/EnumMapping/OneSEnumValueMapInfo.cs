using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Proxy.Common.EnumMapping
{
    /// <summary>
    /// Реализация <see cref="OneSEnumValueMapInfo"/>.
    /// </summary>
    internal sealed class OneSEnumValueMapInfo : IOneSEnumValueMapInfo
    {
        /// <summary>Конструктор.</summary>
        /// <param name="enumValue">
        /// Перечислимое значение, для которого есть соответствие в 1С.
        /// </param>
        /// <param name="oneSPropertyName">
        /// Свойство 1С объекта перечисления,
        /// соответствующее значению перечисления.
        /// </param>
        public OneSEnumValueMapInfo(
            Enum enumValue,
            string oneSPropertyName)
        {
            Contract.Requires<ArgumentNullException>(enumValue != null);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(oneSPropertyName));

            _enumValue = enumValue;
            _oneSPropertyName = oneSPropertyName;
        }

        /// <summary>
        /// Свойство 1С объекта перечисления,
        /// соответствующее значению перечисления.
        /// </summary>
        internal string OneSPropertyName
        {
            get { return _oneSPropertyName; }
        }
        private readonly string _oneSPropertyName;

        /// <summary>
        /// Перечислимое значение, для которого есть соответствие в 1С.
        /// </summary>
        public Enum EnumValue
        {
            get { return _enumValue; }
        }
        private readonly Enum _enumValue;

        /// <summary>
        /// Получение объекта 1С, соответствующего
        /// перечислимому значению из объекта 1С,
        /// соответствующего типу перечисления.
        /// </summary>
        /// <param name="enumObject">Объект 1С всего перечисления.</param>
        public object GetOneSEnumValue(OneSObject enumObject)
        {
            return enumObject[_oneSPropertyName];
        }
    }
}