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

        /// <summary>
        /// Является ли тип, типом данных который имеет соответствие объекту 1С.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool IsDataType(Type type);

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

        /// <summary>
        /// Является ли тип, типом данных который имеет соответствие объекту 1С.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsDataType(Type type)
        {
            Contract.Requires<ArgumentNullException>(type != null);

            return false;
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
