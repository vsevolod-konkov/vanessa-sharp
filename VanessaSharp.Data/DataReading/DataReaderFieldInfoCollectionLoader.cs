using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.DataReading
{
    /// <summary>Загрузчик информации о полях читателя данных.</summary>
    internal static class DataReaderFieldInfoCollectionLoader
    {
        /// <summary>Загрузка коллекции с информацией о полях.</summary>
        /// <param name="queryResult">Результат запроса.</param>
        /// <param name="typeDescriptionConverter">Конвертер описателей типов 1С в типы CLR.</param>
        /// <param name="rawValueConverterProvider">Поставщик конверторов значений из 1С.</param>
        internal static ReadOnlyCollection<DataReaderFieldInfo> Load(
            IQueryResult queryResult,
            ITypeDescriptionConverter typeDescriptionConverter,
            IRawValueConverterProvider rawValueConverterProvider)
        {
            Contract.Requires<ArgumentNullException>(queryResult != null);
            Contract.Requires<ArgumentNullException>(typeDescriptionConverter != null);
            Contract.Requires<ArgumentNullException>(rawValueConverterProvider != null);
            Contract.Ensures(Contract.Result<ReadOnlyCollection<DataReaderFieldInfo>>() != null);

            using (var columns = queryResult.Columns)
            {
                var columnsCount = columns.Count;
                var buffer = new DataReaderFieldInfo[columnsCount];

                for (var index = 0; index < columnsCount; index++)
                {
                    using (var column = columns.Get(index))
                        buffer[index] = Load(column, typeDescriptionConverter, rawValueConverterProvider);
                }

                return new ReadOnlyCollection<DataReaderFieldInfo>(buffer);
            }
        }

        /// <summary>
        /// Загрузка информации о поле.
        /// </summary>
        /// <param name="column">Колонка результата 1С.</param>
        /// <param name="typeDescriptionConverter">Конвертер описателей типов 1С в типы CLR.</param>
        /// <param name="rawValueConverterProvider">Поставщик конверторов значений из 1С.</param>
        private static DataReaderFieldInfo Load(
            IQueryResultColumn column,
            ITypeDescriptionConverter typeDescriptionConverter,
            IRawValueConverterProvider rawValueConverterProvider)
        {
            using (var valueType = column.ValueType)
            {
                var clrType = typeDescriptionConverter.ConvertFrom(valueType);
                var dataTypeName = typeDescriptionConverter.GetDataTypeName(valueType);
                var converter = rawValueConverterProvider.GetRawValueConverter(clrType);
                
                return new DataReaderFieldInfo(column.Name, clrType, dataTypeName, converter);
            }
        }

        /// <summary>Создание коллекции с информацией о полях.</summary>
        /// <param name="queryResult">Результат запроса.</param>
        /// <param name="typeDescriptionConverter">Конвертер описателей типов 1С в типы CLR.</param>
        /// <param name="rawValueConverterProvider">Поставщик конверторов значений из 1С.</param>
        public static IDataReaderFieldInfoCollection Create(
            IQueryResult queryResult,
            ITypeDescriptionConverter typeDescriptionConverter,
            IRawValueConverterProvider rawValueConverterProvider)
        {
            Contract.Requires<ArgumentNullException>(queryResult != null);
            Contract.Requires<ArgumentNullException>(typeDescriptionConverter != null);
            Contract.Requires<ArgumentNullException>(rawValueConverterProvider != null);
            Contract.Ensures(Contract.Result<IDataReaderFieldInfoCollection>() != null);
            
            return new LazyDataReaderFieldInfoCollection(() =>
                new DataReaderFieldInfoFixedCollection(
                    Load(queryResult, typeDescriptionConverter, rawValueConverterProvider)));
        }
    }
}
