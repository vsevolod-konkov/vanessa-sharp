using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
                var enumValueMaps = (
                                        from enumValueField in GetEnumValues(enumType)
                                        where IsSupportEnumValue(enumValueField)
                                        let value = (Enum)enumValueField.GetValue(null)
                                        let name = enumValueField.Name
                                        select 
                                            new OneSEnumValueMapInfo(value, name)
                                    )
                                    .ToArray();

                if (!enumValueMaps.Any())
                {
                    throw new InvalidOperationException(string.Format(
                        "Нет ни одного значения соответствующего объекту 1С в перечислении \"{0}\".",
                        enumType));
                }
                
                return new OneSEnumMapInfo(enumType.Name, enumValueMaps);
            }

            throw new InvalidOperationException(string.Format(
                "Не найдено перечислимое значение соответствующее объекту 1С для перечисления \"{0}\".",
                enumType));
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
        bool IOneSEnumMapInfoProvider.IsSupportEnum(Type enumType)
        {
            return IsSupportEnumType(enumType);
        }

        private static IEnumerable<FieldInfo> GetEnumValues(Type enumType)
        {
            return enumType
                .GetFields(BindingFlags.Public | BindingFlags.Static);
        }

        private static bool IsSupportEnumType(Type enumType)
        {
            return enumType.IsDefined(typeof(OneSEnumAttribute), false);
        }

        private static bool IsSupportEnumValue(FieldInfo field)
        {
            return field.IsDefined(typeof(OneSEnumValueAttribute), false);
        }
    }
}