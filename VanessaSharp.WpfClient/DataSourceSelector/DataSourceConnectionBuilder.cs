using System;
using System.Data;
using System.Data.Common;

namespace VanessaSharp.WpfClient.DataSourceSelector
{
    /// <summary>Построитель подключения к источнику данных.</summary>
    internal sealed class DataSourceConnectionBuilder
    {
        /// <summary>Описание ADO.Net провайдера.</summary>
        private readonly DataRow _dbProviderDescription;

        /// <summary>
        /// Фабрика объектов провайдера ADO.Net.
        /// </summary>
        private readonly DbProviderFactory _dbProviderFactory;

        /// <summary>
        /// Создание построителя подключения к источнику данных
        /// на основании описания.
        /// </summary>
        public static DataSourceConnectionBuilder Create(
            DataRow dbProviderDescription)
        {
            DbProviderFactory dbProviderFactory;

            try
            {
                dbProviderFactory = DbProviderFactories.GetFactory(dbProviderDescription);
            }
            catch (Exception e)
            {
                throw new ApplicationException(
                    string.Format("Ошибка при создании фабрики объектов ADO.Net провайдера \"{0}\". {1}", 
                        GetDbProviderName(dbProviderDescription), 
                        e.Message),
                    e);
            }
            
            var dbConnectionStringBuilder = dbProviderFactory.CreateConnectionStringBuilder();
            if (dbConnectionStringBuilder == null)
            {
                throw new ApplicationException(
                    string.Format(
                        "Для данного ADO.Net провайдера (\"{0}\") невозможна настройка строки подключения, так как он вернул пустой построитель строки подключения.",
                        GetDbProviderName(dbProviderDescription)));
            }

            return new DataSourceConnectionBuilder(
                dbProviderDescription,
                dbProviderFactory,
                dbConnectionStringBuilder);
        }

        private DataSourceConnectionBuilder(
            DataRow dbProviderDescription,
            DbProviderFactory dbProviderFactory,
            DbConnectionStringBuilder dbConnectionStringBuilder)
        {
            _dbProviderDescription = dbProviderDescription;
            _dbProviderFactory = dbProviderFactory;
            _dbConnectionStringBuilder = dbConnectionStringBuilder;
        }

        /// <summary>
        /// Построитель строки подключения к источнику данных.
        /// </summary>
        public DbConnectionStringBuilder DbConnectionStringBuilder
        {
            get { return _dbConnectionStringBuilder; }
        }
        private readonly DbConnectionStringBuilder _dbConnectionStringBuilder;

        /// <summary>
        /// Имя провайдера ADO.Net к источнику данных.
        /// </summary>
        private static string GetDbProviderName(DataRow dbProviderDescription)
        {
            return (string)dbProviderDescription["Name"];
        }

        /// <summary>
        /// Имя провайдера ADO.Net к источнику данных.
        /// </summary>
        private string GetDbProviderName()
        {
            return GetDbProviderName(_dbProviderDescription);
        }

        /// <summary>Создание источника данных.</summary>
        public DataSource CreateDataSource()
        {
            return DataSource.Create(
                GetDbProviderName(),
                _dbProviderFactory,
                DbConnectionStringBuilder.ToString());
        }
    }
}
