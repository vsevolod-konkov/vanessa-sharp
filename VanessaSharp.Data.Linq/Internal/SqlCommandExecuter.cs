using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Data.Linq.Internal
{
    /// <summary>
    /// Реализация <see cref="ISqlCommandExecuter"/>.
    /// </summary>
    internal sealed class SqlCommandExecuter : ISqlCommandExecuter
    {
        /// <summary>Соединение к 1С.</summary>
        private readonly OneSConnection _connection;

        /// <summary>Конструктор.</summary>
        /// <param name="connection">Соединение к 1С.</param>
        public SqlCommandExecuter(OneSConnection connection)
        {
            Contract.Requires<ArgumentNullException>(connection != null);

            _connection = connection;
        }

        /// <summary>Выполнение SQL-запроса для получения табличных данных.</summary>
        /// <param name="command">Команда SQL-запроса.</param>
        public ISqlResultReader ExecuteReader(SqlCommand command)
        {
            throw new NotImplementedException();
        }

        /// <summary>Выполенение SQL-запроса для получения скалярного значения.</summary>
        /// <param name="command">Команда SQL-запроса.</param>
        public object ExecuteScalar(SqlCommand command)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}
