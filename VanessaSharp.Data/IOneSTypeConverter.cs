using System;
using System.Diagnostics.Contracts;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data
{
    /// <summary>Конвертер типов 1С в типы CLR.</summary>
    [ContractClass(typeof(OneSTypeConverterContract))]
    internal interface IOneSTypeConverter
    {
        /// <summary>Попытка конвертации типа 1С в тип CLR.</summary>
        /// <param name="oneSType">Тип 1С.</param>
        /// <returns>
        /// Возвращает <c>null</c>
        /// в случае если не удается найти подходящий тип CLR.
        /// </returns>
        Type TryConvertFrom(IOneSType oneSType);

        /// <summary>
        /// Получение имени типа.
        /// </summary>
        /// <param name="oneSType">Тип 1С.</param>
        string GetTypeName(IOneSType oneSType);
    }

    [ContractClassFor(typeof(IOneSTypeConverter))]
    internal abstract class OneSTypeConverterContract : IOneSTypeConverter
    {
        Type IOneSTypeConverter.TryConvertFrom(IOneSType oneSType)
        {
            Contract.Requires<ArgumentNullException>(oneSType != null);

            return null;
        }

        string IOneSTypeConverter.GetTypeName(IOneSType oneSType)
        {
            Contract.Requires<ArgumentNullException>(oneSType != null);
            Contract.Ensures(Contract.Result<string>() != null);

            return null;
        }
    }
}
