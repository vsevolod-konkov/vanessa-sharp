using System;
using System.Collections.ObjectModel;
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
        /// <param name="level">Уровень данных.</param>
        /// <param name="dataType">Тип данных.</param>
        void CheckDataType(OneSDataLevel level, Type dataType);

        /// <summary>
        /// Является ли тип, типом данных который имеет соответствие объекту 1С.
        /// </summary>
        /// <param name="level">Уровень данных.</param>
        /// <param name="type">Тип данных.</param>
        /// <returns>Возвращает <c>true</c> если тип соответствует типу данных заданного уровня.</returns>
        bool IsDataType(OneSDataLevel level, Type type);

        /// <summary>Получение соответствия для типа верхнего уровня.</summary>
        /// <param name="dataType">Тип.</param>
        OneSTypeMapping GetRootTypeMapping(Type dataType);

        /// <summary>Получение соответствия для типа уровня табличной части.</summary>
        /// <param name="dataType">Тип.</param>
        ReadOnlyCollection<OneSFieldMapping> GetTablePartTypeMappings(Type dataType);
    }

    [ContractClassFor(typeof(IOneSMappingProvider))]
    internal abstract class OneSMappingProviderContract : IOneSMappingProvider
    {
        /// <summary>
        /// Проверка типа на корректность использования его в виде 
        /// типа записи данных из 1С.
        /// </summary>
        /// <param name="level">Уровень данных.</param>
        /// <param name="dataType">Тип данных.</param>
        void IOneSMappingProvider.CheckDataType(OneSDataLevel level, Type dataType)
        {
            Contract.Requires<ArgumentNullException>(dataType != null);
        }

        /// <summary>
        /// Является ли тип, типом данных который имеет соответствие объекту 1С.
        /// </summary>
        /// <param name="level">Уровень данных.</param>
        /// <param name="type">Тип данных.</param>
        /// <returns>Возвращает <c>true</c> если тип соответствует типу данных заданного уровня.</returns>
        bool IOneSMappingProvider.IsDataType(OneSDataLevel level, Type type)
        {
            Contract.Requires<ArgumentNullException>(type != null);

            return false;
        }

        /// <summary>Получение соответствия для типа верхнего уровня.</summary>
        /// <param name="dataType">Тип.</param>
        OneSTypeMapping IOneSMappingProvider.GetRootTypeMapping(Type dataType)
        {
            Contract.Requires<ArgumentNullException>(dataType != null);
            Contract.Ensures(Contract.Result<OneSTypeMapping>() != null);

            return null;
        }

        /// <summary>Получение соответствия для типа уровня табличной части.</summary>
        /// <param name="dataType">Тип.</param>
        ReadOnlyCollection<OneSFieldMapping> IOneSMappingProvider.GetTablePartTypeMappings(Type dataType)
        {
            Contract.Requires<ArgumentNullException>(dataType != null);
            Contract.Ensures(Contract.Result<ReadOnlyCollection<OneSFieldMapping>>() != null);
            
            return null;
        }
    }
}
