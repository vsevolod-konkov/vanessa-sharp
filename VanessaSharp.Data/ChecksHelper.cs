using System;
using System.Diagnostics;

namespace VanessaSharp.Data
{
    // TODO : Перейти на контракты
    /// <summary>Вспомогательные методы проверки.</summary>
    public static class ChecksHelper
    {
        /// <summary>Проверка, того что параметер имеет значение отличное от <c>null</c>.</summary>
        /// <param name="value">Значение.</param>
        /// <param name="paramName">Имя параметра.</param>
        public static void CheckArgumentNotNull(object value, string paramName)
        {
            if (value == null)
                throw new ArgumentNullException(paramName);
        }

        /// <summary>Проверка, того что параметер имеет значение отличное от <c>null</c>.</summary>
        /// <param name="value">Значение.</param>
        /// <param name="paramName">Имя параметра.</param>
        public static void CheckArgumentNotEmpty(string value, string paramName)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException(
                    string.Format(
                        "Параметр \"{0}\" равен null или пустой строке.", 
                        paramName), 
                    paramName);
            }
        }
    }
}
