using System;
using System.Diagnostics.Contracts;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data
{
    /// <summary>Сервис перевода типов 1С в типы CLR.</summary>
    [ContractClass(typeof(TypeDescriptionConverterContract))]
    internal interface ITypeDescriptionConverter
    {
        /// <summary>
        /// Конвертация типа 1С в тип CLR.
        /// </summary>
        /// <param name="typeDescription">Описание типа 1С.</param>
        Type ConvertFrom(ITypeDescription typeDescription);

        /// <summary>
        /// Получение имени типа 1С.
        /// </summary>
        /// <param name="typeDescription">Описание типа 1С.</param>
        string GetDataTypeName(ITypeDescription typeDescription);
    }

    [ContractClassFor(typeof(ITypeDescriptionConverter))]
    internal abstract class TypeDescriptionConverterContract : ITypeDescriptionConverter
    {
        Type ITypeDescriptionConverter.ConvertFrom(ITypeDescription typeDescription)
        {
            Contract.Requires<ArgumentNullException>(typeDescription != null);
            Contract.Ensures(Contract.Result<Type>() != null);

            return null;
        }

        string ITypeDescriptionConverter.GetDataTypeName(ITypeDescription typeDescription)
        {
            Contract.Requires<ArgumentNullException>(typeDescription != null);
            Contract.Ensures(Contract.Result<string>() != null);

            return null;
        }
    }
}
