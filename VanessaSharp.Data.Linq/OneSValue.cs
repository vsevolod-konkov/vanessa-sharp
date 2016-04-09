using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using VanessaSharp.Data.Linq.Internal;

namespace VanessaSharp.Data.Linq
{
    /// <summary>
    /// Значение из 1С.
    /// </summary>
    [Serializable]
    public sealed class OneSValue
    {
        /// <summary>Создание экземпляра значения.</summary>
        /// <param name="rawValue">Сырое значение получаемое из 1С.</param>
        /// <param name="valueConverter">Конвертер значений.</param>
        internal static OneSValue Create(object rawValue, IValueConverter valueConverter)
        {
            Contract.Requires<ArgumentNullException>(valueConverter != null);
            Contract.Ensures(Contract.Result<OneSValue>() != null);

            return new OneSValue(TransformRawValue(rawValue), valueConverter);
        }

        /// <summary>Преобразование сырого значения.</summary>
        private static object TransformRawValue(object rawValue)
        {
            var sqlResultReader = rawValue as ISqlResultReader;
            
            return (sqlResultReader == null)
                ? rawValue
                : new ItemEnumerable<OneSDataRecord>(sqlResultReader, OneSDataRecordReaderFactory.Default);
        }
        
        /// <summary>Конструктор.</summary>
        /// <param name="rawValue">Сырое значение получаемое из 1С.</param>
        /// <param name="valueConverter">Конвертер значений.</param>
        private OneSValue(object rawValue, IValueConverter valueConverter)
        {
            Contract.Requires<ArgumentNullException>(valueConverter != null);

            _rawValue = rawValue;
            _valueConverter = valueConverter;
        }

        /// <summary>Конвертер значений.</summary>
        internal IValueConverter ValueConverter
        {
            get { return _valueConverter; }
        }
        private readonly IValueConverter _valueConverter;

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

        /// <summary>Приведение к <see cref="char"/>.</summary>
        /// <param name="value">Приводимое значение.</param>
        public static explicit operator char(OneSValue value)
        {
            Contract.Requires<ArgumentNullException>(value != null);

            return value.ConvertTo((c, v) => c.ToChar(v));
        }

        /// <summary>Приведение к <see cref="byte"/>.</summary>
        /// <param name="value">Приводимое значение.</param>
        public static explicit operator byte(OneSValue value)
        {
            Contract.Requires<ArgumentNullException>(value != null);

            return value.ConvertTo((c, v) => c.ToByte(v));
        }

        /// <summary>Приведение к <see cref="short"/>.</summary>
        /// <param name="value">Приводимое значение.</param>
        public static explicit operator short(OneSValue value)
        {
            Contract.Requires<ArgumentNullException>(value != null);

            return value.ConvertTo((c, v) => c.ToInt16(v));
        }

        /// <summary>Приведение к <see cref="int"/>.</summary>
        /// <param name="value">Приводимое значение.</param>
        public static explicit operator int(OneSValue value)
        {
            Contract.Requires<ArgumentNullException>(value != null);

            return value.ConvertTo((c, v) => c.ToInt32(v));
        }

        /// <summary>Приведение к <see cref="long"/>.</summary>
        /// <param name="value">Приводимое значение.</param>
        public static explicit operator long(OneSValue value)
        {
            Contract.Requires<ArgumentNullException>(value != null);

            return value.ConvertTo((c, v) => c.ToInt64(v));
        }

        /// <summary>Приведение к <see cref="float"/>.</summary>
        /// <param name="value">Приводимое значение.</param>
        public static explicit operator float(OneSValue value)
        {
            Contract.Requires<ArgumentNullException>(value != null);

            return value.ConvertTo((c, v) => c.ToFloat(v));
        }

        /// <summary>Приведение к <see cref="double"/>.</summary>
        /// <param name="value">Приводимое значение.</param>
        public static explicit operator double(OneSValue value)
        {
            Contract.Requires<ArgumentNullException>(value != null);

            return value.ConvertTo((c, v) => c.ToDouble(v));
        }

        /// <summary>Приведение к <see cref="decimal"/>.</summary>
        /// <param name="value">Приводимое значение.</param>
        public static explicit operator decimal(OneSValue value)
        {
            Contract.Requires<ArgumentNullException>(value != null);

            return value.ConvertTo((c, v) => c.ToDecimal(v));
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

        /// <summary>Приведение к <see cref="Guid"/>.</summary>
        /// <param name="value">Приводимое значение.</param>
        public static explicit operator Guid(OneSValue value)
        {
            Contract.Requires<ArgumentNullException>(value != null);

            return (Guid)value.RawValue;
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
        private readonly object _rawValue;

        /// <summary>
        /// Получение записей табличной части.
        /// </summary>
        public IEnumerable<OneSDataRecord> GetTablePartRecords()
        {
            Contract.Ensures(Contract.Result<IEnumerable<OneSDataRecord>>() != null);

            try
            {
                return (IEnumerable<OneSDataRecord>)RawValue;
            }
            catch (InvalidCastException e)
            {
                throw new InvalidOperationException(
                    string.Format("Значение \"{0}\" не является записями табличной части.", RawValue), 
                    e);
            }
        }

        /// <summary>
        /// Возвращает объект <see cref="T:System.String"/>, который представляет текущий объект <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// Объект <see cref="T:System.String"/>, представляющий текущий объект <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return IsNull
                       ? "[null]"
                       : RawValue.ToString();
        }
    }
}
