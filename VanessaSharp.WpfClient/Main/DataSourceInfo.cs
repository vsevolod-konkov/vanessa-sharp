using System;
using System.Data.Common;

namespace VanessaSharp.WpfClient.Main
{
    /// <summary>Информация об источнике данных.</summary>
    internal sealed class DataSourceInfo : IEquatable<DataSourceInfo>
    {
        public DataSourceInfo(string dbProviderInvariantName, string dbProviderName, string dbConnectionString)
        {
            _dbProviderInvariantName = dbProviderInvariantName;
            _dbProviderName = dbProviderName;
            _dbConnectionString = dbConnectionString;
        }

        /// <summary>Ключ ADO.Net провайдера.</summary>
        public string DbProviderInvariantName
        {
            get { return _dbProviderInvariantName; }
        }
        private readonly string _dbProviderInvariantName;

        /// <summary>Локализованное наименование ADO.Net провайдера.</summary>
        public string DbProviderName
        {
            get { return _dbProviderName; }
        }
        private readonly string _dbProviderName;

        /// <summary>Строка подключения.</summary>
        public string DbConnectionString
        {
            get { return _dbConnectionString; }
        }
        private readonly string _dbConnectionString;

        /// <summary>Получение информации из объекта источника данных.</summary>
        /// <param name="dataSource">Источник.</param>
        public static DataSourceInfo From(DataSource dataSource)
        {
            return new DataSourceInfo(
                dataSource.DbProviderInvariantName,
                dataSource.DbProviderName,
                dataSource.DbConnectionString);
        }

        /// <summary>Создание источника данных.</summary>
        public DataSource CreateDataSource()
        {
            var dbProviderFactory = DbProviderFactories.GetFactory(DbProviderInvariantName);

            return DataSource.Create(
                DbProviderInvariantName, DbProviderName, dbProviderFactory, DbConnectionString);
        }

        public bool Equals(DataSourceInfo other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return (other != null)
                && (DbProviderInvariantName == other.DbProviderInvariantName)
                && (DbConnectionString == other.DbConnectionString);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DataSourceInfo);
        }

        public override int GetHashCode()
        {
            return DbProviderInvariantName.GetHashCode()
                ^ DbConnectionString.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("({0}) : {1}", DbProviderName, DbConnectionString);
        }
    }
}
