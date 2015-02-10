using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Data.DataReading
{
    /// <summary>
    /// Реализация <see cref="IRawValueConverterProvider"/>.
    /// </summary>
    internal sealed class RawValueConverterProvider : IRawValueConverterProvider
    {
        /// <summary>
        /// Экземпляр по умолчанию.
        /// </summary>
        public static RawValueConverterProvider Default
        {
            get { return _default; }    
        }
        private static readonly RawValueConverterProvider _default 
            = new RawValueConverterProvider(OneSObjectSpecialConverter.Default);
        
        private readonly IOneSObjectSpecialConverter _objectSpecialConverter;
        
        public RawValueConverterProvider(IOneSObjectSpecialConverter objectSpecialConverter)
        {
            Contract.Requires<ArgumentNullException>(objectSpecialConverter != null);

            _objectSpecialConverter = objectSpecialConverter;
        }

        /// <summary>
        /// Получение конвертера сырых значений.
        /// </summary>
        /// <param name="targetType">Целевой тип.</param>
        public Func<object, object> GetRawValueConverter(Type targetType)
        {
            if (targetType == typeof(Guid))
                return o => _objectSpecialConverter.ToGuid(o);

            if (targetType == typeof(OneSDataReader))
                return _objectSpecialConverter.ToDataReader;

            return null;
        }
    }
}
