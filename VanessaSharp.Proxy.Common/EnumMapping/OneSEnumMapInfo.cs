using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Proxy.Common.EnumMapping
{
    /// <summary>
    /// Реализация <see cref="IOneSEnumMapInfo"/>
    /// </summary>
    internal sealed class OneSEnumMapInfo : IOneSEnumMapInfo
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="oneSEnumName">Имя объекта в 1С.</param>
        /// <param name="valueMaps">
        /// Список информаций о соответствии
        /// объектов 1С перечислимым значениям.
        /// </param>
        public OneSEnumMapInfo(string oneSEnumName,
                               IEnumerable<IOneSEnumValueMapInfo> valueMaps)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(oneSEnumName));
            Contract.Requires<ArgumentNullException>(valueMaps != null && Contract.ForAll(valueMaps, i => i != null));
            
            _oneSEnumName = oneSEnumName;
            _valueMaps = valueMaps;
        }

        /// <summary>
        /// Имя перечисления в 1С.
        /// </summary>
        internal string OneSEnumName
        {
            get { return _oneSEnumName; }
        }
        private readonly string _oneSEnumName;

        /// <summary>
        /// Получение объекта 1С из глобального контекста,
        /// соответствующего типу перечисления.
        /// </summary>
        /// <param name="globalContext">
        /// Глобальный контекст 1С.
        /// </param>
        public OneSObject GetOneSEnumObject(OneSObject globalContext)
        {
            return (OneSObject)globalContext[_oneSEnumName];
        }

        /// <summary>
        /// Список информаций о соответствии
        /// объектов 1С перечислимым значениям.
        /// </summary>
        public IEnumerable<IOneSEnumValueMapInfo> ValueMaps
        {
            get { return _valueMaps; }
        }
        private readonly IEnumerable<IOneSEnumValueMapInfo> _valueMaps;
    }
}