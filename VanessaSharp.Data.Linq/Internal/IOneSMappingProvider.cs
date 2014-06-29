using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Data.Linq.Internal
{
    /// <summary>
    /// Поставщик соответствия типам CLR
    /// данным 1С.
    /// </summary>
    [ContractClass(typeof(OneSMappingProviderContract))]
    internal interface IOneSMappingProvider
    {
        /// <summary>
        /// Проверка типа на корректность использования его в виде 
        /// типа записи данных из 1С.
        /// </summary>
        /// <param name="dataType">Тип данных.</param>
        void CheckDataType(Type dataType);

        /// <summary>Получения соответствия для типа.</summary>
        /// <param name="dataType">Тип.</param>
        OneSTypeMapping GetTypeMapping(Type dataType);
    }

    [ContractClassFor(typeof(IOneSMappingProvider))]
    internal abstract class OneSMappingProviderContract : IOneSMappingProvider
    {
        /// <summary>
        /// Проверка типа на корректность использования его в виде 
        /// типа записи данных из 1С.
        /// </summary>
        /// <param name="dataType">Тип данных.</param>
        void IOneSMappingProvider.CheckDataType(Type dataType)
        {
            Contract.Requires<ArgumentNullException>(dataType != null);
        }

        /// <summary>Получения соответствия для типа.</summary>
        /// <param name="dataType">Тип.</param>
        OneSTypeMapping IOneSMappingProvider.GetTypeMapping(Type dataType)
        {
            Contract.Requires<ArgumentNullException>(dataType != null);
            Contract.Ensures(Contract.Result<OneSTypeMapping>() != null);

            return null;
        }
    }
}
