using System;
using System.Data;
using System.Data.Common;

namespace VanessaSharp.WpfClient
{
    /// <summary>Источник данных.</summary>
    internal sealed class DataSource
    {
        /// <summary>Фабрика объектов ADO.Net провайдера источника данных.</summary>
        private readonly DbProviderFactory _dbProviderFactory;

        /// <summary>Подключение к источнику данных.</summary>
        private readonly DbConnection _dbConnection;

        public static DataSource Create(string dbProviderInvariantName,
                                        string dbProviderName,
                                        DbProviderFactory dbProviderFactory,
                                        string dbConnectionString)
        {
            return new DataSource(
                dbProviderInvariantName,
                dbProviderName,
                dbProviderFactory,
                CreateConnection(dbProviderName, dbProviderFactory, dbConnectionString));
        }

        private static DbConnection CreateConnection(
            string dbProviderName,
            DbProviderFactory dbProviderFactory,
            string dbConnectionString)
        {
            var dbConnection = dbProviderFactory.CreateConnection();
            if (dbConnection == null)
            {
                throw new InvalidOperationException(
                    string.Format("Провайдер источника данных \"{0}\" вернул пустое подключение.", dbProviderName));
            }

            dbConnection.ConnectionString = dbConnectionString;

            return dbConnection;
        }

        private DataSource(
            string dbProviderInvariantName,
            string dbProviderName,
            DbProviderFactory dbProviderFactory,
            DbConnection dbConnection)
        {
            _dbProviderInvariantName = dbProviderInvariantName;
            _dbProviderName = dbProviderName;
            _dbProviderFactory = dbProviderFactory;
            _dbConnection = dbConnection;
        }

        /// <summary>Ключ ADO.Net провайдера источника данных.</summary>
        public string DbProviderInvariantName
        {
            get { return _dbProviderInvariantName; }
        }
        private readonly string _dbProviderInvariantName;

        /// <summary>Наименование ADO.Net провайдера источника данных.</summary>
        public string DbProviderName
        {
            get { return _dbProviderName; }
        }
        private readonly string _dbProviderName;

        /// <summary>Строка подключения.</summary>
        public string DbConnectionString
        {
            get { return _dbConnection.ConnectionString; }
        }

        public DataTable ExecuteQuery(string queryText)
        {
            var dataAdapter = CreateDataAdapter(queryText);
            var dataTable = new DataTable();

            ConnectToDataSource(() => 
                dataAdapter.Fill(dataTable));

            return dataTable;
        }

        private DbDataAdapter CreateDataAdapter(string queryText)
        {
            var command = CreateCommand(queryText);
            
            var dataAdapter = _dbProviderFactory.CreateDataAdapter();
            if (dataAdapter == null)
            {
                throw new InvalidOperationException(
                    string.Format("Провайдер источника данных \"{0}\" вернул пустой адаптер.", _dbProviderName));
            }

            dataAdapter.SelectCommand = command;

            return dataAdapter;
        }

        private DbCommand CreateCommand(string queryText)
        {
            var command = _dbProviderFactory.CreateCommand();
            if (command == null)
            {
                throw new InvalidOperationException(
                    string.Format("Провайдер источника данных \"{0}\" вернул пустую команду.", _dbProviderName));
            }
            
            command.CommandText = queryText;
            command.CommandType = CommandType.Text;
            command.Connection = _dbConnection;

            return command;
        }

        private void ConnectToDataSource(Action action)
        {
            _dbConnection.Open();
            try
            {
                action();
            }
            finally
            {
                _dbConnection.Close();
            }
        }

        public override string ToString()
        {
            return string.Format("({0}) : {1}", _dbProviderName, _dbConnection.ConnectionString);
        }
    }
}
