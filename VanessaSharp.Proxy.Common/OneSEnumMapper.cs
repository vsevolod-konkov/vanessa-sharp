using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using VanessaSharp.Proxy.Common.EnumMapping;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>
    /// Реализация <see cref="IOneSEnumMapper"/>
    /// для конвертиации перечислений.
    /// </summary>
    internal sealed class OneSEnumMapper : IOneSEnumMapper
    {
        /// <summary>
        /// Глобальный контекст.
        /// </summary>
        private readonly OneSObject _globalContext;

        /// <summary>
        /// Фабрика для создания карты соответствия
        /// значениям перечислений и объектов 1С.
        /// </summary>
        private readonly IOneSEnumMappingFactory _enumMappingFactory;

        /// <summary>
        /// Словарь, в котором ключом является тип перечисления, а значениями карта соответствия значений перечислений.
        /// </summary>
        private readonly Dictionary<Type, IOneSEnumMapping> _typeMappings = new Dictionary<Type, IOneSEnumMapping>();

        /// <summary>
        /// Конструктор для модульного тестирования.
        /// </summary>
        /// <param name="globalContext">
        /// Глобальный контекст 1C.
        /// </param>
        /// <param name="enumMappingFactory">
        /// Фабрика для создания карты соответствия
        /// значениям перечислений и объектов 1С.
        /// </param>
        internal OneSEnumMapper(OneSObject globalContext, IOneSEnumMappingFactory enumMappingFactory)
        {
            Contract.Requires<ArgumentNullException>(globalContext != null);
            Contract.Requires<ArgumentNullException>(enumMappingFactory != null);

            _globalContext = globalContext;
            _enumMappingFactory = enumMappingFactory;
        }

        /// <summary>
        /// Конструктор для использования.
        /// </summary>
        /// <param name="globalContext">
        /// Глобальный контекст 1C.
        /// </param>
        public OneSEnumMapper(OneSGlobalContext globalContext)
            : this(globalContext, OneSEnumMappingFactory.Default)
        {
            Contract.Requires<ArgumentNullException>(globalContext != null);
        }

        /// <summary>
        /// Конвертация RCW-обертки 1С в перечисление.
        /// </summary>
        /// <param name="comObj">Конвертируемая RCW-обертка 1С.</param>
        /// <param name="enumType">Тип перечисления.</param>
        public Enum ConvertComObjectToEnum(object comObj, Type enumType)
        {
            var mapping = GetEnumMapping(enumType);
            
            Enum result;
            if (!mapping.TryGetEnumValue(comObj, out result))
            {
                throw new InvalidOperationException(string.Format(
                    "Не найдено перечислимое значение соответствующее объекту 1С для перечисления \"{0}\".",
                    enumType));
            }

            return result;
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
        public bool TryConvertEnumToOneSObject(Enum value, out OneSObject result)
        {
            var enumType = value.GetType();
            if (_enumMappingFactory.IsSupportEnum(enumType))
            {
                var mapping = GetEnumMapping(enumType);

                return mapping.TryGetOneSValue(value, out result);
            }

            result = default(OneSObject);
            return false;
        }

        private IOneSEnumMapping GetEnumMapping(Type enumType)
        {
            IOneSEnumMapping mapping;
            lock (_typeMappings)
            {
                if (_typeMappings.TryGetValue(enumType, out mapping))
                    return mapping;

                mapping = _enumMappingFactory.CreateMapping(enumType, _globalContext);
                _typeMappings.Add(enumType, mapping);
            }

            return mapping;
        }
    }
}
