using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Data.Linq
{
    /// <summary>
    /// Значение из 1С.
    /// </summary>
    [Serializable]
    public sealed class OneSValue
    {
        /// <summary>Сырое значение из 1С.</summary>
        private readonly object _rawValue;

        /// <summary>Конвертер значений.</summary>
        private readonly IValueConverter _valueConverter;

        /// <summary>Конструктор.</summary>
        /// <param name="rawValue">Сырое значение получаемое из 1С.</param>
        /// <param name="valueConverter">Конвертер значений.</param>
        internal OneSValue(object rawValue, IValueConverter valueConverter)
        {
            Contract.Requires<ArgumentNullException>(valueConverter != null);

            _rawValue = rawValue;
            _valueConverter = valueConverter;
        }

        /// <summary>Конвертация значения к типу <typeparamref name="T"/>.</summary>
        /// <typeparam name="T">Тип к которому конвертируется значение.</typeparam>
        /// <param name="convertAction">Действие конвертации.</param>
        /// <returns></returns>
        private T ConvertTo<T>(Func<IValueConverter, object, T> convertAction)
        {
            Contract.Requires<ArgumentNullException>(convertAction != null);

            return convertAction(_valueConverter, _rawValue);
        }

        /// <summary>Приведение к <see cref="string"/>.</summary>
        /// <param name="value">Приводимое значение.</param>
        public static explicit operator string(OneSValue value)
        {
            Contract.Requires<ArgumentNullException>(value != null);

            return value.ConvertTo((c, v) => c.ToString(v));
        }

        /// <summary>Приведение к <see cref="int"/>.</summary>
        /// <param name="value">Приводимое значение.</param>
        public static explicit operator int(OneSValue value)
        {
            Contract.Requires<ArgumentNullException>(value != null);

            return value.ConvertTo((c, v) => c.ToInt32(v));
        }

        /// <summary>Приведение к <see cref="double"/>.</summary>
        /// <param name="value">Приводимое значение.</param>
        public static explicit operator double(OneSValue value)
        {
            Contract.Requires<ArgumentNullException>(value != null);

            return value.ConvertTo((c, v) => c.ToDouble(v));
        }

        /// <summary>Приведение к <see cref="bool"/>.</summary>
        /// <param name="value">Приводимое значение.</param>
        public static explicit operator bool(OneSValue value)
        {
            Contract.Requires<ArgumentNullException>(value != null);

            return value.ConvertTo((c, v) => c.ToBoolean(v));
        }

        /// <summary>Приведение к <see cref="DateTime"/>.</summary>
        /// <param name="value">Приводимое значение.</param>
        public static explicit operator DateTime(OneSValue value)
        {
            Contract.Requires<ArgumentNullException>(value != null);

            return value.ConvertTo((c, v) => c.ToDateTime(v));
        }

        /// <summary>Приведение к <see cref="char"/>.</summary>
        /// <param name="value">Приводимое значение.</param>
        public static explicit operator char(OneSValue value)
        {
            Contract.Requires<ArgumentNullException>(value != null);

            return value.ConvertTo((c, v) => c.ToChar(v));
        }

        /// <summary>
        /// Является ли объект <c>null</c>.
        /// </summary>
        public bool IsNull
        {
            get { return ReferenceEquals(_rawValue, null); }
        }

        /// <summary>
        /// Исходное занчение возвращаемое 1С.
        /// </summary>
        public object RawValue
        {
            get { return _rawValue; }
        }
    }
}
