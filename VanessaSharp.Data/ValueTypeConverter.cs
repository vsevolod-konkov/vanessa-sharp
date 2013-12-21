using System;
using System.Diagnostics.Contracts;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data
{
    /// <summary>
    /// Стандартная реализация сервиса <see cref="IValueTypeConverter"/>.
    /// </summary>
    internal sealed class ValueTypeConverter : IValueTypeConverter
    {
        /// <summary>Реализация по умолчанию.</summary>
        public static ValueTypeConverter Default
        {
            get
            {
                Contract.Ensures(Contract.Result<ValueTypeConverter>() != null);

                return _default;
            }
        }
        private static readonly ValueTypeConverter _default = new ValueTypeConverter();
        
        /// <summary>
        /// Конвертация типа 1С в тип CLR.
        /// </summary>
        /// <param name="valueType">Описание типа 1С.</param>
        public Type ConvertFrom(IValueType valueType)
        {
            return typeof(string);
        }
    }
}