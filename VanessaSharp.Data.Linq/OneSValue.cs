using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Data.Linq
{
    /// <summary>
    /// Значение из 1С.
    /// </summary>
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

        private T ConvertTo<T>(Func<IValueConverter, object, T> convertAction)
        {
            Contract.Requires<ArgumentNullException>(convertAction != null);

            return convertAction(_valueConverter, _rawValue);
        }

        public static explicit operator string(OneSValue value)
        {
            Contract.Requires<ArgumentNullException>(value != null);

            return value.ConvertTo((c, v) => c.ToString(v));
        }

        public static explicit operator int(OneSValue value)
        {
            Contract.Requires<ArgumentNullException>(value != null);

            return value.ConvertTo((c, v) => c.ToInt32(v));
        }

        public static explicit operator double(OneSValue value)
        {
            Contract.Requires<ArgumentNullException>(value != null);

            return value.ConvertTo((c, v) => c.ToDouble(v));
        }

        public static explicit operator bool(OneSValue value)
        {
            Contract.Requires<ArgumentNullException>(value != null);

            return value.ConvertTo((c, v) => c.ToBoolean(v));
        }

        public static explicit operator DateTime(OneSValue value)
        {
            Contract.Requires<ArgumentNullException>(value != null);

            return value.ConvertTo((c, v) => c.ToDateTime(v));
        }

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
        /// Преобразование в <see cref="object"/>.
        /// </summary>
        public object ToObject()
        {
            return _rawValue;
        }
    }
}
