using System;
using System.Linq;

namespace VanessaSharp.Proxy.Common.EnumMapping
{
    /// <summary>
    /// Реализация <see cref="IOneSEnumMapInfoProvider"/>
    /// </summary>
    internal sealed class OneSEnumMapInfoProvider : IOneSEnumMapInfoProvider
    {
        private OneSEnumMapInfoProvider()
        {}
        
        /// <summary>
        /// Экземпляр по умолчанию.
        /// </summary>
        public static OneSEnumMapInfoProvider Default
        {
            get { return _default; }
        }
        private static readonly OneSEnumMapInfoProvider _default = new OneSEnumMapInfoProvider();

        /// <summary>
        /// Получение карты соответствия
        /// для перечислимого типа.
        /// </summary>
        /// <param name="enumType">
        /// Перечислимый тип, для которого необходимо дать информацию по соответстивию.
        /// </param>
        public IOneSEnumMapInfo GetEnumMapInfo(Type enumType)
        {
            if (IsSupportEnumType(enumType))
            {
                var enumValueMaps = from v in Enum.GetValues(enumType).Cast<Enum>()
                                    let name = Enum.GetName(enumType, v)
                                    select new OneSEnumValueMapInfo(v, name);

                return new OneSEnumMapInfo(enumType.Name, enumValueMaps.ToArray());
            }

            throw new InvalidOperationException(string.Format(
                "Не найдено перечислимое значение соответствующее объекту 1С для перечисления \"{0}\".",
                enumType));
        }

        private static bool IsSupportEnumType(Type enumType)
        {
            return enumType.IsDefined(typeof(OneSEnumAttribute), false);
        }
    }
}