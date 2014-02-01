using System;

namespace VanessaSharp.Data
{
    /// <summary>Интерфейс конвертера значений.</summary>
    internal interface IValueConverter
    {
        /// <summary>Конвертация в строку.</summary>
        /// <param name="value">Конвертируемое значение.</param>
        string ToString(object value);

        /// <summary>
        /// Конвертация значения в <see cref="byte"/>.
        /// </summary>
        /// <param name="value">Конвертируемое значение.</param>
        byte ToByte(object value);

        /// <summary>
        /// Конвертация значения в <see cref="short"/>.
        /// </summary>
        /// <param name="value">Конвертируемое значение.</param>
        short ToInt16(object value);
        
        /// <summary>
        /// Конвертация значения в <see cref="int"/>.
        /// </summary>
        /// <param name="value">Конвертируемое значение.</param>
        int ToInt32(object value);

        /// <summary>
        /// Конвертация значения в <see cref="long"/>.
        /// </summary>
        /// <param name="value">Конвертируемое значение.</param>
        long ToInt64(object value);

        /// <summary>
        /// Конвертация значения в <see cref="float"/>.
        /// </summary>
        /// <param name="value">Конвертируемое значение.</param>
        float ToFloat(object value);
        
        /// <summary>
        /// Конвертация значения в <see cref="double"/>.
        /// </summary>
        /// <param name="value">Конвертируемое значение.</param>
        double ToDouble(object value);

        /// <summary>
        /// Конвертация значения в <see cref="decimal"/>.
        /// </summary>
        /// <param name="value">Конвертируемое значение.</param>
        decimal ToDecimal(object value);

        /// <summary>
        /// Конвертация значения в <see cref="bool"/>.
        /// </summary>
        /// <param name="value">Конвертируемое значение.</param>
        bool ToBoolean(object value);

        /// <summary>
        /// Конвертация значения в <see cref="DateTime"/>.
        /// </summary>
        /// <param name="value">Конвертируемое значение.</param>
        DateTime ToDateTime(object value);
    }
}
