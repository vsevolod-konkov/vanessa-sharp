using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Proxy.Common.EnumMapping
{
    /// <summary>
    /// Реализация <see cref="IOneSEnumMappingFactory"/>.
    /// </summary>
    internal sealed class OneSEnumMappingFactory : IOneSEnumMappingFactory
    {
        /// <summary>
        /// Поставщик информации по соответствию перечисления объектам 1С.
        /// </summary>
        private readonly IOneSEnumMapInfoProvider _enumMapInfoProvider;

        /// <summary>
        /// Конструктор для модульного тестирования.
        /// </summary>
        /// <param name="enumMapInfoProvider">
        /// Поставщик информации по соответствию перечисления объектам 1С.
        /// </param>
        internal OneSEnumMappingFactory(IOneSEnumMapInfoProvider enumMapInfoProvider)
        {
            Contract.Requires<ArgumentNullException>(enumMapInfoProvider != null);

            _enumMapInfoProvider = enumMapInfoProvider;
        }

        private OneSEnumMappingFactory()
            : this(OneSEnumMapInfoProvider.Default)
        {}

        /// <summary>
        /// Создание карты соответствия.
        /// </summary>
        /// <param name="enumType">Перечислимый тип.</param>
        /// <param name="globalContext">Глобалльный контекст 1С.</param>
        /// <returns></returns>
        public IOneSEnumMapping CreateMapping(Type enumType, OneSObject globalContext)
        {
            var mapInfo = _enumMapInfoProvider.GetEnumMapInfo(enumType);
            
            return OneSEnumMapping.Create(globalContext, mapInfo);
        }

        /// <summary>
        /// Есть ли соответствие
        /// перечисления с объектом 1С.
        /// </summary>
        /// <param name="enumType">Перечислимый тип.</param>
        /// <returns>
        /// Возвращает <c>true</c>, если перечислимый тип
        /// имеет соответствие 1С.
        /// В ином случае возвращается <c>false</c>.
        /// </returns>
        public bool IsSupportEnum(Type enumType)
        {
            return _enumMapInfoProvider.IsSupportEnum(enumType);
        }

        /// <summary>
        /// Экземпляр по умолчанию.
        /// </summary>
        public static OneSEnumMappingFactory Default
        {
            get { return _default; }
        }
        private static readonly OneSEnumMappingFactory _default = new OneSEnumMappingFactory();
    }
}