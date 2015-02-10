using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Data.DataReading
{
    /// <summary>
    /// Поставщик конвертеров сырых значений.
    /// </summary>
    [ContractClass(typeof(RawValueConverterProviderContract))]
    internal interface IRawValueConverterProvider
    {
        /// <summary>
        /// Получение конвертера сырых значений.
        /// </summary>
        /// <param name="targetType">Целевой тип.</param>
        /// <returns></returns>
        Func<object, object> GetRawValueConverter(Type targetType);
    }

    [ContractClassFor(typeof(IRawValueConverterProvider))]
    internal abstract class RawValueConverterProviderContract : IRawValueConverterProvider
    {
        Func<object, object> IRawValueConverterProvider.GetRawValueConverter(Type targetType)
        {
            Contract.Requires<ArgumentNullException>(targetType != null);

            return null;
        }
    }
}
