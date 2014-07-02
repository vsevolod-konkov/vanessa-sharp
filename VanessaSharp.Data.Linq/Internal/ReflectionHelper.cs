using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace VanessaSharp.Data.Linq.Internal
{
    /// <summary>Вспомогательные методы для работы с механизмом отражения.</summary>
    internal static class ReflectionHelper
    {
        /// <summary>
        /// Определение типа члена.
        /// </summary>
        /// <param name="memberInfo">Член для которого необходимо определить тип.</param>
        public static Type GetMemberType(this MemberInfo memberInfo)
        {
            Contract.Requires<ArgumentNullException>(memberInfo != null);

            var propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo != null)
                return propertyInfo.PropertyType;

            var fieldInfo = memberInfo as FieldInfo;
            if (fieldInfo != null)
                return fieldInfo.FieldType;

            throw new InvalidOperationException(string.Format(
                "Для члена \"{0}\" невозможно определить тип.",
                memberInfo
                ));
        }
    }
}
