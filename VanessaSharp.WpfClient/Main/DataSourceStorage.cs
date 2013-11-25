using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace VanessaSharp.WpfClient.Main
{
    /// <summary>Хранилище используемых источников данных.</summary>
    internal sealed class DataSourceStorage
    {
        private static readonly XmlSerializer _xmlSerializer
            = new XmlSerializer(typeof(XmlDataSourceCollection));

        private const string DATA_SOURCES_FILENAME = "datasources.xml";

        /// <summary>Загрузка информации по источникам данных.</summary>
        public static IEnumerable<DataSourceInfo> Load()
        {
            var xmlCollection = LoadXmlDataSourceCollection();
            if (xmlCollection == null || xmlCollection.DataSources == null)
                return new DataSourceInfo[0];

            var dbProviderNamesMap = GetDbProviderNamesMap();

            return from xmlDataSource in xmlCollection.DataSources
                   select new DataSourceInfo(
                       xmlDataSource.DbProviderInvariantName,
                       GetDbProviderName(dbProviderNamesMap, xmlDataSource.DbProviderInvariantName),
                       xmlDataSource.DbConnectionString);
        }

        /// <summary>Загрузка xml-кортежей из файла.</summary>
        private static XmlDataSourceCollection LoadXmlDataSourceCollection()
        {
            if (!File.Exists(DATA_SOURCES_FILENAME))
                return null;

            try
            {
                using (var textReader = File.OpenText(DATA_SOURCES_FILENAME))
                {
                    return (XmlDataSourceCollection)_xmlSerializer.Deserialize(textReader);
                }
            }
            catch
            {
                // TODO перейти на log4net
            }

            return null;
        }

        /// <summary>
        /// Получение карты соответствия инвариантного имени провайдера ADO.Net
        /// его отображаемому наименованию.
        /// </summary>
        private static IDictionary<string, string> GetDbProviderNamesMap()
        {
            return DbProviderFactories.GetFactoryClasses()
                                      .Rows
                                      .OfType<DataRow>()
                                      .ToDictionary(
                                            DbFactoriesHelper.GetDbProviderInvariantName, 
                                            DbFactoriesHelper.GetDbProviderName);
        }

        /// <summary>
        /// Получение отображаемому наименования провайдера ADO.Net.
        /// </summary>
        private static string GetDbProviderName(IDictionary<string, string> map, string invariantName)
        {
            string name;
            return (map.TryGetValue(invariantName, out name))
                       ? name
                       : invariantName;
        }

        /// <summary>Сохранение списка источников данных.</summary>
        public static void Save(IEnumerable<DataSourceInfo> dataSources)
        {
            var xmlCollection = new XmlDataSourceCollection
                {
                    DataSources = dataSources
                        .Select(ds =>
                                new XmlDataSource
                                    {
                                        DbProviderInvariantName = ds.DbProviderInvariantName,
                                        DbConnectionString = ds.DbConnectionString
                                    }).ToArray()
                };

            using (var stream = File.Open(DATA_SOURCES_FILENAME, FileMode.Create))
            using (var textWriter = new StreamWriter(stream))
            {
                _xmlSerializer.Serialize(textWriter, xmlCollection);       
            }
        }
    }
}
