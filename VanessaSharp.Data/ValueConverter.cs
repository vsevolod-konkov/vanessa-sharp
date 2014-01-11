using System;

namespace VanessaSharp.Data
{
    /// <summary>Вспомогательные методы для конвертации значений.</summary>
    internal sealed class ValueConverter : IValueConverter
    {
        /// <summary>Реализация по умолчанию.</summary>
        public static IValueConverter Default
        {
            get { return _default; }
        }
        private static readonly ValueConverter _default = new ValueConverter();

        /// <summary>Конвертация в строку.</summary>
        /// <param name="value">Конвертируемое значение.</param>
        public string ToString(object value)
        {
            if (value == null)
                return null;

            var str = value as string;
            if (str != null)
                return str;

            var convertible = value as IConvertible;
            if (convertible != null)
                return convertible.ToString(null);

            var formattable = value as IFormattable;
            if (formattable != null)
                return formattable.ToString(null, null);

            return (string)value;
        }

        /// <summary>
        /// Преобразование значения к требуемому типу
        /// <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Требуемый тип.</typeparam>
        /// <param name="value">Преобразуемое значение.</param>
        /// <param name="convertMethod">Метод коневертации.</param>
        private static T To<T>(object value, Func<IConvertible, T> convertMethod)
            where T : struct
        {
            try
            {
                if (!(value is T))
                {
                    var convertible = value as IConvertible;
                    if (convertible != null)
                        return convertMethod(convertible);
                }

                return (T) value;
            }
            catch (OverflowException e)
            {
                throw new InvalidCastException(
                    string.Format(
                        "Невозможно привести значение \"{0}\" к типу \"{1}\" по причине: {2}", 
                        value, typeof(T).FullName, e.Message), 
                    e);
            }
            catch (NullReferenceException)
            {
                throw new InvalidCastException(
                    string.Format(
                        "Невозможно привести пустое значение к типу \"{0}\".", typeof(T).FullName));
            }
        }

        /// <summary>
        /// Конвертация значения в <see cref="byte"/>.
        /// </summary>
        /// <param name="value">Конвертируемое значение.</param>
        public byte ToByte(object value)
        {
            return To(value, c => c.ToByte(null));
        }

        /// <summary>
        /// Конвертация значения в <see cref="short"/>.
        /// </summary>
        /// <param name="value">Конвертируемое значение.</param>
        public short ToInt16(object value)
        {
            return To(value, c => c.ToInt16(null));
        }

        /// <summary>
        /// Конвертация значения в <see cref="Int32"/>.
        /// </summary>
        /// <param name="value">Конвертируемое значение.</param>
        public int ToInt32(object value)
        {
            return To(value, c => c.ToInt32(null));
        }

        /// <summary>
        /// Конвертация значения в <see cref="long"/>.
        /// </summary>
        /// <param name="value">Конвертируемое значение.</param>
        public long ToInt64(object value)
        {
            return To(value, c => c.ToInt64(null));
        }

        /// <summary>
        /// Конвертация значения в <see cref="float"/>.
        /// </summary>
        /// <param name="value">Конвертируемое значение.</param>
        public float ToFloat(object value)
        {
            return To(value, c => c.ToSingle(null));
        }

        /// <summary>
        /// Конвертация значения в <see cref="double"/>.
        /// </summary>
        /// <param name="value">Конвертируемое значение.</param>
        public double ToDouble(object value)
        {
            return To(value, c => c.ToDouble(null));
        }
    }
}
